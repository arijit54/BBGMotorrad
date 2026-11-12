
namespace BBG.RideService.Domain.Exceptions;

public sealed class InvalidRideOperationException : DomainException
{
    public InvalidRideOperationException(string message)
        : base(message)
    {
    }
}
