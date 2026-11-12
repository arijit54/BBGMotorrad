
using BBG.RideService.Domain.Entities;
using BBG.RideService.Domain.Enums;
using BBG.RideService.Domain.Exceptions;
using BBG.RideService.Domain.ValueObjects;

namespace BBG.RideService.UnitTests.Entities;

public class RideTests
{
    private static readonly DateTimeOffset FixedCurrentTime =
        new(2026, 11, 1, 10, 0, 0, TimeSpan.Zero);

    private static Ride CreateValidRide()
    {
        var startLocation = RideLocation.Create(
            "Kolkata",
            "Kolkata, West Bengal");

        var destination = RideLocation.Create(
            "Simlipal",
            "Mayurbhanj, Odisha");

        var schedule = RideSchedule.Create(
            FixedCurrentTime.AddDays(10),
            FixedCurrentTime.AddDays(12));

        return Ride.Create(
            "The Tiger's Trail",
            "A BBG MOTORRAD adventure ride to Simlipal.",
            RideType.MultiDayRide,
            startLocation,
            destination,
            schedule,
            30,
            RideCapacityPolicy.Waitlist);
    }

    private static void UpdateRideWithCurrentDetails(Ride ride)
    {
        ride.UpdateDetails(
            ride.Title,
            ride.Description,
            ride.Type,
            ride.StartLocation,
            ride.Destination,
            ride.Schedule,
            ride.MaximumCapacity,
            ride.CapacityPolicy);
    }

    [Fact]
    public void Create_WithValidDetails_ShouldCreateDraftRide()
    {
        var ride = CreateValidRide();

        Assert.NotEqual(Guid.Empty, ride.Id);
        Assert.Equal("The Tiger's Trail", ride.Title);
        Assert.Equal(RideStatus.Draft, ride.Status);
        Assert.Equal(30, ride.MaximumCapacity);
        Assert.Equal(RideCapacityPolicy.Waitlist, ride.CapacityPolicy);
    }

    [Fact]
    public void Create_WithEmptyTitle_ShouldThrowException()
    {
        Assert.Throws<InvalidRideDataException>(() =>
            Ride.Create(
                " ",
                "Adventure ride",
                RideType.MultiDayRide,
                RideLocation.Create("Kolkata", "West Bengal"),
                RideLocation.Create("Simlipal", "Odisha"),
                RideSchedule.Create(
                    FixedCurrentTime.AddDays(10),
                    FixedCurrentTime.AddDays(12)),
                30,
                RideCapacityPolicy.Waitlist));
    }

    [Fact]
    public void Create_WithZeroCapacity_ShouldThrowException()
    {
        Assert.Throws<InvalidRideDataException>(() =>
            Ride.Create(
                "The Tiger's Trail",
                "Adventure ride",
                RideType.MultiDayRide,
                RideLocation.Create("Kolkata", "West Bengal"),
                RideLocation.Create("Simlipal", "Odisha"),
                RideSchedule.Create(
                    FixedCurrentTime.AddDays(10),
                    FixedCurrentTime.AddDays(12)),
                0,
                RideCapacityPolicy.Waitlist));
    }

    [Fact]
    public void Create_WithValidDetails_ShouldTrimTitle()
    {
        var ride = Ride.Create(
            "  The Tiger's Trail  ",
            "Adventure ride",
            RideType.MultiDayRide,
            RideLocation.Create("Kolkata", "West Bengal"),
            RideLocation.Create("Simlipal", "Odisha"),
            RideSchedule.Create(
                FixedCurrentTime.AddDays(10),
                FixedCurrentTime.AddDays(12)),
            30,
            RideCapacityPolicy.Waitlist);

        Assert.Equal("The Tiger's Trail", ride.Title);
    }

    [Fact]
    public void Publish_WhenRideIsDraftAndStartDateIsFuture_ShouldPublishRide()
    {
        var ride = CreateValidRide();

        ride.Publish(FixedCurrentTime);

        Assert.Equal(RideStatus.Published, ride.Status);
    }

