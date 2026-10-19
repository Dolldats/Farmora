using Farmora.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Farmora.Infastructure.Data.EntityConfiguration;

public sealed class VerificationConfiguration : IEntityTypeConfiguration<FarmerVerification>
{
    public void Configure(EntityTypeBuilder<FarmerVerification> builder)
    {
        builder.ToTable("FarmerVerifications");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.DecisionNote)
            .HasMaxLength(2000);

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        builder.HasIndex(x => x.FarmerId);
        builder.HasIndex(x => x.Status);

        builder.HasOne(x => x.Farmer)
            .WithMany()
            .HasForeignKey(x => x.FarmerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Documents)
            .WithOne()
            .HasForeignKey(x => x.VerificationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class VerificationDocumentConfiguration : IEntityTypeConfiguration<VerificationDocument>
{
    public void Configure(EntityTypeBuilder<VerificationDocument> builder)
    {
        builder.ToTable("VerificationDocuments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.StorageKey)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.Url)
            .HasMaxLength(2000)
            .IsRequired();

        builder.HasIndex(x => x.VerificationId);
    }
}
