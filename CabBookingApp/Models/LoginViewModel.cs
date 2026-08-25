using System.ComponentModel.DataAnnotations;

namespace CabBookingApp.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "Please enter your email or mobile number.")]
    [Display(Name = "Email / Mobile Number")]
    public string EmailOrMobile { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter your password.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Remember me")]
    public bool RememberMe { get; set; }
}
