
using BBG.RideService.Domain.Entities;
using BBG.RideService.Domain.Enums;
using BBG.RideService.Domain.ValueObjects;
using BBG.RideService.Infrastructure.Persistence;
using BBG.RideService.Infrastructure.Persistence.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace BBG.RideService.IntegrationTests.Persistence;

public sealed class RideCapacityPersistenceTests
{
    [Fact]
    public async Task TryReserveCapacityAsync_ShouldIncrementWhenCapacityIsAvailable()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<RideDbContext>()
            .UseSqlite(connection)
            .Options;

        Guid rideId;

        await using (var setupContext = new RideDbContext(options))
        {
            await setupContext.Database.EnsureCreatedAsync();

            var ride = CreatePublishedRide(maximumCapacity: 2);
            rideId = ride.Id;

            await setupContext.Rides.AddAsync(ride);
            await setupContext.SaveChangesAsync();
        }

        await using (var commandContext = new RideDbContext(options))
        {
            var repository = new RideRepository(commandContext);

            var result = await repository.TryReserveCapacityAsync(rideId);

            Assert.True(result);
        }

        await using (var verifyContext = new RideDbContext(options))
        {
            var ride = await verifyContext.Rides.SingleAsync();

            Assert.Equal(1, ride.ActiveReservations);
            Assert.Equal(1, ride.AvailableCapacity);
        }
    }

    [Fact]
    public async Task TryReserveCapacityAsync_ShouldRejectReservationWhenFull()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<RideDbContext>()
            .UseSqlite(connection)
            .Options;

        Guid rideId;

        await using (var setupContext = new RideDbContext(options))
        {
            await setupContext.Database.EnsureCreatedAsync();

            var ride = CreatePublishedRide(maximumCapacity: 1);
            rideId = ride.Id;

            await setupContext.Rides.AddAsync(ride);
            await setupContext.SaveChangesAsync();
        }

        await using (var firstContext = new RideDbContext(options))
        {
            var repository = new RideRepository(firstContext);

            Assert.True(
                await repository.TryReserveCapacityAsync(rideId));
        }

        await using (var secondContext = new RideDbContext(options))
        {
            var repository = new RideRepository(secondContext);

            Assert.False(
                await repository.TryReserveCapacityAsync(rideId));
        }

        await using (var verifyContext = new RideDbContext(options))
        {
            var ride = await verifyContext.Rides.SingleAsync();

            Assert.Equal(1, ride.ActiveReservations);
            Assert.Equal(0, ride.AvailableCapacity);
        }
    }

    [Fact]
    public async Task TryReleaseCapacityAsync_ShouldDecrementWhenReservationExists()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<RideDbContext>()
            .UseSqlite(connection)
            .Options;

        Guid rideId;

        await using (var setupContext = new RideDbContext(options))
        {
            await setupContext.Database.EnsureCreatedAsync();

            var ride = CreatePublishedRide(maximumCapacity: 2);
            ride.ReserveCapacity();
            rideId = ride.Id;

            await setupContext.Rides.AddAsync(ride);
            await setupContext.SaveChangesAsync();
        }

        await using (var commandContext = new RideDbContext(options))
        {
            var repository = new RideRepository(commandContext);

            Assert.True(
                await repository.TryReleaseCapacityAsync(rideId));
        }

        await using (var verifyContext = new RideDbContext(options))
        {
            var ride = await verifyContext.Rides.SingleAsync();

            Assert.Equal(0, ride.ActiveReservations);
            Assert.Equal(2, ride.AvailableCapacity);
        }
    }

    [Fact]
    public async Task TryReleaseCapacityAsync_ShouldRejectWhenNoReservationsExist()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<RideDbContext>()
            .UseSqlite(connection)
            .Options;

        Guid rideId;

        await using (var setupContext = new RideDbContext(options))
        {
            await setupContext.Database.EnsureCreatedAsync();

            var ride = CreatePublishedRide(maximumCapacity: 2);
            rideId = ride.Id;

            await setupContext.Rides.AddAsync(ride);
            await setupContext.SaveChangesAsync();
        }

        await using (var commandContext = new RideDbContext(options))
        {
            var repository = new RideRepository(commandContext);

            Assert.False(
                await repository.TryReleaseCapacityAsync(rideId));
        }

        await using (var verifyContext = new RideDbContext(options))
        {
            var ride = await verifyContext.Rides.SingleAsync();

            Assert.Equal(0, ride.ActiveReservations);
        }
    }

    private static Ride CreatePublishedRide(int maximumCapacity)
    {
        var ride = Ride.Create(
            title: "Capacity Test Ride",
            description: "Ride for capacity persistence tests.",
            type: RideType.DayRide,
            startLocation: RideLocation.Create(
                "Kolkata",
                "Kolkata, West Bengal"),
            destination: RideLocation.Create(
                "Digha",
                "Digha, West Bengal"),
            schedule: RideSchedule.Create(
                new DateTimeOffset(2027, 1, 10, 8, 0, 0, TimeSpan.Zero),
                new DateTimeOffset(2027, 1, 10, 18, 0, 0, TimeSpan.Zero)),
            maximumCapacity: maximumCapacity,
            capacityPolicy: RideCapacityPolicy.Waitlist);

        ride.Publish(
            new DateTimeOffset(2026, 11, 12, 0, 0, 0, TimeSpan.Zero));

        return ride;
    }
}
