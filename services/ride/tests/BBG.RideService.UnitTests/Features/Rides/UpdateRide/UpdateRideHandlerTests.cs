
using BBG.RideService.Application.Abstractions.Persistence;
using BBG.RideService.Application.Exceptions;
using BBG.RideService.Application.Features.Rides.UpdateRide;
using BBG.RideService.Domain.Entities;
using BBG.RideService.Domain.Enums;
using BBG.RideService.Domain.Exceptions;
using BBG.RideService.Domain.ValueObjects;

namespace BBG.RideService.UnitTests.Features.Rides.UpdateRide;

public sealed class UpdateRideHandlerTests
{
    [Fact]
    public async Task HandleAsync_WithValidDraftRide_ShouldUpdateAndPersistRide()
    {
        var repository = new FakeRideRepository();
        var ride = CreateValidRide();
        repository.Seed(ride);

        var handler = new UpdateRideHandler(repository);
        var command = CreateValidCommand(ride.Id);

        var result = await handler.HandleAsync(command);

        Assert.NotNull(result);
        Assert.Equal(command.RideId, result.Id);
        Assert.Equal(command.Title, result.Title);
        Assert.Equal(command.Description, result.Description);
        Assert.Equal(command.Type, result.Type);
        Assert.Equal(command.StartLocationName, result.StartLocation);
        Assert.Equal(command.StartLocationAddress, result.StartLocationAddress);
        Assert.Equal(command.DestinationName, result.Destination);
        Assert.Equal(command.DestinationAddress, result.DestinationAddress);
        Assert.Equal(command.MaximumCapacity, result.MaximumCapacity);
        Assert.Equal(command.CapacityPolicy, result.CapacityPolicy);
        Assert.Equal(RideStatus.Draft, result.Status);
        Assert.True(repository.UpdateCalled);
    }

    [Fact]
    public async Task HandleAsync_WithValidPublishedRide_ShouldUpdateAndPreserveStatus()
    {
        var repository = new FakeRideRepository();
        var ride = CreateValidRide();
        ride.Publish(DateTimeOffset.UtcNow);
        repository.Seed(ride);

        var handler = new UpdateRideHandler(repository);
        var command = CreateValidCommand(ride.Id);

        var result = await handler.HandleAsync(command);

        Assert.NotNull(result);
        Assert.Equal(RideStatus.Published, result.Status);
        Assert.Equal(command.Title, result.Title);
        Assert.True(repository.UpdateCalled);
    }

    [Fact]
    public async Task HandleAsync_WhenRideDoesNotExist_ShouldReturnNull()
    {
        var repository = new FakeRideRepository();
        var handler = new UpdateRideHandler(repository);

        var command = CreateValidCommand(Guid.NewGuid());

        var result = await handler.HandleAsync(command);

        Assert.Null(result);
        Assert.False(repository.UpdateCalled);
    }

    [Fact]
    public async Task HandleAsync_WithEmptyRideId_ShouldThrowException()
    {
        var repository = new FakeRideRepository();
        var handler = new UpdateRideHandler(repository);

        var command = CreateValidCommand(Guid.Empty);

        await Assert.ThrowsAsync<ArgumentException>(
            () => handler.HandleAsync(command));

        Assert.False(repository.UpdateCalled);
    }

    [Fact]
    public async Task HandleAsync_WithInvalidSchedule_ShouldThrowDomainException()
    {
        var repository = new FakeRideRepository();
        var ride = CreateValidRide();
        repository.Seed(ride);

        var handler = new UpdateRideHandler(repository);
        var startDate = DateTimeOffset.UtcNow.AddDays(30);

        var command = CreateValidCommand(
            ride.Id,
            startDate: startDate,
            endDate: startDate.AddDays(-1));

        await Assert.ThrowsAsync<InvalidRideDataException>(
            () => handler.HandleAsync(command));

        Assert.False(repository.UpdateCalled);
    }

