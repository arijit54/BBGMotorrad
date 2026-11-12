
using BBG.RideService.Domain.Enums;
using BBG.RideService.Domain.Exceptions;
using BBG.RideService.Domain.ValueObjects;

namespace BBG.RideService.Domain.Entities;

public sealed class Ride
{
    public Guid Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public RideType Type { get; private set; }
    public RideLocation StartLocation { get; private set; } = null!;
    public RideLocation Destination { get; private set; } = null!;
    public RideSchedule Schedule { get; private set; } = null!;

    public RideCapacity Capacity { get; private set; } = null!;

    public int MaximumCapacity => Capacity.MaximumCapacity;
    public int ActiveReservations => Capacity.ActiveReservations;
    public int AvailableCapacity => Capacity.AvailableCapacity;

    public RideCapacityPolicy CapacityPolicy { get; private set; }
    public RideStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private Ride()
    {
        // Reserved for ORM materialization.
    }

    private Ride(
        string title,
        string description,
        RideType type,
        RideLocation startLocation,
        RideLocation destination,
        RideSchedule schedule,
        int maximumCapacity,
        RideCapacityPolicy capacityPolicy)
    {
        Id = Guid.NewGuid();
        Title = title;
        Description = description;
        Type = type;
        StartLocation = startLocation;
        Destination = destination;
        Schedule = schedule;
        Capacity = RideCapacity.Create(maximumCapacity);
        CapacityPolicy = capacityPolicy;
        Status = RideStatus.Draft;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public static Ride Create(
        string title,
        string description,
        RideType type,
        RideLocation startLocation,
        RideLocation destination,
        RideSchedule schedule,
        int maximumCapacity,
        RideCapacityPolicy capacityPolicy)
    {
        ValidateText(title, nameof(title), "Ride title is required.");
        ValidateText(description, nameof(description), "Ride description is required.");

        if (!Enum.IsDefined(type))
        {
            throw new InvalidRideDataException("Invalid ride type.");
        }

        if (!Enum.IsDefined(capacityPolicy))
        {
            throw new InvalidRideDataException("Invalid ride capacity policy.");
        }

        ArgumentNullException.ThrowIfNull(startLocation);
        ArgumentNullException.ThrowIfNull(destination);
        ArgumentNullException.ThrowIfNull(schedule);

        return new Ride(
            title.Trim(),
            description.Trim(),
            type,
            startLocation,
            destination,
            schedule,
            maximumCapacity,
            capacityPolicy);
    }

    public void Publish(DateTimeOffset currentTime)
    {
        if (Status != RideStatus.Draft)
        {
            throw new InvalidRideOperationException(
                "Only draft rides can be published.");
        }

        if (Schedule.StartDate <= currentTime.ToUniversalTime())
        {
            throw new InvalidRideOperationException(
                "A ride can only be published before its start date.");
        }

        Status = RideStatus.Published;
    }

    public void Start()
    {
        if (Status != RideStatus.Published)
        {
            throw new InvalidRideOperationException(
                "Only published rides can be started.");
        }

        Status = RideStatus.InProgress;
    }

    public void Complete()
    {
        if (Status != RideStatus.InProgress)
        {
            throw new InvalidRideOperationException(
                "Only in-progress rides can be completed.");
        }

        Status = RideStatus.Completed;
    }

    public void Cancel()
    {
        if (Status is RideStatus.Completed or RideStatus.Cancelled)
        {
            throw new InvalidRideOperationException(
                "Completed or already cancelled rides cannot be cancelled.");
        }

        Status = RideStatus.Cancelled;
    }

    public void UpdateDetails(
        string title,
        string description,
        RideSchedule schedule,
        RideLocation destination,
        int maximumCapacity)
    {
        if (Status is RideStatus.Completed or RideStatus.Cancelled)
        {
            throw new InvalidRideOperationException(
                "Completed or cancelled rides cannot be edited.");
        }

        ValidateText(title, nameof(title), "Ride title is required.");
        ValidateText(description, nameof(description), "Ride description is required.");

        ArgumentNullException.ThrowIfNull(schedule);
        ArgumentNullException.ThrowIfNull(destination);

        Title = title.Trim();
        Description = description.Trim();
        Schedule = schedule;
        Destination = destination;

        Capacity = Capacity.ChangeMaximumCapacity(maximumCapacity);
    }

    public void ReserveCapacity()
    {
        if (Status != RideStatus.Published)
        {
            throw new InvalidRideOperationException(
                "Reservations are only allowed for published rides.");
        }

        Capacity = Capacity.Reserve();
    }

    public void ReleaseCapacity()
    {
        Capacity = Capacity.Release();
    }

    private static void ValidateText(
        string value,
        string parameterName,
        string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidRideDataException(
                $"{errorMessage} Parameter: {parameterName}.");
        }
    }
}
