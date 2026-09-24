using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Railway.Domain.Entities;

namespace Railway.Infrastructure.Data.Configurations;

public sealed class FareConfiguration : IEntityTypeConfiguration<Fare>
{
    public void Configure(EntityTypeBuilder<Fare> builder)
    {
        builder.ToTable("Fares");

        builder.HasKey(fare => fare.Id);

        builder.Property(fare => fare.JourneyId).IsRequired();

        builder.Property(fare => fare.Name).IsRequired().HasMaxLength(100);

        builder.Property(fare => fare.Price).IsRequired().HasPrecision(10, 2);

        builder.HasOne<Journey>().WithMany(journey => journey.Fares).HasForeignKey(fare => fare.JourneyId).OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(fare => fare.JourneyId);
    }
}