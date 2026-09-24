using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Railway.Domain.Entities;

namespace Railway.Infrastructure.Data.Configurations;

public sealed class StationConfiguration : IEntityTypeConfiguration<Station>
{
    public void Configure(EntityTypeBuilder<Station> builder)
    {
        builder.ToTable("Stations");

        builder.HasKey(station => station.Id);

        builder.Property(station => station.Code).IsRequired().HasMaxLength(10);

        builder.Property(station => station.Name).IsRequired().HasMaxLength(200);

        builder.HasIndex(station => station.Code).IsUnique();
    }
}