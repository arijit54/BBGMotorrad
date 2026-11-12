
using BBG.RideService.Application.Abstractions.Persistence;
using BBG.RideService.Domain.Entities;
using BBG.RideService.Domain.Enums;
using BBG.RideService.Infrastructure.Persistence;
using BBG.RideService.Application.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace BBG.RideService.Infrastructure.Persistence.Repositories;

public sealed class RideRepository(RideDbContext dbContext)
    : IRideRepository
{
    public async Task<Ride?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Rides
            .SingleOrDefaultAsync(
                ride => ride.Id == id,
                cancellationToken);
    }

    public async Task AddAsync(
        Ride ride,
        CancellationToken cancellationToken = default)
    {
        await dbContext.Rides.AddAsync(ride, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(
        Ride ride,
        CancellationToken cancellationToken = default)
    {
        var entry = dbContext.Entry(ride);
    
        if (entry.State == EntityState.Detached)
        {
            throw new InvalidOperationException(
                "Ride must be loaded by the current DbContext before updating.");
        }
    
        var version = entry.Property<long>("Version");
        version.CurrentValue = version.OriginalValue + 1;
    
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            throw new PersistenceConcurrencyException(
                "The ride was modified by another operation. Please reload and try again.",
                exception);
        }
    }
    public async Task<bool> TryReserveCapacityAsync(
        Guid rideId,
        CancellationToken cancellationToken = default)
    {
        var affectedRows = await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"""
            UPDATE Rides
            SET ActiveReservations = ActiveReservations + 1,
                Version = Version + 1
            WHERE Id = {rideId}
              AND Status = {(int)RideStatus.Published}
              AND ActiveReservations < MaximumCapacity
            """,
            cancellationToken);

        return affectedRows == 1;
    }

    public async Task<bool> TryReleaseCapacityAsync(
        Guid rideId,
        CancellationToken cancellationToken = default)
    {
        var affectedRows = await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"""
            UPDATE Rides
            SET ActiveReservations = ActiveReservations - 1,
                Version = Version + 1
            WHERE Id = {rideId}
              AND ActiveReservations > 0
            """,
            cancellationToken);

        return affectedRows == 1;
    }


    public async Task<(IReadOnlyList<Ride> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        RideStatus? status,
        RideType? type,
        bool upcomingOnly,
        DateTimeOffset currentTime,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Rides
            .AsNoTracking()
            .AsQueryable();
    
        if (status.HasValue)
        {
            query = query.Where(ride => ride.Status == status.Value);
        }
    
        if (type.HasValue)
        {
            query = query.Where(ride => ride.Type == type.Value);
        }
    
        if (upcomingOnly)
        {
            query = query.Where(
                ride => ride.Schedule.StartDate > currentTime);
        }
    
        var totalCount = await query.CountAsync(cancellationToken);
    
        var rides = await query
            .OrderBy(ride => ride.Schedule.StartDate)
            .ThenBy(ride => ride.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    
        return (rides, totalCount);
    }

}
