namespace RideFast.Contracts;

// Messages carry everything the notification service needs to deliver them,
// so it never has to read the web app's database.

public record OtpRequested(
    Guid MessageId,
    int UserId,
    string Name,
    string Email,
    string Mobile,
    string Otp,
    string Purpose,
    DateTime OccurredAtUtc);

public record BookingConfirmed(
    Guid MessageId,
    int BookingId,
    string CustomerName,
    string? Email,
    string Mobile,
    string Source,
    string Destination,
    string VehicleType,
    DateTime TravelDateTime,
    decimal Amount,
    DateTime OccurredAtUtc);
