using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Railway.Domain.Entities;

namespace Railway.Infrastructure.Data.Configurations;

public sealed class JourneyLegConfiguration : IEntityTypeConfiguration<JourneyLeg>
{
    public void Configure(EntityTypeBuilder<JourneyLeg> builder)
    {
        builder.ToTable("JourneyLegs");

        builder.HasKey(leg => leg.Id);

        builder.Property(leg => leg.JourneyId).IsRequired();

        builder.Property(leg => leg.OriginStationId).IsRequired();

        builder.Property(leg => leg.DestinationStationId).IsRequired();

        builder.Property(leg => leg.DepartureTime).IsRequired();

        builder.Property(leg => leg.ArrivalTime).IsRequired();

        builder.Property(leg => leg.CarrierId).IsRequired();

        builder.HasOne<Journey>().WithMany(journey => journey.JourneyLegs).HasForeignKey(leg => leg.JourneyId).OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Station>().WithMany().HasForeignKey(leg => leg.OriginStationId).HasPrincipalKey(station => station.Id).OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Station>().WithMany().HasForeignKey(leg => leg.DestinationStationId).HasPrincipalKey(station => station.Id).OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Carrier>().WithMany().HasForeignKey(leg => leg.CarrierId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(leg => leg.JourneyId);
    }
}