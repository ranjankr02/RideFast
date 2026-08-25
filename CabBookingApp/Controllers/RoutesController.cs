using CabBookingApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace CabBookingApp.Controllers;

public class RoutesController : Controller
{
    private static List<VehicleOption> Vehicles(decimal hMin, decimal hMax, decimal sMin, decimal sMax, decimal uvMin, decimal uvMax, decimal tMin, decimal tMax)
        => new()
        {
            new() { Type = "Hatchback", Icon = "bi-car-front", Description = "Budget-friendly compact car", Capacity = 4, MinPrice = hMin, MaxPrice = hMax },
            new() { Type = "Sedan",     Icon = "bi-car-front-fill", Description = "Comfortable & stylish",    Capacity = 4, MinPrice = sMin, MaxPrice = sMax },
            new() { Type = "SUV",       Icon = "bi-truck-front-fill", Description = "Spacious premium ride",  Capacity = 6, MinPrice = uvMin, MaxPrice = uvMax },
            new() { Type = "Tempo Traveller", Icon = "bi-bus-front-fill", Description = "Ideal for groups",   Capacity = 12, MinPrice = tMin, MaxPrice = tMax },
        };

    private static readonly List<RouteInfo> AllRoutes = new()
    {
        new()
        {
            Destination  = "Varanasi",
            State        = "Uttar Pradesh",
            DistanceKm   = 300,
            EstimatedTime = "5 – 6 hrs",
            PopularFor   = "Kashi Vishwanath, Ghats, Sarnath",
            Vehicles     = Vehicles(2500, 3200, 3200, 4000, 4500, 5500, 6000, 7500),
        },
        new()
        {
            Destination  = "Prayagraj",
            State        = "Uttar Pradesh",
            DistanceKm   = 350,
            EstimatedTime = "6 – 7 hrs",
            PopularFor   = "Triveni Sangam, Kumbh Mela, Anand Bhavan",
            Vehicles     = Vehicles(2800, 3500, 3800, 4800, 5200, 6500, 7000, 8500),
        },
        new()
        {
            Destination  = "Lucknow",
            State        = "Uttar Pradesh",
            DistanceKm   = 500,
            EstimatedTime = "8 – 9 hrs",
            PopularFor   = "Bara Imambara, Hazratganj, Chikan Craft",
            Vehicles     = Vehicles(4500, 5500, 5800, 7200, 7800, 9800, 10500, 13500),
        },
        new()
        {
            Destination  = "Kanpur",
            State        = "Uttar Pradesh",
            DistanceKm   = 530,
            EstimatedTime = "9 – 10 hrs",
            PopularFor   = "Phool Bagh, Kanpur Zoo, Allen Forest",
            Vehicles     = Vehicles(4800, 5800, 6200, 7800, 8200, 10500, 11000, 14500),
        },
        new()
        {
            Destination  = "Gorakhpur",
            State        = "Uttar Pradesh",
            DistanceKm   = 240,
            EstimatedTime = "4 – 5 hrs",
            PopularFor   = "Gorakhnath Temple, Ramgarh Tal, Buddha Museum",
            Vehicles     = Vehicles(2200, 2800, 2900, 3600, 4000, 5000, 5500, 7000),
        },
        new()
        {
            Destination  = "Agra",
            State        = "Uttar Pradesh",
            DistanceKm   = 800,
            EstimatedTime = "12 – 14 hrs",
            PopularFor   = "Taj Mahal, Agra Fort, Fatehpur Sikri",
            Vehicles     = Vehicles(7500, 9000, 9500, 12000, 13500, 17000, 18000, 23000),
        },
        new()
        {
            Destination  = "Delhi",
            State        = "Delhi NCR",
            DistanceKm   = 1000,
            EstimatedTime = "14 – 16 hrs",
            PopularFor   = "Red Fort, India Gate, Qutub Minar, Connaught Place",
            Vehicles     = Vehicles(9500, 11500, 12000, 15000, 16500, 21000, 22000, 28000),
        },
    };

    public IActionResult Index(string? vehicle = null)
    {
        ViewBag.SelectedVehicle = vehicle ?? "All";
        ViewBag.VehicleTypes    = new[] { "All", "Hatchback", "Sedan", "SUV", "Tempo Traveller" };
        return View(AllRoutes);
    }
}
