namespace BBG.RideService.Application.Exceptions;

public sealed class PersistenceConcurrencyException : Exception
{
    public PersistenceConcurrencyException(
        string message,
        Exception innerException)
        : base(message, innerException)
    {
    }
}