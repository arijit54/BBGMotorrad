
using BBG.RideService.Domain.Enums;

namespace BBG.RideService.Application.Features.Rides.GetRides;

public sealed record GetRidesQuery(
    int PageNumber = 1,
    int PageSize = 10,
    RideStatus? Status = null,
    RideType? Type = null,
    bool UpcomingOnly = false
);
