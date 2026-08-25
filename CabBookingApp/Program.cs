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

    // Create default admin account if none exists
    if (!db.Users.Any(u => u.Role == "Admin"))
    {
        db.Users.Add(new AppUser
        {
            Name         = "Admin",
            Email        = "admin@ridefast.in",
            MobileNumber = "9000000000",
            PasswordHash = PasswordHelper.CreateHash(""),
            Role         = "Admin",
            CreatedAt    = DateTime.Now,
        });
        db.SaveChanges();
    }
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
