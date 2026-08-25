namespace CabBookingApp.Services;

public class NotificationSettings
{
    public const string Section = "Notifications";
    public string Provider { get; set; } = "Mock"; // Mock | Email | Sms | Both
    public EmailSettings Email { get; set; } = new();
    public SmsSettings Sms { get; set; } = new();
}

public class EmailSettings
{
    public string SmtpHost { get; set; } = "smtp.gmail.com";
    public int SmtpPort { get; set; } = 587;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromAddress { get; set; } = "noreply@ridefast.in";
    public string FromName { get; set; } = "RideFast";
}

public class SmsSettings
{
    public string ApiKey { get; set; } = string.Empty; // Fast2SMS API key
}
