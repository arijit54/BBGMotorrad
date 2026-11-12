
using BBG.RideService.Domain.Exceptions;

namespace BBG.RideService.Domain.ValueObjects;

public sealed record RideCapacity
{
    public int MaximumCapacity { get; }
    public int ActiveReservations { get; }

    public int AvailableCapacity =>
        Math.Max(0, MaximumCapacity - ActiveReservations);

    public bool IsAtCapacity =>
        ActiveReservations >= MaximumCapacity;

    private RideCapacity(
        int maximumCapacity,
        int activeReservations)
    {
        MaximumCapacity = maximumCapacity;
        ActiveReservations = activeReservations;
    }

    public static RideCapacity Create(int maximumCapacity)
    {
        if (maximumCapacity <= 0)
        {
            throw new InvalidRideDataException(
                "Maximum capacity must be greater than zero.");
        }

        return new RideCapacity(maximumCapacity, 0);
    }

    public RideCapacity Reserve()
    {
        if (IsAtCapacity)
        {
            throw new InvalidRideOperationException(
                "The ride has reached its maximum capacity.");
        }

        return new RideCapacity(
            MaximumCapacity,
            ActiveReservations + 1);
    }

    public RideCapacity Release()
    {
        if (ActiveReservations <= 0)
        {
            throw new InvalidRideOperationException(
                "There are no active reservations to release.");
        }

        return new RideCapacity(
            MaximumCapacity,
            ActiveReservations - 1);
    }

    public RideCapacity ChangeMaximumCapacity(int newMaximumCapacity)
    {
        if (newMaximumCapacity <= 0)
        {
            throw new InvalidRideDataException(
                "Maximum capacity must be greater than zero.");
        }

        // Existing reservations are deliberately preserved,
        // even when they exceed the new maximum capacity.
        return new RideCapacity(
            newMaximumCapacity,
            ActiveReservations);
    }
}
