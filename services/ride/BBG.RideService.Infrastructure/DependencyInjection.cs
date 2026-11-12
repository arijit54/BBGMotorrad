using BBG.RideService.Application.Abstractions.Persistence;
using BBG.RideService.Infrastructure.Persistence;
using BBG.RideService.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BBG.RideService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddRideInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("RideDatabase")
            ?? throw new InvalidOperationException(
                "RideDatabase connection string is missing.");

        services.AddDbContext<RideDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddScoped<IRideRepository, RideRepository>();

        return services;
    }
}