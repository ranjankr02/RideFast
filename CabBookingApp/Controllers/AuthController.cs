using System.Security.Claims;
using System.Security.Cryptography;
using CabBookingApp.Data;
using CabBookingApp.Helpers;
using CabBookingApp.Models;
using CabBookingApp.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CabBookingApp.Controllers;

public class AuthController : Controller
{
    private readonly AppDbContext _context;
    private readonly INotificationService _notify;

    public AuthController(AppDbContext context, INotificationService notify)
    {
        _context = context;
        _notify  = notify;
    }

    // ── Login ────────────────────────────────────────────────────────────────

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;

        if (!ModelState.IsValid)
            return View(model);

        var input = model.EmailOrMobile.Trim();
        var user  = await _context.Users.FirstOrDefaultAsync(u =>
            u.Email == input || u.MobileNumber == input);

        if (user == null || !PasswordHelper.Verify(model.Password, user.PasswordHash))
        {
            ModelState.AddModelError(string.Empty, "Invalid email/mobile or password.");
            return View(model);
        }

        var otp = GenerateOtp();
        await SaveOtpAsync(user.Id, otp, "Login");
        await _notify.SendOtpAsync(user, otp, "Login");

        if (_notify.IsMock)
            TempData["DevOtp"] = otp;

        return RedirectToAction(nameof(VerifyOtp), new
        {
            userId     = user.Id,
            purpose    = "Login",
            rememberMe = model.RememberMe,
            returnUrl
        });
    }

    // ── Register ─────────────────────────────────────────────────────────────

    [HttpGet]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        if (await _context.Users.AnyAsync(u => u.Email == model.Email.Trim()))
        {
            ModelState.AddModelError("Email", "This email is already registered.");
            return View(model);
        }

        if (await _context.Users.AnyAsync(u => u.MobileNumber == model.MobileNumber.Trim()))
        {
            ModelState.AddModelError("MobileNumber", "This mobile number is already registered.");
            return View(model);
        }

        var user = new AppUser
        {
            Name         = model.Name.Trim(),
            Email        = model.Email.Trim().ToLower(),
            MobileNumber = model.MobileNumber.Trim(),
            PasswordHash = PasswordHelper.CreateHash(model.Password),
            Role         = "User",
            CreatedAt    = DateTime.Now,
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var otp = GenerateOtp();
        await SaveOtpAsync(user.Id, otp, "Registration");
        await _notify.SendOtpAsync(user, otp, "Registration");

        if (_notify.IsMock)
            TempData["DevOtp"] = otp;

        return RedirectToAction(nameof(VerifyOtp), new
        {
            userId  = user.Id,
            purpose = "Registration"
        });
    }

    // ── Verify OTP ───────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> VerifyOtp(int userId, string purpose,
        bool rememberMe = false, string? returnUrl = null)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return RedirectToAction(nameof(Login));

        var vm = new VerifyOtpViewModel
        {
            UserId       = userId,
            Purpose      = purpose,
            RememberMe   = rememberMe,
            ReturnUrl    = returnUrl,
            MaskedTarget = MaskEmail(user.Email) + " / " + MaskMobile(user.MobileNumber),
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyOtp(VerifyOtpViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var record = await _context.OtpRecords
            .Where(o => o.UserId  == model.UserId  &&
                        o.Purpose == model.Purpose  &&
                        !o.IsUsed &&
                        o.ExpiresAt > DateTime.Now)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync();

        if (record == null)
        {
            ModelState.AddModelError(string.Empty, "OTP is invalid or has expired. Please request a new one.");
            return View(model);
        }

        bool valid = CryptographicOperations.FixedTimeEquals(
            System.Text.Encoding.UTF8.GetBytes(model.Otp.Trim()),
            System.Text.Encoding.UTF8.GetBytes(record.Code));

        if (!valid)
        {
            ModelState.AddModelError(string.Empty, "Incorrect OTP. Please try again.");
            return View(model);
        }

        record.IsUsed = true;
        await _context.SaveChangesAsync();

        var user = await _context.Users.FindAsync(model.UserId);
        if (user == null) return RedirectToAction(nameof(Login));

        await SignInUser(user, model.RememberMe);

        TempData["Success"] = model.Purpose == "Registration"
            ? $"Welcome to RideFast, {user.Name}! Your account is verified."
            : $"Welcome back, {user.Name}!";

        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            return Redirect(model.ReturnUrl);

        return RedirectToAction("Index", "Home");
    }

    // ── Resend OTP ───────────────────────────────────────────────────────────

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResendOtp(int userId, string purpose,
        bool rememberMe = false, string? returnUrl = null)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return RedirectToAction(nameof(Login));

        var active = _context.OtpRecords
            .Where(o => o.UserId == userId && o.Purpose == purpose && !o.IsUsed);
        await active.ForEachAsync(o => o.IsUsed = true);

        var otp = GenerateOtp();
        await SaveOtpAsync(userId, otp, purpose);
        await _notify.SendOtpAsync(user, otp, purpose);

        if (_notify.IsMock)
            TempData["DevOtp"] = otp;

        TempData["Info"] = "A new OTP has been sent.";
        return RedirectToAction(nameof(VerifyOtp), new { userId, purpose, rememberMe, returnUrl });
    }

    // ── Logout ───────────────────────────────────────────────────────────────

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private async Task SignInUser(AppUser user, bool isPersistent)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name,           user.Name),
            new(ClaimTypes.Email,          user.Email),
            new("MobileNumber",            user.MobileNumber),
            new(ClaimTypes.Role,           user.Role),
        };

        var identity  = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties { IsPersistent = isPersistent });
    }

    private async Task SaveOtpAsync(int userId, string code, string purpose)
    {
        _context.OtpRecords.Add(new OtpRecord
        {
            UserId    = userId,
            Code      = code,
            Purpose   = purpose,
            CreatedAt = DateTime.Now,
            ExpiresAt = DateTime.Now.AddMinutes(10),
            IsUsed    = false
        });
        await _context.SaveChangesAsync();
    }

    private static string GenerateOtp()
    {
        var bytes = new byte[4];
        RandomNumberGenerator.Fill(bytes);
        var num = BitConverter.ToUInt32(bytes) % 900000u + 100000u;
        return num.ToString();
    }

    private static string MaskEmail(string email)
    {
        var idx = email.IndexOf('@');
        if (idx <= 1) return email;
        return email[0] + new string('*', Math.Min(idx - 1, 4)) + email[idx..];
    }

    private static string MaskMobile(string mobile)
    {
        if (mobile.Length < 6) return mobile;
        return mobile[..2] + new string('*', mobile.Length - 4) + mobile[^2..];
    }
}
