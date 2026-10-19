using Farmora.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Farmora.Infastructure.Data.EntityConfiguration;

public sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.DisplayName)
            .HasMaxLength(200);

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        builder.HasIndex(x => x.UserId)
            .IsUnique();

        builder.HasOne(x => x.User)
            .WithOne(x => x.Customer)
            .HasForeignKey<Customer>(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class FarmerConfiguration : IEntityTypeConfiguration<Farmer>
{
    public void Configure(EntityTypeBuilder<Farmer> builder)
    {
        builder.ToTable("Farmers");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.FarmName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(2000);

        builder.Property(x => x.Address)
            .HasMaxLength(500);

        builder.Property(x => x.Latitude)
            .HasPrecision(9, 6);

        builder.Property(x => x.Longitude)
            .HasPrecision(9, 6);

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        builder.HasIndex(x => x.UserId)
            .IsUnique();

        builder.HasIndex(x => x.VerificationStatus);
        builder.HasIndex(x => x.OperationalStatus);

        builder.HasOne(x => x.User)
            .WithOne(x => x.Farmer)
            .HasForeignKey<Farmer>(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
