using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RideFast.Contracts;

namespace RideFast.NotificationService;

/// <summary>
/// Consumes notification events from RabbitMQ and delivers them.
/// Ack on success; on failure the message is retried once and then dead-lettered.
/// </summary>
public class NotificationConsumer : BackgroundService
{
    private static readonly TimeSpan ReconnectDelay   = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan ProcessedIdsTtl  = TimeSpan.FromHours(24);

    private readonly RabbitMqOptions _options;
    private readonly NotificationSender _sender;
    private readonly IMemoryCache _processedIds;
    private readonly ILogger<NotificationConsumer> _logger;

    public NotificationConsumer(
        IOptions<RabbitMqOptions> options,
        NotificationSender sender,
        IMemoryCache processedIds,
        ILogger<NotificationConsumer> logger)
    {
        _options = options.Value;
        _sender = sender;
        _processedIds = processedIds;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (string.IsNullOrWhiteSpace(_options.Uri))
            throw new InvalidOperationException("RabbitMq:Uri is not configured.");

        var factory = new ConnectionFactory
        {
            Uri = new Uri(_options.Uri),
            ClientProvidedName = "ridefast-notification-service",
        };

        // Keep trying until the broker is reachable; after that the client's automatic
        // recovery re-establishes the connection and consumer.
        IConnection? connection = null;
        while (connection == null)
        {
            try
            {
                connection = await factory.CreateConnectionAsync(stoppingToken);
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogWarning("RabbitMQ not reachable ({Error}); retrying in {Delay}s",
                    ex.Message, ReconnectDelay.TotalSeconds);
                await Task.Delay(ReconnectDelay, stoppingToken);
            }
        }

        await using (connection)
        await using (var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken))
        {
            await NotificationTopology.DeclareAsync(channel, stoppingToken);
            await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 10, global: false, stoppingToken);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += (_, ea) => HandleAsync(channel, ea);
            await channel.BasicConsumeAsync(NotificationTopology.Queue, autoAck: false, consumer, stoppingToken);

            _logger.LogInformation("Consuming from {Queue}", NotificationTopology.Queue);
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
    }

    private async Task HandleAsync(IChannel channel, BasicDeliverEventArgs ea)
    {
        var messageId = ea.BasicProperties.MessageId ?? "(none)";

        // RabbitMQ delivers at-least-once; skip anything already handled so a user
        // never gets the same OTP twice.
        if (_processedIds.TryGetValue(messageId, out _))
        {
            _logger.LogInformation("Duplicate {MessageId} ignored", messageId);
            await channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
            return;
        }

        bool delivered;
        try
        {
            delivered = ea.RoutingKey switch
            {
                NotificationTopology.OtpRequestedKey =>
                    await _sender.SendOtpAsync(Deserialize<OtpRequested>(ea)),
                NotificationTopology.BookingConfirmedKey =>
                    await _sender.SendBookingConfirmationAsync(Deserialize<BookingConfirmed>(ea)),
                _ => throw new InvalidOperationException($"Unknown routing key '{ea.RoutingKey}'"),
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling {RoutingKey} {MessageId}", ea.RoutingKey, messageId);
            delivered = false;
        }

        if (delivered)
        {
            _processedIds.Set(messageId, true, ProcessedIdsTtl);
            await channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
        }
        else if (!ea.Redelivered)
        {
            _logger.LogWarning("Delivery failed for {MessageId}; requeueing for one retry", messageId);
            await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true);
        }
        else
        {
            _logger.LogError("Delivery failed again for {MessageId}; moving to {Dlq}",
                messageId, NotificationTopology.DeadLetterQueue);
            await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false);
        }
    }

    private static T Deserialize<T>(BasicDeliverEventArgs ea) =>
        JsonSerializer.Deserialize<T>(ea.Body.Span)
        ?? throw new InvalidOperationException($"Empty {typeof(T).Name} payload");
}
