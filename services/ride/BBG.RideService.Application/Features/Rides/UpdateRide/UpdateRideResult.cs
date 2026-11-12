using BBG.RideService.Domain.Enums;

namespace BBG.RideService.Application.Features.Rides.UpdateRide;

public sealed record UpdateRideResult(
    Guid Id,
    string Title,
    string Description,
    RideType Type,
    string StartLocation,
    string StartLocationAddress,
    string Destination,
    string DestinationAddress,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    int MaximumCapacity,
    int ActiveReservations,
    int AvailableCapacity,
    RideCapacityPolicy CapacityPolicy,
    RideStatus Status,
    DateTimeOffset CreatedAt
);