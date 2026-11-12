
using BBG.RideService.Application.Abstractions.Persistence;
using BBG.RideService.Application.Features.Rides.CreateRide;
using BBG.RideService.Domain.Entities;
using BBG.RideService.Domain.Enums;
using BBG.RideService.Domain.Exceptions;

namespace BBG.RideService.UnitTests.Features.Rides.CreateRide;

public sealed class CreateRideHandlerTests
{
    [Fact]
    public async Task HandleAsync_WithValidCommand_ShouldCreateAndPersistRide()
    {
        // Arrange
        var repository = new FakeRideRepository();
        var handler = new CreateRideHandler(repository);

        var startDate = DateTimeOffset.UtcNow.AddDays(30);
        var endDate = startDate.AddDays(2);

        var command = new CreateRideCommand(
            Title: "The Eastern Himalayan Trail",
            Description: "A BBG motorcycle adventure.",
            Type: RideType.MultiDayRide,
            StartLocationName: "Kolkata",
            StartLocationAddress: "Kolkata, West Bengal",
            DestinationName: "Charkhole",
            DestinationAddress: "Charkhole, West Bengal",
            StartDate: startDate,
            EndDate: endDate,
            MaximumCapacity: 20,
            CapacityPolicy: RideCapacityPolicy.Waitlist);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        Assert.NotNull(repository.SavedRide);

        Assert.Equal(repository.SavedRide.Id, result.Id);
        Assert.Equal(command.Title, result.Title);
        Assert.Equal(command.Description, result.Description);
        Assert.Equal(command.Type, result.Type);
        Assert.Equal("Kolkata", result.StartLocation);
        Assert.Equal("Charkhole", result.Destination);
        Assert.Equal(20, result.MaximumCapacity);
        Assert.Equal(20, result.AvailableCapacity);
        Assert.Equal(RideCapacityPolicy.Waitlist, result.CapacityPolicy);
        Assert.Equal(RideStatus.Draft, result.Status);
    }

    [Fact]
    public async Task HandleAsync_WithInvalidSchedule_ShouldThrowDomainException()
    {
        // Arrange
        var repository = new FakeRideRepository();
        var handler = new CreateRideHandler(repository);

        var startDate = DateTimeOffset.UtcNow.AddDays(30);

        var command = new CreateRideCommand(
            Title: "Invalid Ride",
            Description: "Invalid schedule test.",
            Type: RideType.DayRide,
            StartLocationName: "Kolkata",
            StartLocationAddress: "Kolkata, West Bengal",
            DestinationName: "Digha",
            DestinationAddress: "Digha, West Bengal",
            StartDate: startDate,
            EndDate: startDate.AddHours(-2),
            MaximumCapacity: 10,
            CapacityPolicy: RideCapacityPolicy.CloseAtCapacity);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidRideDataException>(
            () => handler.HandleAsync(command));

        Assert.Null(repository.SavedRide);
    }

    private sealed class FakeRideRepository : IRideRepository
    {
        public Ride? SavedRide { get; private set; }

        public Task<Ride?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                SavedRide?.Id == id ? SavedRide : null);
        }

        public Task AddAsync(
            Ride ride,
            CancellationToken cancellationToken = default)
        {
            SavedRide = ride;
            return Task.CompletedTask;
        }

        public Task UpdateAsync(
            Ride ride,
            CancellationToken cancellationToken = default)
        {
            SavedRide = ride;
            return Task.CompletedTask;
        }

        public Task<bool> TryReserveCapacityAsync(
            Guid rideId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(false);
        }

        public Task<bool> TryReleaseCapacityAsync(
            Guid rideId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(false);
        }
    }
}
