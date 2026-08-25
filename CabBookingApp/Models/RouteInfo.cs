namespace CabBookingApp.Models;

public class RouteInfo
{
    public string Destination { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public int DistanceKm { get; set; }
    public string EstimatedTime { get; set; } = string.Empty;
    public string PopularFor { get; set; } = string.Empty;
    public List<VehicleOption> Vehicles { get; set; } = new();
}

public class VehicleOption
{
    public string Type { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public decimal MinPrice { get; set; }
    public decimal MaxPrice { get; set; }
}
