
using BBG.RideService.Domain.Entities;
using BBG.RideService.Domain.Enums;
using BBG.RideService.Domain.ValueObjects;
using BBG.RideService.Infrastructure.Persistence;
using BBG.RideService.Infrastructure.Persistence.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace BBG.RideService.IntegrationTests.Persistence;

public sealed class RidePersistenceTests
{
    [Fact]
    public async Task AddAndRetrieveAsync_ShouldPreserveRideAggregate()
    {
        // Arrange: use an isolated in-memory SQLite database.
        await using var connection =
            new SqliteConnection("Data Source=:memory:");

        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<RideDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var dbContext = new RideDbContext(options);

        await dbContext.Database.EnsureCreatedAsync();

        var repository = new RideRepository(dbContext);

        var startDate = new DateTimeOffset(
            2026, 11, 20, 8, 0, 0, TimeSpan.FromHours(5.5));

        var endDate = new DateTimeOffset(
            2026, 11, 22, 18, 0, 0, TimeSpan.FromHours(5.5));

        var ride = Ride.Create(
            title: "BBG Eastern Himalayan Trail",
            description: "A weekend motorcycle adventure.",
            type: RideType.WeekendRide,
            startLocation: RideLocation.Create(
                "Kolkata",
                "Kolkata, West Bengal"),
            destination: RideLocation.Create(
                "Charkhole",
                "Charkhole, Kalimpong, West Bengal"),
            schedule: RideSchedule.Create(startDate, endDate),
            maximumCapacity: 15,
            capacityPolicy: RideCapacityPolicy.Waitlist);

        ride.Publish(
            new DateTimeOffset(2026, 11, 12, 0, 0, 0, TimeSpan.Zero));

        ride.ReserveCapacity();

        // Act: save the aggregate.
        await repository.AddAsync(ride);

        // Clear tracking to force EF Core to materialize it from SQLite.
        dbContext.ChangeTracker.Clear();

        var retrievedRide = await repository.GetByIdAsync(ride.Id);

        // Assert: verify the complete aggregate.
        Assert.NotNull(retrievedRide);

        Assert.Equal(ride.Id, retrievedRide.Id);
        Assert.Equal("BBG Eastern Himalayan Trail", retrievedRide.Title);
        Assert.Equal("A weekend motorcycle adventure.", retrievedRide.Description);

        Assert.Equal(RideType.WeekendRide, retrievedRide.Type);
        Assert.Equal(RideStatus.Published, retrievedRide.Status);
        Assert.Equal(RideCapacityPolicy.Waitlist, retrievedRide.CapacityPolicy);

        Assert.Equal("Kolkata", retrievedRide.StartLocation.Name);
        Assert.Equal("Kolkata, West Bengal", retrievedRide.StartLocation.Address);

        Assert.Equal("Charkhole", retrievedRide.Destination.Name);
        Assert.Equal(
            "Charkhole, Kalimpong, West Bengal",
            retrievedRide.Destination.Address);

        Assert.Equal(
            startDate.ToUniversalTime(),
            retrievedRide.Schedule.StartDate);

        Assert.Equal(
            endDate.ToUniversalTime(),
            retrievedRide.Schedule.EndDate);

        Assert.Equal(15, retrievedRide.MaximumCapacity);
        Assert.Equal(1, retrievedRide.ActiveReservations);
        Assert.Equal(14, retrievedRide.AvailableCapacity);
    }

    [Fact]
    public async Task GetByIdAsync_WhenRideDoesNotExist_ShouldReturnNull()
    {
        await using var connection =
            new SqliteConnection("Data Source=:memory:");

        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<RideDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var dbContext = new RideDbContext(options);

        await dbContext.Database.EnsureCreatedAsync();

        var repository = new RideRepository(dbContext);

        var result = await repository.GetByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }
}
