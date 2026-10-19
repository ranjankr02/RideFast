namespace CabBookingApp.Services;

public class NotificationSettings
{
    public const string Section = "Notifications";

    // Mock     — log only, OTP shown on screen in Development
    // RabbitMq — publish to the notification microservice
    public string Provider { get; set; } = "RabbitMq";

    public bool IsMock => Provider.Equals("Mock", StringComparison.OrdinalIgnoreCase);
}
