using CabBookingApp.Data;
using CabBookingApp.Helpers;
using CabBookingApp.Models;
using CabBookingApp.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath        = "/Auth/Login";
        options.LogoutPath       = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/Login";
        options.ExpireTimeSpan   = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
    });

// Notification service
builder.Services.Configure<NotificationSettings>(
    builder.Configuration.GetSection(NotificationSettings.Section));
builder.Services.AddHttpClient();
builder.Services.AddScoped<INotificationService, NotificationService>();

// Server-side state for the pending OTP challenge
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout        = TimeSpan.FromMinutes(15);
    options.Cookie.HttpOnly    = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Apply pending migrations and seed default admin
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    // Create the default admin account only when a seed password is supplied
    // (Seed:AdminPassword, e.g. via user-secrets or an app-service setting).
    if (!db.Users.Any(u => u.Role == "Admin"))
    {
        var seedPassword = app.Configuration["Seed:AdminPassword"];
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Seed");

        if (string.IsNullOrWhiteSpace(seedPassword))
        {
            logger.LogWarning(
                "No admin account exists and 'Seed:AdminPassword' is not configured — skipping admin seeding.");
        }
        else
        {
            db.Users.Add(new AppUser
            {
                Name         = "Admin",
                Email        = app.Configuration["Seed:AdminEmail"] ?? "admin@ridefast.in",
                MobileNumber = app.Configuration["Seed:AdminMobile"] ?? "9000000000",
                PasswordHash = PasswordHelper.CreateHash(seedPassword),
                Role         = "Admin",
                CreatedAt    = DateTime.Now,
            });
            db.SaveChanges();
            logger.LogInformation("Seeded default admin account.");
        }
    }
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
