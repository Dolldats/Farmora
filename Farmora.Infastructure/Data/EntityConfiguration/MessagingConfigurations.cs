using Farmora.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Farmora.Infastructure.Data.EntityConfiguration;

public sealed class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> builder)
    {
        builder.ToTable("Conversations");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Subject)
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        builder.HasMany(x => x.Messages)
            .WithOne()
            .HasForeignKey(x => x.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Participants)
            .WithOne()
            .HasForeignKey(x => x.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class ConversationParticipantConfiguration : IEntityTypeConfiguration<ConversationParticipant>
{
    public void Configure(EntityTypeBuilder<ConversationParticipant> builder)
    {
        builder.ToTable("ConversationParticipants");

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new { x.ConversationId, x.UserId })
            .IsUnique();

        builder.HasIndex(x => x.UserId);
    }
}

public sealed class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.ToTable("Messages");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Body)
            .HasMaxLength(5000)
            .IsRequired();

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        builder.HasIndex(x => x.ConversationId);
        builder.HasIndex(x => x.SenderId);
    }
}

public sealed class FarmerFollowConfiguration : IEntityTypeConfiguration<FarmerFollow>
{
    public void Configure(EntityTypeBuilder<FarmerFollow> builder)
    {
        builder.ToTable("FarmerFollows");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        builder.HasIndex(x => new { x.CustomerId, x.FarmerId })
            .IsUnique();
    }
}

public sealed class ProductFavoriteConfiguration : IEntityTypeConfiguration<ProductFavorite>
{
    public void Configure(EntityTypeBuilder<ProductFavorite> builder)
    {
        builder.ToTable("ProductFavorites");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        builder.HasIndex(x => new { x.CustomerId, x.ProductId })
            .IsUnique();
    }
}
