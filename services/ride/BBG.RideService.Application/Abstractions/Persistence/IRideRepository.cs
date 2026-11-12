
using BBG.RideService.Domain.Entities;
using BBG.RideService.Domain.Enums;

namespace BBG.RideService.Application.Abstractions.Persistence;

public interface IRideRepository
{
    Task<Ride?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Ride ride,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        Ride ride,
        CancellationToken cancellationToken = default);

    Task<bool> TryReserveCapacityAsync(
        Guid rideId,
        CancellationToken cancellationToken = default);

    Task<bool> TryReleaseCapacityAsync(
        Guid rideId,
        CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<Ride> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        RideStatus? status,
        RideType? type,
        bool upcomingOnly,
        DateTimeOffset currentTime,
        CancellationToken cancellationToken = default);
}
