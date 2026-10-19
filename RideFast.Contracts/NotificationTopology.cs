using RabbitMQ.Client;

namespace RideFast.Contracts;

/// <summary>
/// RabbitMQ names shared by the publisher (web app) and the consumer (notification service).
/// Both sides declare the same topology so messages are never dropped because the other
/// side has not started yet.
/// </summary>
public static class NotificationTopology
{
    public const string Exchange           = "ridefast.notifications";
    public const string Queue              = "notification-service";
    public const string DeadLetterExchange = "ridefast.notifications.dlx";
    public const string DeadLetterQueue    = "notification-service.dlq";

    public const string OtpRequestedKey     = "otp.requested";
    public const string BookingConfirmedKey = "booking.confirmed";

    // OTPs are valid for 10 minutes; an undelivered one is useless after that.
    public static readonly TimeSpan OtpMessageTtl = TimeSpan.FromMinutes(10);

    public static async Task DeclareAsync(IChannel channel, CancellationToken ct = default)
    {
        await channel.ExchangeDeclareAsync(DeadLetterExchange, ExchangeType.Fanout, durable: true, cancellationToken: ct);
        await channel.QueueDeclareAsync(DeadLetterQueue, durable: true, exclusive: false, autoDelete: false, cancellationToken: ct);
        await channel.QueueBindAsync(DeadLetterQueue, DeadLetterExchange, routingKey: "", cancellationToken: ct);

        await channel.ExchangeDeclareAsync(Exchange, ExchangeType.Topic, durable: true, cancellationToken: ct);
        await channel.QueueDeclareAsync(Queue, durable: true, exclusive: false, autoDelete: false,
            arguments: new Dictionary<string, object?> { ["x-dead-letter-exchange"] = DeadLetterExchange },
            cancellationToken: ct);
        await channel.QueueBindAsync(Queue, Exchange, OtpRequestedKey, cancellationToken: ct);
        await channel.QueueBindAsync(Queue, Exchange, BookingConfirmedKey, cancellationToken: ct);
    }
}
