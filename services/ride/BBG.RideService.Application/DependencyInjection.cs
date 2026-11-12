
using BBG.RideService.Application.Features.Rides.CreateRide;
using BBG.RideService.Application.Features.Rides.GetRideById;
using BBG.RideService.Application.Features.Rides.GetRides;
using BBG.RideService.Application.Features.Rides.UpdateRide;
using Microsoft.Extensions.DependencyInjection;

namespace BBG.RideService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddRideApplication(
        this IServiceCollection services)
    {
        services.AddScoped<CreateRideHandler>();
        services.AddScoped<GetRideByIdHandler>();
        services.AddScoped<GetRidesHandler>();
        services.AddScoped<UpdateRideHandler>();

        return services;
    }
}