    [Theory]
    [InlineData(RideStatus.InProgress)]
    [InlineData(RideStatus.Completed)]
    [InlineData(RideStatus.Cancelled)]
    public async Task HandleAsync_WhenRideCannotBeEdited_ShouldThrowException(
        RideStatus status)
    {
        var repository = new FakeRideRepository();
        var ride = CreateValidRide();

        if (status is RideStatus.InProgress or RideStatus.Completed)
        {
            ride.Publish(DateTimeOffset.UtcNow);
            ride.Start();
        }

        if (status == RideStatus.Completed)
        {
            ride.Complete();
        }
        else if (status == RideStatus.Cancelled)
        {
            ride.Cancel();
        }

        repository.Seed(ride);

        var handler = new UpdateRideHandler(repository);
        var command = CreateValidCommand(ride.Id);

        await Assert.ThrowsAsync<InvalidRideOperationException>(
            () => handler.HandleAsync(command));

        Assert.False(repository.UpdateCalled);
    }

    [Fact]
    public async Task HandleAsync_WhenConcurrencyConflictOccurs_ShouldPropagateException()
    {
        var repository = new FakeRideRepository
        {
            UpdateException = new PersistenceConcurrencyException(
                "The ride was modified by another operation.",
                new Exception("Concurrency conflict."))
        };

        var ride = CreateValidRide();
        repository.Seed(ride);

        var handler = new UpdateRideHandler(repository);
        var command = CreateValidCommand(ride.Id);

        await Assert.ThrowsAsync<PersistenceConcurrencyException>(
            () => handler.HandleAsync(command));

        Assert.True(repository.UpdateCalled);
    }

    private static Ride CreateValidRide()
    {
        return Ride.Create(
            "Original Ride",
            "Original description.",
            RideType.MultiDayRide,
            RideLocation.Create("Kolkata", "Kolkata, West Bengal"),
            RideLocation.Create("Charkhole", "Charkhole, West Bengal"),
            RideSchedule.Create(
                DateTimeOffset.UtcNow.AddDays(10),
                DateTimeOffset.UtcNow.AddDays(12)),
            30,
            RideCapacityPolicy.Waitlist);
    }

    private static UpdateRideCommand CreateValidCommand(
        Guid rideId,
        DateTimeOffset? startDate = null,
        DateTimeOffset? endDate = null)
    {
        var start = startDate ?? DateTimeOffset.UtcNow.AddDays(20);

        return new UpdateRideCommand(
            RideId: rideId,
            Title: "Updated Eastern Himalayan Trail",
            Description: "Updated BBG MOTORRAD adventure.",
            Type: RideType.WeekendRide,
            StartLocationName: "Siliguri",
            StartLocationAddress: "Siliguri, West Bengal",
            DestinationName: "Puri",
            DestinationAddress: "Puri, Odisha",
            StartDate: start,
            EndDate: endDate ?? start.AddDays(2),
            MaximumCapacity: 25,
            CapacityPolicy: RideCapacityPolicy.CloseAtCapacity);
    }

    private sealed class FakeRideRepository : IRideRepository
    {
        private Ride? _ride;

        public bool UpdateCalled { get; private set; }

        public Exception? UpdateException { get; init; }

        public void Seed(Ride ride)
        {
            _ride = ride;
        }

        public Task<Ride?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                _ride?.Id == id ? _ride : null);
        }

        public Task AddAsync(
            Ride ride,
            CancellationToken cancellationToken = default)
        {
            _ride = ride;
            return Task.CompletedTask;
        }

        public Task UpdateAsync(
            Ride ride,
            CancellationToken cancellationToken = default)
        {
            UpdateCalled = true;

            if (UpdateException is not null)
            {
                throw UpdateException;
            }

            _ride = ride;
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

        public Task<(IReadOnlyList<Ride> Items, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            RideStatus? status,
            RideType? type,
            bool upcomingOnly,
            DateTimeOffset currentTime,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<(IReadOnlyList<Ride> Items, int TotalCount)>(
                (Array.Empty<Ride>(), 0));
        }
    }
}
