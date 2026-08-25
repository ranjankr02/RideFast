using CabBookingApp.Models;
using Microsoft.EntityFrameworkCore;

namespace CabBookingApp.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Booking> Bookings { get; set; }
    public DbSet<AppUser> Users { get; set; }
    public DbSet<OtpRecord> OtpRecords { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppUser>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<AppUser>()
            .HasIndex(u => u.MobileNumber)
            .IsUnique();
    }
}
