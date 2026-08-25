using CabBookingApp.Models;

namespace CabBookingApp.Services;

public interface INotificationService
{
    bool IsMock { get; }
    Task<bool> SendOtpAsync(AppUser user, string otp, string purpose);
    Task<bool> SendBookingConfirmationAsync(Booking booking, AppUser? user = null);
}
