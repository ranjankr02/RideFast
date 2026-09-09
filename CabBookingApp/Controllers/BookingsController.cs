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

    [Authorize]
    public IActionResult Create(string? source = null, string? destination = null,
        decimal? amount = null, string? vehicleType = null, string? travelDateTime = null)
    {
        var booking = new Booking
        {
            Source        = IndianCities.Canonical(source),
            Destination   = IndianCities.Canonical(destination),
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
    [Authorize]
    public async Task<IActionResult> Create(
        [Bind("CustomerName,CustomerMobileNumber,Source,Destination,VehicleType,TravelDateTime,BookingAmount")]
        Booking booking)
    {
        ValidateCities(booking);

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

            var delivered = await _notify.SendBookingConfirmationAsync(booking, user);

            TempData["Success"] = delivered
                ? "Booking confirmed! A confirmation has been sent to your contact."
                : "Booking confirmed! We could not send the confirmation message — please note your booking id.";
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
        [Bind("Id,CustomerName,CustomerMobileNumber,Source,Destination,VehicleType,TravelDateTime,BookingAmount")]
        Booking booking)
    {
        if (id != booking.Id) return NotFound();

        // Authorise against the stored booking, not the values posted by the caller.
        var existing = await _context.Bookings.FindAsync(id);
        if (existing == null) return NotFound();
        if (!CanAccess(existing)) return Forbid();

        booking.CreatedAt = existing.CreatedAt;
        booking.UserId    = existing.UserId;

        ValidateCities(booking);

        if (ModelState.IsValid)
        {
            existing.CustomerName         = booking.CustomerName;
            existing.CustomerMobileNumber = booking.CustomerMobileNumber;
            existing.Source               = booking.Source;
            existing.Destination          = booking.Destination;
            existing.VehicleType          = booking.VehicleType;
            existing.TravelDateTime       = booking.TravelDateTime;
            existing.BookingAmount        = booking.BookingAmount;

            try
            {
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

    // The pickup/drop selects are populated from IndianCities, so anything else was
    // hand-crafted; canonicalise the casing and reject unknown or identical cities.
    private void ValidateCities(Booking booking)
    {
        booking.Source      = IndianCities.Canonical(booking.Source);
        booking.Destination = IndianCities.Canonical(booking.Destination);

        if (!IndianCities.IsKnown(booking.Source))
            ModelState.AddModelError(nameof(Booking.Source), "Please select a pickup city from the list.");

        if (!IndianCities.IsKnown(booking.Destination))
            ModelState.AddModelError(nameof(Booking.Destination), "Please select a drop city from the list.");

        if (IndianCities.IsKnown(booking.Source) &&
            string.Equals(booking.Source, booking.Destination, StringComparison.OrdinalIgnoreCase))
            ModelState.AddModelError(nameof(Booking.Destination), "Drop city must be different from the pickup city.");
    }

    private int? CurrentUserId()
    {
        var val = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(val, out var uid) ? uid : null;
    }

    private bool CanAccess(Booking booking) =>
        User.IsInRole("Admin") || booking.UserId == CurrentUserId();
}
