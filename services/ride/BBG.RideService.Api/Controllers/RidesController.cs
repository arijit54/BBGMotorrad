
using BBG.RideService.Api.Contracts.Rides;
using BBG.RideService.Application.Features.Rides.CreateRide;
using BBG.RideService.Application.Features.Rides.GetRideById;
using BBG.RideService.Domain.Exceptions;
using BBG.RideService.Application.Features.Rides.GetRides;
using Microsoft.AspNetCore.Mvc;
using BBG.RideService.Domain.Enums;
using BBG.RideService.Application.Features.Rides.UpdateRide;
using BBG.RideService.Application.Exceptions;

namespace BBG.RideService.Api.Controllers;

[ApiController]
[Route("api/rides")]
public sealed class RidesController(
    CreateRideHandler createRideHandler,
    GetRideByIdHandler getRideByIdHandler,
    GetRidesHandler getRidesHandler,
    UpdateRideHandler updateRideHandler) : ControllerBase{
    [HttpPost]
    [ProducesResponseType(typeof(CreateRideResult), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CreateRideResult>> CreateRide(
        [FromBody] CreateRideRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateRideCommand(
            request.Title,
            request.Description,
            request.Type,
            request.StartLocationName,
            request.StartLocationAddress,
            request.DestinationName,
            request.DestinationAddress,
            request.StartDate,
            request.EndDate,
            request.MaximumCapacity,
            request.CapacityPolicy);

        try
        {
            var result = await createRideHandler.HandleAsync(
                command,
                cancellationToken);

            return Created($"/api/rides/{result.Id}", result);
        }
        catch (InvalidRideDataException exception)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid ride data",
                Detail = exception.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(GetRideByIdResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GetRideByIdResult>> GetRideById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetRideByIdQuery(id);

        var result = await getRideByIdHandler.HandleAsync(
            query,
            cancellationToken);

        if (result is null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Ride not found",
                Detail = $"No ride was found with ID '{id}'.",
                Status = StatusCodes.Status404NotFound
            });
        }

        return Ok(result);
    }


    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<RideSummaryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PaginatedResult<RideSummaryResult>>> GetRides(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] RideStatus? status = null,
        [FromQuery] RideType? type = null,
        [FromQuery] bool upcomingOnly = false,
        CancellationToken cancellationToken = default)
    {
        var query = new GetRidesQuery(
            pageNumber,
            pageSize,
            status,
            type,
            upcomingOnly);
    
        try
        {
            var result = await getRidesHandler.HandleAsync(
                query,
                cancellationToken);
    
            return Ok(result);
        }
        catch (ArgumentOutOfRangeException exception)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid pagination parameters",
                Detail = exception.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
    }
    
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(UpdateRideResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UpdateRideResult>> UpdateRide(
        Guid id,
        [FromBody] UpdateRideRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateRideCommand(
            id,
            request.Title,
            request.Description,
            request.Type,
            request.StartLocationName,
            request.StartLocationAddress,
            request.DestinationName,
            request.DestinationAddress,
            request.StartDate,
            request.EndDate,
            request.MaximumCapacity,
            request.CapacityPolicy);
    
        try
        {
            var result = await updateRideHandler.HandleAsync(
                command,
                cancellationToken);
    
            if (result is null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Ride not found",
                    Detail = $"No ride was found with ID '{id}'.",
                    Status = StatusCodes.Status404NotFound
                });
            }
    
            return Ok(result);
        }
        catch (DomainException exception)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Ride update rejected",
                Detail = exception.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (PersistenceConcurrencyException exception)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Ride update conflict",
                Detail = exception.Message,
                Status = StatusCodes.Status409Conflict
            });
        }
    }

}