    [Fact]
    public void Publish_WhenStartDateIsInPast_ShouldThrowException()
    {
        var ride = CreateValidRide();

        var pastSchedule = RideSchedule.Create(
            FixedCurrentTime.AddDays(-2),
            FixedCurrentTime.AddDays(-1));

        ride.UpdateDetails(
    ride.Title,
    ride.Description,
    ride.Type,
    ride.StartLocation,
    ride.Destination,
    pastSchedule,
    ride.MaximumCapacity,
    ride.CapacityPolicy);

        Assert.Throws<InvalidRideOperationException>(
            () => ride.Publish(FixedCurrentTime));
    }

    [Fact]
    public void Publish_WhenRideIsAlreadyPublished_ShouldThrowException()
    {
        var ride = CreateValidRide();

        ride.Publish(FixedCurrentTime);

        Assert.Throws<InvalidRideOperationException>(
            () => ride.Publish(FixedCurrentTime));
    }

    [Fact]
    public void Start_WhenRideIsDraft_ShouldThrowException()
    {
        var ride = CreateValidRide();

        Assert.Throws<InvalidRideOperationException>(
            () => ride.Start());
    }

    [Fact]
    public void Complete_WhenRideIsInProgress_ShouldChangeStatusToCompleted()
    {
        var ride = CreateValidRide();

        ride.Publish(FixedCurrentTime);
        ride.Start();
        ride.Complete();

        Assert.Equal(RideStatus.Completed, ride.Status);
    }

    [Fact]
    public void Cancel_WhenRideIsCompleted_ShouldThrowException()
    {
        var ride = CreateValidRide();

        ride.Publish(FixedCurrentTime);
        ride.Start();
        ride.Complete();

        Assert.Throws<InvalidRideOperationException>(
            () => ride.Cancel());
    }

    [Fact]
    public void UpdateDetails_WhenRideIsPublished_ShouldUpdateRideDetails()
    {
        var ride = CreateValidRide();

        ride.Publish(FixedCurrentTime);

        var updatedSchedule = RideSchedule.Create(
            FixedCurrentTime.AddDays(20),
            FixedCurrentTime.AddDays(22));

        var updatedDestination = RideLocation.Create(
            "Puri",
            "Puri, Odisha");

        var updatedStartLocation = RideLocation.Create(
            "Bhubaneswar",
            "Bhubaneswar, Odisha");

        ride.UpdateDetails(
            "The Coastal Trail",
            "A new BBG MOTORRAD coastal adventure.",
            RideType.WeekendRide,
            updatedStartLocation,
            updatedDestination,
            updatedSchedule,
            40,
            RideCapacityPolicy.CloseAtCapacity);
        Assert.Equal("The Coastal Trail", ride.Title);
        Assert.Equal("A new BBG MOTORRAD coastal adventure.", ride.Description);
        Assert.Equal(updatedSchedule, ride.Schedule);
        Assert.Equal(updatedDestination, ride.Destination);
        Assert.Equal(40, ride.MaximumCapacity);
        Assert.Equal(RideStatus.Published, ride.Status);
        Assert.Equal(RideType.WeekendRide, ride.Type);
        Assert.Equal(updatedStartLocation, ride.StartLocation);
        Assert.Equal(RideCapacityPolicy.CloseAtCapacity, ride.CapacityPolicy);
    }

    [Fact]
    public void UpdateDetails_WhenCapacityIsReduced_ShouldAllowUpdate()
    {
        var ride = CreateValidRide();

        ride.Publish(FixedCurrentTime);

        // Active reservation accounting belongs to the capacity
        // reservation workflow, not this Ride entity.
        ride.UpdateDetails(
            ride.Title,
            ride.Description,
            ride.Type,
            ride.StartLocation,
            ride.Destination,
            ride.Schedule,
            10,
            ride.CapacityPolicy);
        Assert.Equal(10, ride.MaximumCapacity);
        Assert.Equal(RideStatus.Published, ride.Status);
    }

