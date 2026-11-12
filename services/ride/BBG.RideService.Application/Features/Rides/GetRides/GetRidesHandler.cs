
using BBG.RideService.Application.Abstractions.Persistence;

namespace BBG.RideService.Application.Features.Rides.GetRides;

public sealed class GetRidesHandler(IRideRepository rideRepository)
{
    public async Task<PaginatedResult<RideSummaryResult>> HandleAsync(
        GetRidesQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (query.PageNumber < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(query.PageNumber),
                "Page number must be greater than zero.");
        }

        if (query.PageSize < 1 || query.PageSize > 100)
        {
            throw new ArgumentOutOfRangeException(
                nameof(query.PageSize),
                "Page size must be between 1 and 100.");
        }

        var (rides, totalCount) = await rideRepository.GetPagedAsync(
            query.PageNumber,
            query.PageSize,
            query.Status,
            query.Type,
            query.UpcomingOnly,
            DateTimeOffset.UtcNow,
            cancellationToken);

        var items = rides.Select(ride => new RideSummaryResult(
            ride.Id,
            ride.Title,
            ride.Type,
            ride.StartLocation.Name,
            ride.Destination.Name,
            ride.Schedule.StartDate,
            ride.Schedule.EndDate,
            ride.MaximumCapacity,
            ride.AvailableCapacity,
            ride.CapacityPolicy,
            ride.Status)).ToList();

        return new PaginatedResult<RideSummaryResult>(
            items,
            query.PageNumber,
            query.PageSize,
            totalCount);
    }
}
