
using BBG.RideService.Domain.Enums;

namespace BBG.RideService.Application.Features.Rides.CreateRide;

public sealed record CreateRideResult(
    Guid Id,
    string Title,
    string Description,
    RideType Type,
    string StartLocation,
    string Destination,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    int MaximumCapacity,
    int AvailableCapacity,
    RideCapacityPolicy CapacityPolicy,
    RideStatus Status,
    DateTimeOffset CreatedAt
);
