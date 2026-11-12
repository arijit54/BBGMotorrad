
using BBG.RideService.Application.Abstractions.Persistence;
using BBG.RideService.Application.Features.Rides.GetRideById;
using BBG.RideService.Domain.Entities;

namespace BBG.RideService.UnitTests.Features.Rides.GetRideById;

public sealed class GetRideByIdHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenRideExists_ShouldReturnRideDetails()
    {
        // Arrange
        var ride = CreateTestRide();
        var repository = new FakeRideRepository(ride);
        var handler = new GetRideByIdHandler(repository);

        var query = new GetRideByIdQuery(ride.Id);

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ride.Id, result.Id);
        Assert.Equal(ride.Title, result.Title);
        Assert.Equal(ride.StartLocation.Name, result.StartLocation);
        Assert.Equal(ride.Destination.Name, result.Destination);
        Assert.Equal(ride.MaximumCapacity, result.MaximumCapacity);
        Assert.Equal(ride.AvailableCapacity, result.AvailableCapacity);
    }

    [Fact]
    public async Task HandleAsync_WhenRideDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var repository = new FakeRideRepository(null);
        var handler = new GetRideByIdHandler(repository);

        var query = new GetRideByIdQuery(Guid.NewGuid());

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        Assert.Null(result);
    }

    private static Ride CreateTestRide()
    {
        var startDate = DateTimeOffset.UtcNow.AddDays(30);

        return Ride.Create(
            "Test Ride",
            "Test description",
            BBG.RideService.Domain.Enums.RideType.DayRide,
            BBG.RideService.Domain.ValueObjects.RideLocation.Create(
                "Kolkata", "Kolkata, West Bengal"),
            BBG.RideService.Domain.ValueObjects.RideLocation.Create(
                "Digha", "Digha, West Bengal"),
            BBG.RideService.Domain.ValueObjects.RideSchedule.Create(
                startDate, startDate.AddDays(1)),
            10,
            BBG.RideService.Domain.Enums.RideCapacityPolicy.CloseAtCapacity);
    }

    private sealed class FakeRideRepository(Ride? ride) : IRideRepository
    {
        public Task<Ride?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(ride?.Id == id ? ride : null);
        }

        public Task AddAsync(
            Ride ride,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task UpdateAsync(
            Ride ride,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task<bool> TryReserveCapacityAsync(
            Guid rideId,
            CancellationToken cancellationToken = default)
            => Task.FromResult(false);

        public Task<bool> TryReleaseCapacityAsync(
            Guid rideId,
            CancellationToken cancellationToken = default)
            => Task.FromResult(false);
    }
}
