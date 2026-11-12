
using BBG.RideService.Domain.Entities;
using BBG.RideService.Domain.Enums;
using BBG.RideService.Domain.Exceptions;
using BBG.RideService.Domain.ValueObjects;

namespace BBG.RideService.UnitTests.Entities;

public class RideCapacityTests
{
    private static readonly DateTimeOffset CurrentTime =
        new(2026, 11, 1, 10, 0, 0, TimeSpan.Zero);

    private static Ride CreatePublishedRide()
    {
        var ride = Ride.Create(
            "The Tiger's Trail",
            "BBG MOTORRAD adventure ride.",
            RideType.MultiDayRide,
            RideLocation.Create("Kolkata", "West Bengal"),
            RideLocation.Create("Simlipal", "Odisha"),
            RideSchedule.Create(
                CurrentTime.AddDays(10),
                CurrentTime.AddDays(12)),
            30,
            RideCapacityPolicy.Waitlist);

        ride.Publish(CurrentTime);

        return ride;
    }

    [Fact]
    public void ReserveCapacity_WhenPublished_ShouldIncreaseReservationCount()
    {
        var ride = CreatePublishedRide();

        ride.ReserveCapacity();

        Assert.Equal(1, ride.ActiveReservations);
        Assert.Equal(29, ride.AvailableCapacity);
    }

    [Fact]
    public void ReserveCapacity_WhenDraft_ShouldThrowException()
    {
        var ride = Ride.Create(
            "The Tiger's Trail",
            "BBG MOTORRAD adventure ride.",
            RideType.MultiDayRide,
            RideLocation.Create("Kolkata", "West Bengal"),
            RideLocation.Create("Simlipal", "Odisha"),
            RideSchedule.Create(
                CurrentTime.AddDays(10),
                CurrentTime.AddDays(12)),
            30,
            RideCapacityPolicy.Waitlist);

        Assert.Throws<InvalidRideOperationException>(
            () => ride.ReserveCapacity());
    }

    [Fact]
    public void ReleaseCapacity_ShouldDecreaseReservationCount()
    {
        var ride = CreatePublishedRide();

        ride.ReserveCapacity();
        ride.ReleaseCapacity();

        Assert.Equal(0, ride.ActiveReservations);
        Assert.Equal(30, ride.AvailableCapacity);
    }

    [Fact]
    public void UpdateDetails_WhenCapacityIsReduced_ShouldPreserveReservations()
    {
        var ride = CreatePublishedRide();

        for (var i = 0; i < 25; i++)
        {
            ride.ReserveCapacity();
        }

        ride.UpdateDetails(
    ride.Title,
    ride.Description,
    ride.Type,
    ride.StartLocation,
    ride.Destination,
    ride.Schedule,
    20,
    ride.CapacityPolicy);

        Assert.Equal(20, ride.MaximumCapacity);
        Assert.Equal(25, ride.ActiveReservations);
        Assert.Equal(0, ride.AvailableCapacity);
    }

    [Fact]
    public void UpdateDetails_WithInvalidCapacity_ShouldNotModifyRide()
    {
        var ride = CreatePublishedRide();
    
        var originalTitle = ride.Title;
        var originalDescription = ride.Description;
        var originalDestination = ride.Destination;
        var originalMaximumCapacity = ride.MaximumCapacity;
    
        Assert.Throws<InvalidRideDataException>(() =>
            ride.UpdateDetails(
                "Updated Title",
                "Updated Description",
                RideType.DayRide,
                RideLocation.Create("Delhi", "Delhi"),
                RideLocation.Create("Manali", "Himachal Pradesh"),
                RideSchedule.Create(
                    CurrentTime.AddDays(20),
                    CurrentTime.AddDays(22)),
                0,
                RideCapacityPolicy.CloseAtCapacity));
    
        Assert.Equal(originalTitle, ride.Title);
        Assert.Equal(originalDescription, ride.Description);
        Assert.Equal(RideType.MultiDayRide, ride.Type);
        Assert.Equal("Simlipal", ride.Destination.Name);
        Assert.Equal(originalDestination, ride.Destination);
        Assert.Equal(originalMaximumCapacity, ride.MaximumCapacity);
    }
}
