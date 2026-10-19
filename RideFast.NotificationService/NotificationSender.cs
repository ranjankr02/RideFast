using System.Net.Http.Json;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using RideFast.Contracts;

namespace RideFast.NotificationService;

/// <summary>
/// Delivers notification messages over email (MailKit/SMTP) and SMS (Fast2SMS).
/// Returns false when nothing could be delivered so the consumer can dead-letter the message.
/// </summary>
public class NotificationSender
{
    private readonly NotificationSettings _settings;
    private readonly ILogger<NotificationSender> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public NotificationSender(
        IOptions<NotificationSettings> settings,
        ILogger<NotificationSender> logger,
        IHttpClientFactory httpClientFactory)
    {
        _settings = settings.Value;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    private bool IsMock =>
        string.IsNullOrWhiteSpace(_settings.Provider) ||
        _settings.Provider.Equals("Mock", StringComparison.OrdinalIgnoreCase);

    // ── OTP ───────────────────────────────────────────────────────────────────

    public async Task<bool> SendOtpAsync(OtpRequested msg)
    {
        if (IsMock)
        {
            _logger.LogWarning(
                "[MOCK OTP] Purpose={Purpose} | User={Name} | Email={Email} | Mobile={Mobile} | OTP={Otp}",
                msg.Purpose, msg.Name, msg.Email, msg.Mobile, msg.Otp);
            return true;
        }

        var provider = _settings.Provider.ToLowerInvariant();
        bool sent = false;

        if (provider is "email" or "both")
            sent |= await SendEmailAsync(
                msg.Email, msg.Name,
                BuildOtpEmailBody(msg.Otp, msg.Purpose, msg.Name),
                $"RideFast OTP — {msg.Purpose}");

        if (provider is "sms" or "both")
            sent |= await SendSmsAsync(
                msg.Mobile,
                $"Your RideFast OTP for {msg.Purpose} is {msg.Otp}. Valid 10 mins. Do not share. -RideFast");

        return sent;
    }

    // ── Booking confirmation ──────────────────────────────────────────────────

    public async Task<bool> SendBookingConfirmationAsync(BookingConfirmed msg)
    {
        if (IsMock)
        {
            _logger.LogWarning(
                "[MOCK BOOKING] Booking#{Id} | {Name} | {Source}→{Destination} | {Vehicle} | ₹{Amount}",
                msg.BookingId, msg.CustomerName, msg.Source,
                msg.Destination, msg.VehicleType, msg.Amount);
            return true;
        }

        var provider = _settings.Provider.ToLowerInvariant();
        bool attempted = false, sent = false;

        if ((provider is "email" or "both") && !string.IsNullOrWhiteSpace(msg.Email))
        {
            attempted = true;
            sent |= await SendEmailAsync(
                msg.Email, msg.CustomerName,
                BuildBookingEmailBody(msg),
                $"Booking Confirmed — #{msg.BookingId} | RideFast");
        }

        if (provider is "sms" or "both")
        {
            attempted = true;
            var text = $"Booking #{msg.BookingId} confirmed! " +
                       $"{msg.Source}→{msg.Destination} on " +
                       $"{msg.TravelDateTime:dd MMM, h:mmtt}. " +
                       $"Vehicle: {msg.VehicleType}. Amt: Rs.{msg.Amount:N0}. -RideFast";
            sent |= await SendSmsAsync(msg.Mobile, text);
        }

        // Guest booking with no email on an email-only setup: nothing to deliver, not a failure.
        if (!attempted)
        {
            _logger.LogInformation("Booking#{Id} has no email address; no confirmation sent", msg.BookingId);
            return true;
        }

        return sent;
    }

    // ── Email (MailKit / SMTP) ────────────────────────────────────────────────

    private async Task<bool> SendEmailAsync(string toAddress, string toName, string htmlBody, string subject)
    {
        if (string.IsNullOrWhiteSpace(_settings.Email.Username))
        {
            _logger.LogWarning("SMTP username not configured — skipping email to {Address}", toAddress);
            return false;
        }

        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_settings.Email.FromName, _settings.Email.FromAddress));
            message.To.Add(new MailboxAddress(toName, toAddress));
            message.Subject = subject;
            message.Body = new TextPart("html") { Text = htmlBody };

