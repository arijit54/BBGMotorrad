
using BBG.RideService.Application.Features.Rides.CreateRide;
using BBG.RideService.Application.Features.Rides.GetRideById;
using BBG.RideService.Application.Features.Rides.GetRides;
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

        return services;
    }
}
