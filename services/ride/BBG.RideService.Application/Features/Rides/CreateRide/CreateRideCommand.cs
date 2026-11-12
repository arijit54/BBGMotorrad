
using BBG.RideService.Domain.Enums;

namespace BBG.RideService.Application.Features.Rides.CreateRide;

public sealed record CreateRideCommand(
    string Title,
    string Description,
    RideType Type,
    string StartLocationName,
    string StartLocationAddress,
    string DestinationName,
    string DestinationAddress,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    int MaximumCapacity,
    RideCapacityPolicy CapacityPolicy
);
