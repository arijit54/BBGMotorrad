using BBG.RideService.Application.Abstractions.Persistence;
using BBG.RideService.Domain.Entities;
using BBG.RideService.Domain.ValueObjects;

namespace BBG.RideService.Application.Features.Rides.UpdateRide;

public sealed class UpdateRideHandler(IRideRepository rideRepository)
{
    public async Task<UpdateRideResult?> HandleAsync(
        UpdateRideCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.RideId == Guid.Empty)
        {
            throw new ArgumentException(
                "Ride ID cannot be empty.",
                nameof(command));
        }

        var ride = await rideRepository.GetByIdAsync(
            command.RideId,
            cancellationToken);

        if (ride is null)
        {
            return null;
        }

        var startLocation = RideLocation.Create(
            command.StartLocationName,
            command.StartLocationAddress);

        var destination = RideLocation.Create(
            command.DestinationName,
            command.DestinationAddress);

        var schedule = RideSchedule.Create(
            command.StartDate,
            command.EndDate);

        ride.UpdateDetails(
            command.Title,
            command.Description,
            command.Type,
            startLocation,
            destination,
            schedule,
            command.MaximumCapacity,
            command.CapacityPolicy);

        await rideRepository.UpdateAsync(
            ride,
            cancellationToken);

        return new UpdateRideResult(
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