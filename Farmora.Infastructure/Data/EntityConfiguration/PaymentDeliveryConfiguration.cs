using Farmora.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Farmora.Infastructure.Data.EntityConfiguration;

public sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Provider)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.ProviderReference)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasIndex(x => x.ProviderReference)
            .IsUnique();

        builder.Property(x => x.Amount).HasColumnType("decimal(18,2)");

        builder.Property(x => x.Currency)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        builder.HasIndex(x => x.OrderId);
        builder.HasIndex(x => x.Status);

        builder.HasOne(x => x.Order)
            .WithMany()
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Events)
            .WithOne()
            .HasForeignKey(x => x.PaymentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class PaymentEventConfiguration : IEntityTypeConfiguration<PaymentEvent>
{
    public void Configure(EntityTypeBuilder<PaymentEvent> builder)
    {
        builder.ToTable("PaymentEvents");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.EventId)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasIndex(x => x.EventId)
            .IsUnique();

        builder.Property(x => x.EventType)
            .HasMaxLength(150)
            .IsRequired();

        builder.HasIndex(x => x.PaymentId);
    }
}

public sealed class DeliveryConfiguration : IEntityTypeConfiguration<Delivery>
{
    public void Configure(EntityTypeBuilder<Delivery> builder)
    {
        builder.ToTable("Deliveries");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Fee).HasColumnType("decimal(18,2)");

        builder.Property(x => x.TrackingNumber)
            .HasMaxLength(200);

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        builder.HasIndex(x => x.OrderId);
        builder.HasIndex(x => x.Status);

        builder.HasOne(x => x.Order)
            .WithMany()
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Timeline)
            .WithOne()
            .HasForeignKey(x => x.DeliveryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class DeliveryStatusHistoryConfiguration : IEntityTypeConfiguration<DeliveryStatusHistory>
{
    public void Configure(EntityTypeBuilder<DeliveryStatusHistory> builder)
    {
        builder.ToTable("DeliveryStatusHistories");

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.DeliveryId);
    }
}