    [Fact]
    public void UpdateDetails_WhenRideIsCompleted_ShouldThrowException()
    {
        var ride = CreateValidRide();

        ride.Publish(FixedCurrentTime);
        ride.Start();
        ride.Complete();

        Assert.Throws<InvalidRideOperationException>(() =>
            ride.UpdateDetails(
                "Updated ride",
                "Updated description",
                ride.Type,
                ride.StartLocation,
                ride.Destination,
                ride.Schedule,
                20,
                ride.CapacityPolicy));
    }

    [Fact]
    public void UpdateDetails_WhenRideIsCancelled_ShouldThrowException()
    {
        var ride = CreateValidRide();

        ride.Cancel();

        Assert.Throws<InvalidRideOperationException>(() =>
            ride.UpdateDetails(
            "Updated ride",
            "Updated description",
            ride.Type,
            ride.StartLocation,
            ride.Destination,
            ride.Schedule,
            20,
            ride.CapacityPolicy));
    }

    [Fact]
public void UpdateDetails_WhenRideIsDraft_ShouldUpdateRideDetails()
{
    var ride = CreateValidRide();

    var updatedDestination = RideLocation.Create(
        "Puri",
        "Puri, Odisha");

    ride.UpdateDetails(
        "Updated adventure",
        "Updated description",
        RideType.WeekendRide,
        ride.StartLocation,
        updatedDestination,
        ride.Schedule,
        25,
        RideCapacityPolicy.CloseAtCapacity);

    Assert.Equal("Updated adventure", ride.Title);
    Assert.Equal("Updated description", ride.Description);
    Assert.Equal(RideType.WeekendRide, ride.Type);
    Assert.Equal(updatedDestination, ride.Destination);
    Assert.Equal(25, ride.MaximumCapacity);
    Assert.Equal(RideCapacityPolicy.CloseAtCapacity, ride.CapacityPolicy);
    Assert.Equal(RideStatus.Draft, ride.Status);
}

[Fact]
public void UpdateDetails_WhenRideIsInProgress_ShouldThrowException()
{
    var ride = CreateValidRide();
    ride.Publish(FixedCurrentTime);
    ride.Start();

    Assert.Throws<InvalidRideOperationException>(() =>
        UpdateRideWithCurrentDetails(ride));
}

[Fact]
public void UpdateDetails_WhenCapacityIsZero_ShouldThrowException()
{
    var ride = CreateValidRide();

    Assert.Throws<InvalidRideDataException>(() =>
        ride.UpdateDetails(
            ride.Title,
            ride.Description,
            ride.Type,
            ride.StartLocation,
            ride.Destination,
            ride.Schedule,
            0,
            ride.CapacityPolicy));
}

[Fact]
public void UpdateDetails_WhenTitleIsEmpty_ShouldThrowException()
{
    var ride = CreateValidRide();

    Assert.Throws<InvalidRideDataException>(() =>
        ride.UpdateDetails(
            " ",
            ride.Description,
            ride.Type,
            ride.StartLocation,
            ride.Destination,
            ride.Schedule,
            ride.MaximumCapacity,
            ride.CapacityPolicy));
}

[Fact]
public void UpdateDetails_WhenRideTypeIsInvalid_ShouldThrowException()
{
    var ride = CreateValidRide();

    Assert.Throws<InvalidRideDataException>(() =>
        ride.UpdateDetails(
            ride.Title,
            ride.Description,
            (RideType)999,
            ride.StartLocation,
            ride.Destination,
            ride.Schedule,
            ride.MaximumCapacity,
            ride.CapacityPolicy));
}

[Fact]
public void UpdateDetails_WhenCapacityPolicyIsInvalid_ShouldThrowException()
{
    var ride = CreateValidRide();

    Assert.Throws<InvalidRideDataException>(() =>
        ride.UpdateDetails(
            ride.Title,
            ride.Description,
            ride.Type,
            ride.StartLocation,
            ride.Destination,
            ride.Schedule,
            ride.MaximumCapacity,
            (RideCapacityPolicy)999));
}
}
