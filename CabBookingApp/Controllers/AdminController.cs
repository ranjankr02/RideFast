using CabBookingApp.Data;
using CabBookingApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CabBookingApp.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly AppDbContext _context;

    public AdminController(AppDbContext context) => _context = context;

    // ── Dashboard ─────────────────────────────────────────────────────────────

    public async Task<IActionResult> Index()
    {
        var today = DateTime.Today;
        var vm = new AdminDashboardViewModel
        {
            TotalBookings  = await _context.Bookings.CountAsync(),
            TotalUsers     = await _context.Users.CountAsync(),
            TotalRevenue   = await _context.Bookings.SumAsync(b => (decimal?)b.BookingAmount) ?? 0m,
            TodayBookings  = await _context.Bookings.CountAsync(b => b.CreatedAt >= today),
            RecentBookings = await _context.Bookings
                                 .OrderByDescending(b => b.CreatedAt).Take(5).ToListAsync(),
            RecentUsers    = await _context.Users
                                 .OrderByDescending(u => u.CreatedAt).Take(5).ToListAsync(),
        };
        return View(vm);
    }

    // ── Users ─────────────────────────────────────────────────────────────────

    public async Task<IActionResult> Users()
    {
        var users = await _context.Users.OrderByDescending(u => u.CreatedAt).ToListAsync();
        return View(users);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetRole(int userId, string role)
    {
        if (role is not ("Admin" or "User"))
            return BadRequest();

        var user = await _context.Users.FindAsync(userId);
        if (user == null) return NotFound();

        // Prevent removing the last admin
        if (role == "User" && user.Role == "Admin")
        {
            int adminCount = await _context.Users.CountAsync(u => u.Role == "Admin");
            if (adminCount <= 1)
            {
                TempData["Error"] = "Cannot demote the only admin account.";
                return RedirectToAction(nameof(Users));
            }
        }

        user.Role = role;
        await _context.SaveChangesAsync();

        TempData["Success"] = $"{user.Name} is now {role}.";
        return RedirectToAction(nameof(Users));
    }

    // ── All Bookings ──────────────────────────────────────────────────────────

    public async Task<IActionResult> Bookings()
    {
        var bookings = await _context.Bookings
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();
        return View(bookings);
    }
}
