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
    private const int MaxOtpAttempts = 5;
    private static readonly TimeSpan OtpLifetime    = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan ResendCooldown = TimeSpan.FromSeconds(60);

    private const string SessionUserId     = "PendingOtp.UserId";
    private const string SessionPurpose    = "PendingOtp.Purpose";
    private const string SessionRememberMe = "PendingOtp.RememberMe";
    private const string SessionReturnUrl  = "PendingOtp.ReturnUrl";

    private readonly AppDbContext _context;
    private readonly INotificationService _notify;
    private readonly IWebHostEnvironment _env;

    public AuthController(AppDbContext context, INotificationService notify, IWebHostEnvironment env)
    {
        _context = context;
        _notify  = notify;
        _env     = env;
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
        var email = NormalizeEmail(input);
        var user  = await _context.Users.FirstOrDefaultAsync(u =>
            u.Email == email || u.MobileNumber == input);

        if (user == null || !PasswordHelper.Verify(model.Password, user.PasswordHash))
        {
            ModelState.AddModelError(string.Empty, "Invalid email/mobile or password.");
            return View(model);
        }

        await StartOtpChallengeAsync(user, "Login", model.RememberMe, returnUrl);
        return RedirectToAction(nameof(VerifyOtp));
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

        var email = NormalizeEmail(model.Email);

        if (await _context.Users.AnyAsync(u => u.Email == email))
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
            Email        = email,
            MobileNumber = model.MobileNumber.Trim(),
            PasswordHash = PasswordHelper.CreateHash(model.Password),
            Role         = "User",
            CreatedAt    = DateTime.Now,
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        await StartOtpChallengeAsync(user, "Registration", false, null);
        return RedirectToAction(nameof(VerifyOtp));
    }

    // ── Verify OTP ───────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> VerifyOtp()
    {
        var challenge = ReadChallenge();
        if (challenge == null) return RedirectToAction(nameof(Login));

        var user = await _context.Users.FindAsync(challenge.UserId);
        if (user == null)
        {
            ClearChallenge();
            return RedirectToAction(nameof(Login));
        }

        return View(BuildVerifyOtpViewModel(challenge, user));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyOtp(VerifyOtpViewModel model)
    {
        // The account being verified comes from the server-side session, never from the
        // request, so a caller cannot verify an OTP for an account they did not authenticate.
        var challenge = ReadChallenge();
        if (challenge == null) return RedirectToAction(nameof(Login));

        var user = await _context.Users.FindAsync(challenge.UserId);
        if (user == null)
        {
            ClearChallenge();
            return RedirectToAction(nameof(Login));
        }

        var vm = BuildVerifyOtpViewModel(challenge, user);

        if (!ModelState.IsValid)
            return View(vm);

        var record = await _context.OtpRecords
            .Where(o => o.UserId  == challenge.UserId  &&
                        o.Purpose == challenge.Purpose &&
                        !o.IsUsed &&
                        o.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync();

        if (record == null)
        {
            ModelState.AddModelError(string.Empty, "OTP is invalid or has expired. Please request a new one.");
            return View(vm);
        }

        bool valid = CryptographicOperations.FixedTimeEquals(
            System.Text.Encoding.UTF8.GetBytes(model.Otp.Trim()),
            System.Text.Encoding.UTF8.GetBytes(record.Code));

        if (!valid)
        {
            record.Attempts++;
            bool exhausted = record.Attempts >= MaxOtpAttempts;
            if (exhausted) record.IsUsed = true;
            await _context.SaveChangesAsync();

            ModelState.AddModelError(string.Empty, exhausted
                ? "Too many incorrect attempts. This OTP has been cancelled — please request a new one."
                : $"Incorrect OTP. {MaxOtpAttempts - record.Attempts} attempt(s) left.");
            return View(vm);
        }

        record.IsUsed = true;
        await _context.SaveChangesAsync();

        ClearChallenge();
        await SignInUser(user, challenge.RememberMe);

        TempData["Success"] = challenge.Purpose == "Registration"
            ? $"Welcome to RideFast, {user.Name}! Your account is verified."
            : $"Welcome back, {user.Name}!";

        if (!string.IsNullOrEmpty(challenge.ReturnUrl) && Url.IsLocalUrl(challenge.ReturnUrl))
            return Redirect(challenge.ReturnUrl);

        return RedirectToAction("Index", "Home");
    }

    // ── Resend OTP ───────────────────────────────────────────────────────────

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResendOtp()
    {
        var challenge = ReadChallenge();
        if (challenge == null) return RedirectToAction(nameof(Login));

        var user = await _context.Users.FindAsync(challenge.UserId);
        if (user == null)
        {
            ClearChallenge();
            return RedirectToAction(nameof(Login));
        }

        var lastSentAt = await _context.OtpRecords
            .Where(o => o.UserId == challenge.UserId && o.Purpose == challenge.Purpose)
            .MaxAsync(o => (DateTime?)o.CreatedAt);

        if (lastSentAt.HasValue && DateTime.UtcNow - lastSentAt.Value < ResendCooldown)
        {
            var wait = (int)Math.Ceiling((ResendCooldown - (DateTime.UtcNow - lastSentAt.Value)).TotalSeconds);
            TempData["Info"] = $"Please wait {wait}s before requesting another OTP.";
            return RedirectToAction(nameof(VerifyOtp));
        }

        await StartOtpChallengeAsync(user, challenge.Purpose, challenge.RememberMe, challenge.ReturnUrl);

        TempData["Info"] = "A new OTP has been sent.";
        return RedirectToAction(nameof(VerifyOtp));
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

    private sealed record OtpChallenge(int UserId, string Purpose, bool RememberMe, string? ReturnUrl);

    private OtpChallenge? ReadChallenge()
    {
        var userId  = HttpContext.Session.GetInt32(SessionUserId);
        var purpose = HttpContext.Session.GetString(SessionPurpose);

        if (userId == null || string.IsNullOrEmpty(purpose)) return null;

        return new OtpChallenge(
            userId.Value,
            purpose,
            HttpContext.Session.GetInt32(SessionRememberMe) == 1,
            HttpContext.Session.GetString(SessionReturnUrl));
    }

    private void ClearChallenge()
    {
        HttpContext.Session.Remove(SessionUserId);
        HttpContext.Session.Remove(SessionPurpose);
        HttpContext.Session.Remove(SessionRememberMe);
        HttpContext.Session.Remove(SessionReturnUrl);
    }

    private async Task StartOtpChallengeAsync(AppUser user, string purpose, bool rememberMe, string? returnUrl)
    {
        HttpContext.Session.SetInt32(SessionUserId, user.Id);
        HttpContext.Session.SetString(SessionPurpose, purpose);
        HttpContext.Session.SetInt32(SessionRememberMe, rememberMe ? 1 : 0);

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            HttpContext.Session.SetString(SessionReturnUrl, returnUrl);
        else
            HttpContext.Session.Remove(SessionReturnUrl);

        await _context.OtpRecords
            .Where(o => o.UserId == user.Id && o.Purpose == purpose && !o.IsUsed)
            .ForEachAsync(o => o.IsUsed = true);

        var otp = GenerateOtp();
        await SaveOtpAsync(user.Id, otp, purpose);
        await _notify.SendOtpAsync(user, otp, purpose);

        // Showing the code on screen is a development affordance only.
        if (_notify.IsMock && _env.IsDevelopment())
            TempData["DevOtp"] = otp;
    }

    private static VerifyOtpViewModel BuildVerifyOtpViewModel(OtpChallenge challenge, AppUser user) => new()
    {
        Purpose      = challenge.Purpose,
        MaskedTarget = MaskEmail(user.Email) + " / " + MaskMobile(user.MobileNumber),
    };

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

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
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.Add(OtpLifetime),
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
