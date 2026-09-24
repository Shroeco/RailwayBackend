using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Railway.Domain.Entities;

namespace Railway.Infrastructure.Data.Configurations;

public sealed class JourneyConfiguration : IEntityTypeConfiguration<Journey>
{
    public void Configure(EntityTypeBuilder<Journey> builder)
    {
        builder.ToTable("Journeys");

        builder.HasKey(journey => journey.Id);

        builder.Property(journey => journey.OriginStationId).IsRequired();

        builder.Property(journey => journey.DestinationStationId).IsRequired();

        builder.Property(journey => journey.DepartureTime).IsRequired();

        builder.Property(journey => journey.ArrivalTime).IsRequired();

        builder.Property(journey => journey.Capacity).IsRequired();

        builder.Property(journey => journey.AvailableSeats).IsRequired();

        builder.HasOne<Station>().WithMany().HasForeignKey(journey => journey.OriginStationId).OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Station>().WithMany().HasForeignKey(journey => journey.DestinationStationId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(journey => new
        {
            journey.OriginStationId,
            journey.DestinationStationId,
            journey.DepartureTime
        });
    }
}