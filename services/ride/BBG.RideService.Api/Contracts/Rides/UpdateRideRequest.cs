
using System.ComponentModel.DataAnnotations;
using BBG.RideService.Domain.Enums;

namespace BBG.RideService.Api.Contracts.Rides;

public sealed record UpdateRideRequest
{
    [Required]
    [StringLength(200, MinimumLength = 3)]
    public string Title { get; init; } = string.Empty;

    [Required]
    [StringLength(5000, MinimumLength = 3)]
    public string Description { get; init; } = string.Empty;

    public RideType Type { get; init; }

    [Required]
    public string StartLocationName { get; init; } = string.Empty;

    [Required]
    public string StartLocationAddress { get; init; } = string.Empty;

    [Required]
    public string DestinationName { get; init; } = string.Empty;

    [Required]
    public string DestinationAddress { get; init; } = string.Empty;

    public DateTimeOffset StartDate { get; init; }

    public DateTimeOffset EndDate { get; init; }

    [Range(1, int.MaxValue)]
    public int MaximumCapacity { get; init; }

    public RideCapacityPolicy CapacityPolicy { get; init; }
}
