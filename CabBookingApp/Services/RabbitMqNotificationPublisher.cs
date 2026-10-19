using System.Text.Json;
using CabBookingApp.Models;
using RabbitMQ.Client;
using RideFast.Contracts;

namespace CabBookingApp.Services;

/// <summary>
/// Hands notifications to the notification microservice via RabbitMQ.
/// A <c>true</c> result means the broker accepted the message (publisher confirm),
/// not that the email/SMS has been delivered yet.
/// </summary>
public class RabbitMqNotificationPublisher : INotificationService
{
    private readonly RabbitMqConnectionProvider _connections;
    private readonly ILogger<RabbitMqNotificationPublisher> _logger;

    public RabbitMqNotificationPublisher(
        RabbitMqConnectionProvider connections,
        ILogger<RabbitMqNotificationPublisher> logger)
    {
        _connections = connections;
        _logger = logger;
    }

    public bool IsMock => false;

    public Task<bool> SendOtpAsync(AppUser user, string otp, string purpose)
    {
        var message = new OtpRequested(
            Guid.NewGuid(), user.Id, user.Name, user.Email, user.MobileNumber,
            otp, purpose, DateTime.UtcNow);

        return PublishAsync(NotificationTopology.OtpRequestedKey, message, message.MessageId,
            ttl: NotificationTopology.OtpMessageTtl);
    }

    public Task<bool> SendBookingConfirmationAsync(Booking booking, AppUser? user = null)
    {
        var message = new BookingConfirmed(
            Guid.NewGuid(), booking.Id, booking.CustomerName, user?.Email, booking.CustomerMobileNumber,
            booking.Source, booking.Destination, booking.VehicleType,
            booking.TravelDateTime, booking.BookingAmount, DateTime.UtcNow);

        return PublishAsync(NotificationTopology.BookingConfirmedKey, message, message.MessageId);
    }

    private async Task<bool> PublishAsync<T>(string routingKey, T message, Guid messageId, TimeSpan? ttl = null)
    {
        try
        {
            var connection = await _connections.GetConnectionAsync();

            // Confirm tracking makes BasicPublishAsync wait for the broker's ack and throw
            // if the message is nacked or unroutable.
            await using var channel = await connection.CreateChannelAsync(
                new CreateChannelOptions(publisherConfirmationsEnabled: true,
                                         publisherConfirmationTrackingEnabled: true));

            var props = new BasicProperties
            {
                Persistent  = true,
                ContentType = "application/json",
                MessageId   = messageId.ToString(),
                Type        = typeof(T).Name,
                Timestamp   = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds()),
            };
            if (ttl is not null)
                props.Expiration = ((long)ttl.Value.TotalMilliseconds).ToString();

            await channel.BasicPublishAsync(
                NotificationTopology.Exchange, routingKey, mandatory: true,
                basicProperties: props, body: JsonSerializer.SerializeToUtf8Bytes(message));

            _logger.LogInformation("Published {Type} {MessageId}", typeof(T).Name, messageId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish {Type} {MessageId}", typeof(T).Name, messageId);
            return false;
        }
    }
}
