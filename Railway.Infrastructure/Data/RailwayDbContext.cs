using Microsoft.EntityFrameworkCore;
using Railway.Domain.Entities;

namespace Railway.Infrastructure.Data;

public class RailwayDbContext : DbContext
{
    public RailwayDbContext(DbContextOptions<RailwayDbContext> options) : base(options)
    {
    }

    public DbSet<Station> Stations => Set<Station>();
    public DbSet<Carrier> Carriers => Set<Carrier>();
    public DbSet<Journey> Journeys => Set<Journey>();
    public DbSet<JourneyLeg> JourneyLegs => Set<JourneyLeg>();
    public DbSet<Fare> Fares => Set<Fare>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Booking> Bookings => Set<Booking>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(RailwayDbContext).Assembly);
    }
}