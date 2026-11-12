using BBG.RideService.Domain.Entities;
using BBG.RideService.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace BBG.RideService.Infrastructure.Persistence;

public sealed class RideDbContext(
    DbContextOptions<RideDbContext> options) : DbContext(options)
{
    public DbSet<Ride> Rides => Set<Ride>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new RideConfiguration());

        base.OnModelCreating(modelBuilder);
    }
}