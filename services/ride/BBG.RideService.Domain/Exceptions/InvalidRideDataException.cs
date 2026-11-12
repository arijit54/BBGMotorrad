
namespace BBG.RideService.Domain.Exceptions;

public sealed class InvalidRideDataException : DomainException
{
    public InvalidRideDataException(string message)
        : base(message)
    {
    }
}
