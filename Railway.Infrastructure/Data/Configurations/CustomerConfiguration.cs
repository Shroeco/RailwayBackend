using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Railway.Domain.Entities;

namespace Railway.Infrastructure.Data.Configurations;

public sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");

        builder.HasKey(customer => customer.Id);

        builder.Property(customer => customer.Name).IsRequired().HasMaxLength(200);

        builder.Property(customer => customer.Email).IsRequired().HasMaxLength(320);

        builder.HasIndex(customer => customer.Email).IsUnique();
    }
}