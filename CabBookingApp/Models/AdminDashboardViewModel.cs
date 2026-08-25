namespace CabBookingApp.Models;

public class AdminDashboardViewModel
{
    public int TotalBookings { get; set; }
    public int TotalUsers { get; set; }
    public decimal TotalRevenue { get; set; }
    public int TodayBookings { get; set; }
    public List<Booking> RecentBookings { get; set; } = new();
    public List<AppUser> RecentUsers { get; set; } = new();
}
