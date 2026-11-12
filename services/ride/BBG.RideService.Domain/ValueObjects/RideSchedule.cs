using BBG.RideService.Domain.Exceptions;

namespace BBG.RideService.Domain.ValueObjects;

public sealed record RideSchedule
{
    public DateTimeOffset StartDate { get; }
    public DateTimeOffset EndDate { get; }

    private RideSchedule(
        DateTimeOffset startDate,
        DateTimeOffset endDate)
    {
        StartDate = startDate.ToUniversalTime();
        EndDate = endDate.ToUniversalTime();
    }

    public static RideSchedule Create(
        DateTimeOffset startDate,
        DateTimeOffset endDate)
    {
        if (endDate <= startDate)
        {
            throw new InvalidRideDataException(
                "Ride end date must be after its start date.");
        }

        return new RideSchedule(startDate, endDate);
    }
}