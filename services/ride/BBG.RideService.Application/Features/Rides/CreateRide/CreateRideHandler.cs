
using BBG.RideService.Application.Abstractions.Persistence;
using BBG.RideService.Domain.Entities;
using BBG.RideService.Domain.Enums;
using BBG.RideService.Domain.ValueObjects;

namespace BBG.RideService.Application.Features.Rides.CreateRide;

public sealed class CreateRideHandler(IRideRepository rideRepository)
{
    public async Task<CreateRideResult> HandleAsync(
        CreateRideCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var startLocation = RideLocation.Create(
            command.StartLocationName,
            command.StartLocationAddress);

        var destination = RideLocation.Create(
            command.DestinationName,
            command.DestinationAddress);

        var schedule = RideSchedule.Create(
            command.StartDate,
            command.EndDate);

        var ride = Ride.Create(
            command.Title,
            command.Description,
            command.Type,
            startLocation,
            destination,
            schedule,
            command.MaximumCapacity,
            command.CapacityPolicy);

        await rideRepository.AddAsync(ride, cancellationToken);

        return new CreateRideResult(
            ride.Id,
            ride.Title,
            ride.Description,
            ride.Type,
            ride.StartLocation.Name,
            ride.Destination.Name,
            ride.Schedule.StartDate,
            ride.Schedule.EndDate,
            ride.MaximumCapacity,
            ride.AvailableCapacity,
            ride.CapacityPolicy,
            ride.Status,
            ride.CreatedAt);
    }
}
