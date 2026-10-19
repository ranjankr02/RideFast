using CabBookingApp.Models;

namespace CabBookingApp.Services;

/// <summary>
/// Development stand-in that only logs; AuthController shows the OTP on screen in this mode.
/// </summary>
public class MockNotificationService : INotificationService
{
    private readonly ILogger<MockNotificationService> _logger;
    private readonly IHostEnvironment _env;

    public MockNotificationService(ILogger<MockNotificationService> logger, IHostEnvironment env)
    {
        _logger = logger;
        _env = env;
    }

    public bool IsMock => true;

    public Task<bool> SendOtpAsync(AppUser user, string otp, string purpose)
    {
        if (_env.IsDevelopment())
            _logger.LogWarning(
                "[DEV OTP] Purpose={Purpose} | User={Name} | Email={Email} | Mobile={Mobile} | OTP={Otp}",
                purpose, user.Name, user.Email, user.MobileNumber, otp);
        else
            _logger.LogWarning(
                "Notification provider is 'Mock' outside development — no OTP was delivered for user {UserId}.",
                user.Id);
        return Task.FromResult(true);
    }

    public Task<bool> SendBookingConfirmationAsync(Booking booking, AppUser? user = null)
    {
        _logger.LogWarning(
            "[DEV BOOKING] Booking#{Id} | {Name} | {Source}→{Destination} | {Vehicle} | ₹{Amount}",
            booking.Id, booking.CustomerName, booking.Source,
            booking.Destination, booking.VehicleType, booking.BookingAmount);
        return Task.FromResult(true);
    }
}
