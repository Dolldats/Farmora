using Farmora.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Farmora.Infastructure.Data.EntityConfiguration;

public sealed class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.ToTable("Reviews");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Comment)
            .HasMaxLength(2000);

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        builder.HasIndex(x => x.OrderItemId)
            .IsUnique();

        builder.HasIndex(x => x.ProductId);
        builder.HasIndex(x => x.CustomerId);
    }
}

public sealed class ReportConfiguration : IEntityTypeConfiguration<Report>
{
    public void Configure(EntityTypeBuilder<Report> builder)
    {
        builder.ToTable("Reports");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TargetType)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Reason)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(x => x.EvidenceUrl)
            .HasMaxLength(2000);

        builder.Property(x => x.Resolution)
            .HasMaxLength(2000);

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        builder.HasIndex(x => x.ReporterId);
        builder.HasIndex(x => new { x.TargetType, x.TargetId });
        builder.HasIndex(x => x.Status);
    }
}

public sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(x => x.Body)
            .HasMaxLength(5000)
            .IsRequired();

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        builder.HasIndex(x => x.RecipientId);
        builder.HasIndex(x => new { x.RecipientId, x.IsRead });
    }
}
