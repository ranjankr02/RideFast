using System.ComponentModel.DataAnnotations;

namespace CabBookingApp.Models;

public class OtpRecord
{
    public int Id { get; set; }
    public int UserId { get; set; }

    [Required, StringLength(6)]
    public string Code { get; set; } = string.Empty;

    [Required, StringLength(30)]
    public string Purpose { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; }
    public DateTime CreatedAt { get; set; }
}
