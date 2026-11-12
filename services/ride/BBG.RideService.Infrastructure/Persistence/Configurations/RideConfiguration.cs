using BBG.RideService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BBG.RideService.Infrastructure.Persistence.Configurations;

public sealed class RideConfiguration : IEntityTypeConfiguration<Ride>
{
    public void Configure(EntityTypeBuilder<Ride> builder)
    {
        builder.ToTable("Rides");

        builder.HasKey(ride => ride.Id);
        
        builder.Property<long>("Version")
            .HasDefaultValue(1L)
            .IsConcurrencyToken()
            .IsRequired();

        builder.Property(ride => ride.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(ride => ride.Description)
            .HasMaxLength(5000)
            .IsRequired();

        builder.Property(ride => ride.Type)
            .IsRequired();

        builder.Property(ride => ride.CapacityPolicy)
            .IsRequired();

        builder.Property(ride => ride.Status)
            .IsRequired();

        builder.Property(ride => ride.CreatedAt)
            .HasConversion(
                value => value.UtcTicks,
                value => new DateTimeOffset(value, TimeSpan.Zero))
            .IsRequired();

        builder.Ignore(ride => ride.MaximumCapacity);
        builder.Ignore(ride => ride.ActiveReservations);
        builder.Ignore(ride => ride.AvailableCapacity);

        builder.OwnsOne(ride => ride.StartLocation, location =>
        {
            location.Property(value => value.Name)
                .HasColumnName("StartLocationName")
                .HasMaxLength(200)
                .IsRequired();

            location.Property(value => value.Address)
                .HasColumnName("StartLocationAddress")
                .HasMaxLength(500)
                .IsRequired();
        });

        builder.OwnsOne(ride => ride.Destination, location =>
        {
            location.Property(value => value.Name)
                .HasColumnName("DestinationName")
                .HasMaxLength(200)
                .IsRequired();

            location.Property(value => value.Address)
                .HasColumnName("DestinationAddress")
                .HasMaxLength(500)
                .IsRequired();
        });

        builder.OwnsOne(ride => ride.Schedule, schedule =>
        {
            schedule.Property(value => value.StartDate)
                .HasColumnName("StartDateUtcTicks")
                .HasConversion(
                    value => value.UtcTicks,
                    value => new DateTimeOffset(value, TimeSpan.Zero))
                .IsRequired();

            schedule.Property(value => value.EndDate)
                .HasColumnName("EndDateUtcTicks")
                .HasConversion(
                    value => value.UtcTicks,
                    value => new DateTimeOffset(value, TimeSpan.Zero))
                .IsRequired();
        });

        builder.OwnsOne(ride => ride.Capacity, capacity =>
        {
            capacity.Property(value => value.MaximumCapacity)
                .HasColumnName("MaximumCapacity")
                .IsRequired();

            capacity.Property(value => value.ActiveReservations)
                .HasColumnName("ActiveReservations")
                .IsRequired();

            capacity.Ignore(value => value.AvailableCapacity);
            capacity.Ignore(value => value.IsAtCapacity);
        });
    }
}