            using var client = new SmtpClient();
            await client.ConnectAsync(_settings.Email.SmtpHost, _settings.Email.SmtpPort, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_settings.Email.Username, _settings.Email.Password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("Email sent to {Address}", toAddress);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Address}", toAddress);
            return false;
        }
    }

    // ── SMS (Fast2SMS) ────────────────────────────────────────────────────────

    private async Task<bool> SendSmsAsync(string mobile, string message)
    {
        if (string.IsNullOrWhiteSpace(_settings.Sms.ApiKey))
        {
            _logger.LogWarning("SMS API key not configured — skipping SMS to {Mobile}", mobile);
            return false;
        }

        try
        {
            var client = _httpClientFactory.CreateClient();
            var payload = new
            {
                route    = "q",
                message,
                language = "english",
                flash    = 0,
                numbers  = mobile
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "https://www.fast2sms.com/dev/bulkV2")
            {
                Content = JsonContent.Create(payload)
            };
            request.Headers.TryAddWithoutValidation("authorization", _settings.Sms.ApiKey);

            var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                _logger.LogWarning("SMS API returned {Status} for {Mobile}", response.StatusCode, mobile);

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SMS to {Mobile}", mobile);
            return false;
        }
    }

    // ── Email templates ───────────────────────────────────────────────────────

    private static string BuildOtpEmailBody(string otp, string purpose, string name) => $"""
        <!DOCTYPE html>
        <html>
        <body style="font-family:Arial,sans-serif;background:#f0f4f8;margin:0;padding:20px">
          <div style="max-width:480px;margin:0 auto;background:#fff;border-radius:12px;overflow:hidden;box-shadow:0 4px 16px rgba(0,0,0,.1)">
            <div style="background:#0f172a;padding:24px;text-align:center">
              <span style="font-size:2rem">🚖</span>
              <h1 style="color:#fff;margin:8px 0 0;font-size:1.4rem;letter-spacing:.05em">RideFast</h1>
            </div>
            <div style="padding:32px;text-align:center">
              <h2 style="color:#1e293b;margin-bottom:6px">{purpose} Verification</h2>
              <p style="color:#64748b;margin-bottom:28px">Hi {name}, use the code below to complete your {purpose.ToLower()}.</p>
              <div style="display:inline-block;background:#f8fafc;border:2px dashed #fbbf24;border-radius:10px;padding:18px 32px;margin-bottom:24px">
                <span style="font-size:2.8rem;font-weight:700;letter-spacing:.55em;color:#0f172a;font-family:monospace">{otp}</span>
              </div>
              <p style="color:#ef4444;font-size:.88rem;margin:0 0 6px">&#x23F1; This OTP expires in <strong>10 minutes</strong>.</p>
              <p style="color:#94a3b8;font-size:.8rem;margin:0">Never share this code with anyone — including RideFast staff.</p>
            </div>
            <div style="background:#f8fafc;padding:14px;text-align:center;border-top:1px solid #e2e8f0">
              <p style="color:#94a3b8;font-size:.75rem;margin:0">&copy; 2026 RideFast &mdash; Drive safe, arrive happy.</p>
            </div>
          </div>
        </body>
        </html>
        """;

    private static string BuildBookingEmailBody(BookingConfirmed booking) => $"""
        <!DOCTYPE html>
        <html>
        <body style="font-family:Arial,sans-serif;background:#f0f4f8;margin:0;padding:20px">
          <div style="max-width:520px;margin:0 auto;background:#fff;border-radius:12px;overflow:hidden;box-shadow:0 4px 16px rgba(0,0,0,.1)">
            <div style="background:#0f172a;padding:24px;text-align:center">
              <span style="font-size:2rem">🚖</span>
              <h1 style="color:#fff;margin:8px 0 0;font-size:1.4rem;letter-spacing:.05em">RideFast</h1>
            </div>
            <div style="padding:32px">
              <h2 style="color:#16a34a;margin-bottom:4px">&#x2705; Booking Confirmed!</h2>
              <p style="color:#64748b;margin-bottom:24px">Hi {booking.CustomerName}, your ride is booked. Here are your trip details:</p>
              <table style="width:100%;border-collapse:collapse;border-radius:8px;overflow:hidden">
                <tr style="background:#f8fafc">
                  <td style="padding:10px 14px;color:#64748b;font-size:.85rem;width:38%">Booking ID</td>
                  <td style="padding:10px 14px;font-weight:700;color:#0f172a">#{booking.BookingId}</td>
                </tr>
                <tr>
                  <td style="padding:10px 14px;color:#64748b;font-size:.85rem">From</td>
                  <td style="padding:10px 14px;font-weight:600">{booking.Source}</td>
                </tr>
                <tr style="background:#f8fafc">
                  <td style="padding:10px 14px;color:#64748b;font-size:.85rem">To</td>
                  <td style="padding:10px 14px;font-weight:600">{booking.Destination}</td>
                </tr>
                <tr>
                  <td style="padding:10px 14px;color:#64748b;font-size:.85rem">Vehicle</td>
                  <td style="padding:10px 14px;font-weight:600">{booking.VehicleType}</td>
                </tr>
                <tr style="background:#f8fafc">
                  <td style="padding:10px 14px;color:#64748b;font-size:.85rem">Travel Date</td>
                  <td style="padding:10px 14px;font-weight:600">{booking.TravelDateTime:dd MMM yyyy, hh:mm tt}</td>
                </tr>
                <tr>
                  <td style="padding:10px 14px;color:#64748b;font-size:.85rem">Amount</td>
                  <td style="padding:10px 14px;font-weight:700;font-size:1.05rem;color:#16a34a">&#x20B9;{booking.Amount:N0}</td>
                </tr>
              </table>
              <div style="margin-top:24px;padding:14px;background:#fefce8;border-left:4px solid #fbbf24;border-radius:4px">
                <p style="color:#78350f;font-size:.85rem;margin:0">
                  Our driver will call you before departure. For support: <strong>+91 98765 43210</strong>
                </p>
              </div>
            </div>
            <div style="background:#f8fafc;padding:14px;text-align:center;border-top:1px solid #e2e8f0">
              <p style="color:#94a3b8;font-size:.75rem;margin:0">&copy; 2026 RideFast &mdash; Drive safe, arrive happy.</p>
            </div>
          </div>
        </body>
        </html>
        """;
}
