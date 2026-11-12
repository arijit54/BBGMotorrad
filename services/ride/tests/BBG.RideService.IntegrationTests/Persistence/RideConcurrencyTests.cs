
using BBG.RideService.Application.Exceptions;
using BBG.RideService.Domain.Entities;
using BBG.RideService.Domain.Enums;
using BBG.RideService.Domain.ValueObjects;
using BBG.RideService.Infrastructure.Persistence;
using BBG.RideService.Infrastructure.Persistence.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace BBG.RideService.IntegrationTests.Persistence;

public sealed class RideConcurrencyTests
{
    [Fact]
    public async Task UpdateAsync_WhenRideWasChangedByAnotherContext_ShouldRejectStaleUpdate()
    {
        var databasePath = CreateDatabasePath();

        try
        {
            var options = CreateOptions(databasePath);
            Guid rideId;

            await using (var setupContext = new RideDbContext(options))
            {
                await setupContext.Database.EnsureCreatedAsync();

                var ride = CreatePublishedRide(5);
                rideId = ride.Id;

                await setupContext.Rides.AddAsync(ride);
                await setupContext.SaveChangesAsync();
            }

            await using var firstContext = new RideDbContext(options);
            await using var secondContext = new RideDbContext(options);

            var firstRepository = new RideRepository(firstContext);
            var secondRepository = new RideRepository(secondContext);

            var firstRide = await firstRepository.GetByIdAsync(rideId);
            var secondRide = await secondRepository.GetByIdAsync(rideId);

            Assert.NotNull(firstRide);
            Assert.NotNull(secondRide);

            // Another request reserves capacity and increments Version.
            Assert.True(
                await secondRepository.TryReserveCapacityAsync(rideId));

            // The first context still holds the old Version.
            firstRide.UpdateDetails(
                "Updated Ride Title",
                firstRide.Description,
                firstRide.Type,
                firstRide.StartLocation,
                firstRide.Destination,
                firstRide.Schedule,
                firstRide.MaximumCapacity,
                firstRide.CapacityPolicy);

            await Assert.ThrowsAsync<PersistenceConcurrencyException>(
    () => firstRepository.UpdateAsync(firstRide));

            await using var verifyContext = new RideDbContext(options);

            var persistedRide = await verifyContext.Rides
                .SingleAsync(ride => ride.Id == rideId);

            Assert.Equal(1, persistedRide.ActiveReservations);
            Assert.Equal("Capacity Test Ride", persistedRide.Title);
        }
        finally
        {
            DeleteDatabase(databasePath);
        }
    }

    [Fact]
    public async Task TryReserveCapacityAsync_WhenRequestsCompeteForLastSlot_ShouldAllowOnlyOne()
    {
        var databasePath = CreateDatabasePath();

        try
        {
            var options = CreateOptions(databasePath);
            Guid rideId;

            await using (var setupContext = new RideDbContext(options))
            {
                await setupContext.Database.EnsureCreatedAsync();

                var ride = CreatePublishedRide(1);
                rideId = ride.Id;

                await setupContext.Rides.AddAsync(ride);
                await setupContext.SaveChangesAsync();
            }

            const int requestCount = 10;

            var tasks = Enumerable.Range(0, requestCount)
                .Select(async _ =>
                {
                    await using var context = new RideDbContext(options);

                    var repository = new RideRepository(context);

                    return await repository.TryReserveCapacityAsync(rideId);
                });

            var results = await Task.WhenAll(tasks);

            Assert.Equal(1, results.Count(result => result));
            Assert.Equal(requestCount - 1, results.Count(result => !result));

            await using var verifyContext = new RideDbContext(options);

            var persistedRide = await verifyContext.Rides
                .SingleAsync(ride => ride.Id == rideId);

            Assert.Equal(1, persistedRide.ActiveReservations);
            Assert.Equal(0, persistedRide.AvailableCapacity);
        }
        finally
        {
            DeleteDatabase(databasePath);
        }
    }

    private static DbContextOptions<RideDbContext> CreateOptions(
        string databasePath)
    {
        var connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = databasePath,
            DefaultTimeout = 30
        }.ToString();

        return new DbContextOptionsBuilder<RideDbContext>()
            .UseSqlite(connectionString)
            .Options;
    }

    private static string CreateDatabasePath()
    {
        return Path.Combine(
            Path.GetTempPath(),
            $"bbg-ride-concurrency-{Guid.NewGuid():N}.db");
    }

    private static void DeleteDatabase(string databasePath)
    {
        if (File.Exists(databasePath))
        {
            File.Delete(databasePath);
        }
    }

    private static Ride CreatePublishedRide(int maximumCapacity)
    {
        var ride = Ride.Create(
            title: "Capacity Test Ride",
            description: "Ride for concurrency tests.",
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
