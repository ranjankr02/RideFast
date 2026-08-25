using System.ComponentModel.DataAnnotations;

namespace CabBookingApp.Models;

public class AppUser
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(150), EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(10)]
    public string MobileNumber { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [Required, StringLength(20)]
    public string Role { get; set; } = "User"; // "User" | "Admin"

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
