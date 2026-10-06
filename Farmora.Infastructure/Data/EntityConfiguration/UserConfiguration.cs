using Farmora.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Farmora.Infastructure.Data.EntityConfiguration;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Email)
                .HasMaxLength(320)
                .IsRequired();

            builder.HasIndex(x => x.Email)
                .IsUnique();

            builder.Property(x => x.Phone)
                .HasMaxLength(32);

            builder.Property(x => x.PasswordHash)
                .IsRequired();

            builder.HasOne(x => x.Customer)
                .WithOne(x => x.User)
                .HasForeignKey<Customer>(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Farmer)
                .WithOne(x => x.User)
                .HasForeignKey<Farmer>(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
       
}
