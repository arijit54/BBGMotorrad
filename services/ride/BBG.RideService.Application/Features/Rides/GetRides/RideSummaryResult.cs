
using BBG.RideService.Domain.Enums;

namespace BBG.RideService.Application.Features.Rides.GetRides;

public sealed record RideSummaryResult(
    Guid Id,
    string Title,
    RideType Type,
    string StartLocation,
    string Destination,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    int MaximumCapacity,
    int AvailableCapacity,
    RideCapacityPolicy CapacityPolicy,
    RideStatus Status
);
