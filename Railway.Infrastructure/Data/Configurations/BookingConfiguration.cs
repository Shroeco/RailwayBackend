using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Railway.Domain.Entities;

namespace Railway.Infrastructure.Data.Configurations;

public sealed class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable("Bookings");

        builder.HasKey(booking => booking.Id);

        builder.Property(booking => booking.CustomerId).IsRequired();

        builder.Property(booking => booking.JourneyId).IsRequired();

        builder.Property(booking => booking.FareId).IsRequired();

        builder.Property(booking => booking.BookedAt).IsRequired();

        builder.Property(booking => booking.Status).IsRequired().HasConversion<string>().HasMaxLength(20);

        builder.HasOne<Customer>().WithMany().HasForeignKey(booking => booking.CustomerId).OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Journey>().WithMany().HasForeignKey(booking => booking.JourneyId).OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Fare>().WithMany().HasForeignKey(booking => booking.FareId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(booking => booking.CustomerId);
        builder.HasIndex(booking => booking.JourneyId);
    }
}