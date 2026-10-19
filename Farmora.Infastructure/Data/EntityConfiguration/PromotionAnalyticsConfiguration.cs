using Farmora.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Farmora.Infastructure.Data.EntityConfiguration;

public sealed class PromotionConfiguration : IEntityTypeConfiguration<Promotion>
{
    public void Configure(EntityTypeBuilder<Promotion> builder)
    {
        builder.ToTable("Promotions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(x => x.Code)
            .IsUnique();

        builder.Property(x => x.Value).HasColumnType("decimal(18,2)");
        builder.Property(x => x.MinOrderAmount).HasColumnType("decimal(18,2)");

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        builder.HasIndex(x => x.Status);

        builder.HasMany(x => x.Redemptions)
            .WithOne()
            .HasForeignKey(x => x.PromotionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class CouponRedemptionConfiguration : IEntityTypeConfiguration<CouponRedemption>
{
    public void Configure(EntityTypeBuilder<CouponRedemption> builder)
    {
        builder.ToTable("CouponRedemptions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Discount).HasColumnType("decimal(18,2)");

        builder.HasIndex(x => x.PromotionId);
        builder.HasIndex(x => x.OrderId);
        builder.HasIndex(x => x.CustomerId);
    }
}

public sealed class AnalyticsConfiguration : IEntityTypeConfiguration<AnalyticsEvent>
{
    public void Configure(EntityTypeBuilder<AnalyticsEvent> builder)
    {
        builder.ToTable("AnalyticsEvents");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.EventType)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.Metadata)
            .HasMaxLength(4000);

        builder.HasIndex(x => x.EventType);
        builder.HasIndex(x => x.ActorId);
        builder.HasIndex(x => x.CreatedAtUtc);
    }
}

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Action)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Resource)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Detail)
            .HasMaxLength(4000);

        builder.HasIndex(x => x.CreatedAtUtc);
        builder.HasIndex(x => x.ActorId);
        builder.HasIndex(x => new { x.Resource, x.ResourceId });
    }
}
