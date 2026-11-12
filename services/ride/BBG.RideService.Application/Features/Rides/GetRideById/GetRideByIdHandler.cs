
using BBG.RideService.Application.Abstractions.Persistence;

namespace BBG.RideService.Application.Features.Rides.GetRideById;

public sealed class GetRideByIdHandler(IRideRepository rideRepository)
{
    public async Task<GetRideByIdResult?> HandleAsync(
        GetRideByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (query.RideId == Guid.Empty)
        {
            throw new ArgumentException(
                "Ride ID cannot be empty.",
                nameof(query));
        }

        var ride = await rideRepository.GetByIdAsync(
            query.RideId,
            cancellationToken);

        if (ride is null)
        {
            return null;
        }

        return new GetRideByIdResult(
            ride.Id,
            ride.Title,
            ride.Description,
            ride.Type,
            ride.StartLocation.Name,
            ride.StartLocation.Address,
            ride.Destination.Name,
            ride.Destination.Address,
            ride.Schedule.StartDate,
            ride.Schedule.EndDate,
            ride.MaximumCapacity,
            ride.ActiveReservations,
            ride.AvailableCapacity,
            ride.CapacityPolicy,
            ride.Status,
            ride.CreatedAt);
    }
}
