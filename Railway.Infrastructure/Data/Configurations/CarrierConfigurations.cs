using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Railway.Domain.Entities;

namespace Railway.Infrastructure.Data.Configurations;

public sealed class CarrierConfiguration : IEntityTypeConfiguration<Carrier>
{
    public void Configure(EntityTypeBuilder<Carrier> builder)
    {
        builder.ToTable("Carriers");

        builder.HasKey(carrier => carrier.Id);

        builder.Property(carrier => carrier.Code).IsRequired().HasMaxLength(10);

        builder.Property(carrier => carrier.Name).IsRequired().HasMaxLength(200);

        builder.HasIndex(carrier => carrier.Code).IsUnique();
    }
}