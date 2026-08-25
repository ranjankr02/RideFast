using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CabBookingApp.Models;

public class Booking
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Customer Name")]
    [StringLength(100)]
    public string CustomerName { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Mobile Number")]
    [RegularExpression(@"^\d{10}$", ErrorMessage = "Mobile number must be exactly 10 digits.")]
    public string CustomerMobileNumber { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Source { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Destination { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please select a vehicle type.")]
    [Display(Name = "Vehicle Type")]
    [StringLength(50)]
    public string VehicleType { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Travel Date & Time")]
    public DateTime TravelDateTime { get; set; }

    [Required]
    [Display(Name = "Booking Amount (₹)")]
    [Column(TypeName = "decimal(10,2)")]
    [Range(0.01, 99999.99, ErrorMessage = "Booking amount must be greater than 0.")]
    public decimal BookingAmount { get; set; }

    [Display(Name = "Booked On")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Null for guest bookings; set to logged-in user's Id when authenticated
    public int? UserId { get; set; }
}
