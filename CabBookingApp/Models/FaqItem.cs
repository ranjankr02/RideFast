namespace CabBookingApp.Models;

public class FaqItem
{
    public string Category { get; set; } = string.Empty;
    public string CategoryIcon { get; set; } = string.Empty;
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public bool IsPopular { get; set; }
}
