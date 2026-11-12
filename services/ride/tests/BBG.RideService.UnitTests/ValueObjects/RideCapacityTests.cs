
using BBG.RideService.Domain.Exceptions;
using BBG.RideService.Domain.ValueObjects;

namespace BBG.RideService.UnitTests.ValueObjects;

public class RideCapacityTests
{
    [Fact]
    public void Create_WithValidCapacity_ShouldInitializeCorrectly()
    {
        var capacity = RideCapacity.Create(30);

        Assert.Equal(30, capacity.MaximumCapacity);
        Assert.Equal(0, capacity.ActiveReservations);
        Assert.Equal(30, capacity.AvailableCapacity);
        Assert.False(capacity.IsAtCapacity);
    }

    [Fact]
    public void Create_WithZeroCapacity_ShouldThrowException()
    {
        Assert.Throws<InvalidRideDataException>(
            () => RideCapacity.Create(0));
    }

    [Fact]
    public void Reserve_ShouldIncreaseActiveReservations()
    {
        var capacity = RideCapacity.Create(30);

        capacity = capacity.Reserve();

        Assert.Equal(1, capacity.ActiveReservations);
        Assert.Equal(29, capacity.AvailableCapacity);
    }

    [Fact]
    public void Reserve_WhenAtCapacity_ShouldThrowException()
    {
        var capacity = RideCapacity.Create(1).Reserve();

        Assert.Throws<InvalidRideOperationException>(
            () => capacity.Reserve());
    }

    [Fact]
    public void Release_ShouldDecreaseActiveReservations()
    {
        var capacity = RideCapacity.Create(30).Reserve();

        capacity = capacity.Release();

        Assert.Equal(0, capacity.ActiveReservations);
        Assert.Equal(30, capacity.AvailableCapacity);
    }

    [Fact]
    public void Release_WhenNoReservationsExist_ShouldThrowException()
    {
        var capacity = RideCapacity.Create(30);

        Assert.Throws<InvalidRideOperationException>(
            () => capacity.Release());
    }

    [Fact]
    public void ChangeMaximumCapacity_BelowActiveReservations_ShouldPreserveReservations()
    {
        var capacity = RideCapacity.Create(30);

        for (var i = 0; i < 25; i++)
        {
            capacity = capacity.Reserve();
        }

        capacity = capacity.ChangeMaximumCapacity(20);

        Assert.Equal(20, capacity.MaximumCapacity);
        Assert.Equal(25, capacity.ActiveReservations);
        Assert.Equal(0, capacity.AvailableCapacity);
        Assert.True(capacity.IsAtCapacity);
    }

    [Fact]
    public void ChangeMaximumCapacity_WithInvalidValue_ShouldThrowException()
    {
        var capacity = RideCapacity.Create(30);

        Assert.Throws<InvalidRideDataException>(
            () => capacity.ChangeMaximumCapacity(0));
    }

    [Fact]
public void Create_WithNegativeCapacity_ShouldThrowException()
{
    Assert.Throws<InvalidRideDataException>(
        () => RideCapacity.Create(-1));
}

[Fact]
public void ChangeMaximumCapacity_WhenIncreased_ShouldPreserveReservations()
{
    var capacity = RideCapacity.Create(20);

    for (var i = 0; i < 10; i++)
    {
        capacity = capacity.Reserve();
    }

    var updatedCapacity = capacity.ChangeMaximumCapacity(30);

    Assert.Equal(30, updatedCapacity.MaximumCapacity);
    Assert.Equal(10, updatedCapacity.ActiveReservations);
    Assert.Equal(20, updatedCapacity.AvailableCapacity);
    Assert.False(updatedCapacity.IsAtCapacity);
}
}
