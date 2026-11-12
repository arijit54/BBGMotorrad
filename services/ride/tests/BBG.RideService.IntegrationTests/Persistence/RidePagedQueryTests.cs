
using BBG.RideService.Domain.Entities;
using BBG.RideService.Domain.Enums;
using BBG.RideService.Domain.ValueObjects;
using BBG.RideService.Infrastructure.Persistence;
using BBG.RideService.Infrastructure.Persistence.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace BBG.RideService.IntegrationTests.Persistence;

public sealed class RidePagedQueryTests
{
    [Fact]
    public async Task GetPagedAsync_ShouldReturnAllMatchingRidesWithTotalCount()
    {
        await using var connection = CreateConnection();
        await using var dbContext = await CreateDatabaseAsync(connection);

        var rides = CreateRides();
        dbContext.Rides.AddRange(rides);
        await dbContext.SaveChangesAsync();

        var repository = new RideRepository(dbContext);

        var result = await repository.GetPagedAsync(
            1, 10, null, null, false, DateTimeOffset.UtcNow);

        Assert.Equal(3, result.TotalCount);
        Assert.Equal(3, result.Items.Count);
    }

    [Fact]
    public async Task GetPagedAsync_ShouldFilterByStatus()
    {
        await using var connection = CreateConnection();
        await using var dbContext = await CreateDatabaseAsync(connection);

        var rides = CreateRides();

        rides[0].Publish(DateTimeOffset.UtcNow);
        rides[0].Start();

        dbContext.Rides.AddRange(rides);
        await dbContext.SaveChangesAsync();

        var repository = new RideRepository(dbContext);

        var result = await repository.GetPagedAsync(
            1, 10, RideStatus.InProgress, null,
            false, DateTimeOffset.UtcNow);

        Assert.Single(result.Items);
        Assert.Equal(RideStatus.InProgress, result.Items[0].Status);
    }

    [Fact]
    public async Task GetPagedAsync_ShouldFilterByRideType()
    {
        await using var connection = CreateConnection();
        await using var dbContext = await CreateDatabaseAsync(connection);

        dbContext.Rides.AddRange(CreateRides());
        await dbContext.SaveChangesAsync();

        var repository = new RideRepository(dbContext);

        var result = await repository.GetPagedAsync(
            1, 10, null, RideType.MultiDayRide,
            false, DateTimeOffset.UtcNow);

        Assert.Equal(2, result.TotalCount);
        Assert.All(result.Items,
            ride => Assert.Equal(RideType.MultiDayRide, ride.Type));
    }

    [Fact]
    public async Task GetPagedAsync_ShouldReturnOnlyUpcomingRides()
    {
        await using var connection = CreateConnection();
        await using var dbContext = await CreateDatabaseAsync(connection);

        var rides = CreateRides();
        rides[2] = CreateRide(
            "Past Ride",
            RideType.DayRide,
            DateTimeOffset.UtcNow.AddDays(-10));

        dbContext.Rides.AddRange(rides);
        await dbContext.SaveChangesAsync();

        var repository = new RideRepository(dbContext);

        var result = await repository.GetPagedAsync(
            1, 10, null, null,
            true, DateTimeOffset.UtcNow);

        Assert.Equal(2, result.TotalCount);
        Assert.All(result.Items,
            ride => Assert.True(
                ride.Schedule.StartDate > DateTimeOffset.UtcNow));
    }

    [Fact]
    public async Task GetPagedAsync_ShouldApplyPaginationAndPreserveTotalCount()
    {
        await using var connection = CreateConnection();
        await using var dbContext = await CreateDatabaseAsync(connection);

        dbContext.Rides.AddRange(CreateRides());
        await dbContext.SaveChangesAsync();

        var repository = new RideRepository(dbContext);

        var result = await repository.GetPagedAsync(
            2, 1, null, null,
            false, DateTimeOffset.UtcNow);

        Assert.Equal(3, result.TotalCount);
        Assert.Single(result.Items);
    }

    private static SqliteConnection CreateConnection()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        return connection;
    }

    private static async Task<RideDbContext> CreateDatabaseAsync(
        SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<RideDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new RideDbContext(options);
        await context.Database.EnsureCreatedAsync();

        return context;
    }

    private static List<Ride> CreateRides()
    {
        var now = DateTimeOffset.UtcNow;

        return
        [
            CreateRide("Ride One", RideType.MultiDayRide, now.AddDays(10)),
            CreateRide("Ride Two", RideType.MultiDayRide, now.AddDays(20)),
            CreateRide("Ride Three", RideType.DayRide, now.AddDays(30))
        ];
    }

    private static Ride CreateRide(
        string title,
        RideType type,
        DateTimeOffset startDate)
    {
        return Ride.Create(
            title,
            "Integration test ride",
            type,
            RideLocation.Create("Kolkata", "Kolkata, West Bengal"),
            RideLocation.Create("Digha", "Digha, West Bengal"),
            RideSchedule.Create(startDate, startDate.AddDays(2)),
            20,
            RideCapacityPolicy.Waitlist);
    }
}
