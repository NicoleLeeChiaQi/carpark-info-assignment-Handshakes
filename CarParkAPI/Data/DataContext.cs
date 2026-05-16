using Microsoft.EntityFrameworkCore;
using CarParkAPI.Models;

namespace CarParkAPI.Data;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options) : base(options) { }

    public DbSet<CarPark> CarParks => Set<CarPark>();
    public DbSet<CarParkType> CarParkTypes => Set<CarParkType>();
    public DbSet<ParkingSystem> ParkingSystems => Set<ParkingSystem>();
    public DbSet<ParkingPolicy> ParkingPolicies => Set<ParkingPolicy>();
    public DbSet<UserFavorite> UserFavorites => Set<UserFavorite>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Explicit 1:1 binding between CarPark and its Policy
        modelBuilder.Entity<CarPark>()
            .HasOne(c => c.ParkingPolicy)
            .WithOne(p => p.CarPark)
            .HasForeignKey<ParkingPolicy>(p => p.CarParkNo);

        // Core Performance Query Tuning Indexes
        modelBuilder.Entity<CarPark>().HasIndex(c => c.GantryHeight);
        modelBuilder.Entity<ParkingPolicy>().HasIndex(p => p.FreeParking);
        modelBuilder.Entity<ParkingPolicy>().HasIndex(p => p.OffersNightParking);
        modelBuilder.Entity<UserFavorite>()
            .HasOne(f => f.CarPark)
            .WithMany()
            .HasForeignKey(f => f.CarParkNo);
    }
}