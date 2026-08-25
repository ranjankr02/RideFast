using System.Security.Claims;
using CabBookingApp.Data;
using CabBookingApp.Models;
using CabBookingApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CabBookingApp.Controllers;

public class BookingsController : Controller
{
    private readonly AppDbContext _context;
    private readonly INotificationService _notify;

    public BookingsController(AppDbContext context, INotificationService notify)
    {
        _context = context;
        _notify  = notify;
    }

    // ── Index — users see own bookings, admins see all ────────────────────────

    [Authorize]
    public async Task<IActionResult> Index()
    {
        var isAdmin = User.IsInRole("Admin");
        IQueryable<Booking> query = _context.Bookings.OrderByDescending(b => b.CreatedAt);

        if (!isAdmin)
        {
            var uid = CurrentUserId();
            query = uid.HasValue
                ? query.Where(b => b.UserId == uid.Value)
                : query.Where(_ => false);
        }

        ViewBag.IsAdmin = isAdmin;
        return View(await query.ToListAsync());
    }

    // ── Details ───────────────────────────────────────────────────────────────

    [Authorize]
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        var booking = await _context.Bookings.FindAsync(id);
        if (booking == null) return NotFound();
        if (!CanAccess(booking)) return Forbid();
        return View(booking);
    }

    // ── Create ────────────────────────────────────────────────────────────────

    public IActionResult Create(string? source = null, string? destination = null,
        decimal? amount = null, string? vehicleType = null, string? travelDateTime = null)
    {
        var booking = new Booking
        {
            Source        = source ?? string.Empty,
            Destination   = destination ?? string.Empty,
            BookingAmount = amount ?? 0,
            VehicleType   = vehicleType ?? string.Empty,
        };

        // Pre-fill customer details from the logged-in user's account
        if (User.Identity?.IsAuthenticated == true)
        {
            booking.CustomerName         = User.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;
            booking.CustomerMobileNumber = User.FindFirst("MobileNumber")?.Value ?? string.Empty;
        }

        if (DateTime.TryParse(travelDateTime, out var dt))
            booking.TravelDateTime = dt;

        return View(booking);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("CustomerName,CustomerMobileNumber,Source,Destination,VehicleType,TravelDateTime,BookingAmount")]
        Booking booking)
    {
        if (ModelState.IsValid)
        {
            booking.CreatedAt = DateTime.Now;
            booking.UserId    = CurrentUserId();

            _context.Add(booking);
            await _context.SaveChangesAsync();

            // Resolve user for email notification
            AppUser? user = null;
            var uid = booking.UserId;
            if (uid.HasValue)
                user = await _context.Users.FindAsync(uid.Value);
            user ??= await _context.Users.FirstOrDefaultAsync(u =>
                u.MobileNumber == booking.CustomerMobileNumber);

            await _notify.SendBookingConfirmationAsync(booking, user);

            TempData["Success"] = "Booking confirmed! A confirmation has been sent to your contact.";
            return RedirectToAction(nameof(Index));
        }
        return View(booking);
    }

    // ── Edit ──────────────────────────────────────────────────────────────────

    [Authorize]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var booking = await _context.Bookings.FindAsync(id);
        if (booking == null) return NotFound();
        if (!CanAccess(booking)) return Forbid();
        return View(booking);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Edit(int id,
        [Bind("Id,CustomerName,CustomerMobileNumber,Source,Destination,VehicleType,TravelDateTime,BookingAmount,CreatedAt,UserId")]
        Booking booking)
    {
        if (id != booking.Id) return NotFound();
        if (!CanAccess(booking)) return Forbid();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(booking);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Booking updated successfully.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Bookings.Any(e => e.Id == booking.Id))
                    return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }
        return View(booking);
    }

    // ── Delete ────────────────────────────────────────────────────────────────

    [Authorize]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var booking = await _context.Bookings.FindAsync(id);
        if (booking == null) return NotFound();
        if (!CanAccess(booking)) return Forbid();
        return View(booking);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var booking = await _context.Bookings.FindAsync(id);
        if (booking == null) return NotFound();
        if (!CanAccess(booking)) return Forbid();

        _context.Bookings.Remove(booking);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Booking deleted successfully.";
        return RedirectToAction(nameof(Index));
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private int? CurrentUserId()
    {
        var val = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(val, out var uid) ? uid : null;
    }

    private bool CanAccess(Booking booking) =>
        User.IsInRole("Admin") || booking.UserId == CurrentUserId();
}
