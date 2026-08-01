using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using AirportApp.Models.Commerce;

namespace AirportApp.Data;

public class ApplicationDbContext : IdentityDbContext<IdentityUser, IdentityRole, string>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Seat> Seats => Set<Seat>();
    public DbSet<SeatReservation> SeatReservations => Set<SeatReservation>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderDetail> OrderDetails => Set<OrderDetail>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<TransactionHistory> TransactionHistory => Set<TransactionHistory>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Payment>()
            .HasIndex(p => new { p.Gateway, p.ExternalTransactionId })
            .IsUnique();
    }
}
