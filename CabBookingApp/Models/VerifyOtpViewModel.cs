using System.ComponentModel.DataAnnotations;

namespace CabBookingApp.Models;

public class VerifyOtpViewModel
{
    public int UserId { get; set; }
    public string Purpose { get; set; } = string.Empty;
    public string? MaskedTarget { get; set; }
    public string? ReturnUrl { get; set; }
    public bool RememberMe { get; set; }

    [Required(ErrorMessage = "Please enter the OTP")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "OTP must be exactly 6 digits")]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "OTP must contain only digits")]
    [Display(Name = "OTP")]
    public string Otp { get; set; } = string.Empty;
}
