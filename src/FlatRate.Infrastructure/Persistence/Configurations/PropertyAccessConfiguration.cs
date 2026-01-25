using FlatRate.Domain.Aggregates.Properties;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlatRate.Infrastructure.Persistence.Configurations;

public class PropertyAccessConfiguration : IEntityTypeConfiguration<PropertyAccess>
{
    public void Configure(EntityTypeBuilder<PropertyAccess> builder)
    {
        builder.ToTable("PropertyAccess");

        builder.HasKey(pa => pa.Id);

        builder.Property(pa => pa.PropertyId)
            .IsRequired();

        builder.Property(pa => pa.UserId);

        builder.Property(pa => pa.InvitedEmail)
            .HasMaxLength(256);

        builder.Property(pa => pa.Role)
            .IsRequired();

        builder.Property(pa => pa.CreatedAt)
            .IsRequired();

        builder.Property(pa => pa.AcceptedAt);

        // Unique constraint: one access per user per property
        builder.HasIndex(pa => new { pa.PropertyId, pa.UserId })
            .IsUnique()
            .HasFilter("\"UserId\" IS NOT NULL");

        // Index for looking up user's properties
        builder.HasIndex(pa => pa.UserId);

        // Index for looking up pending invites by email
        builder.HasIndex(pa => pa.InvitedEmail)
            .HasFilter("\"InvitedEmail\" IS NOT NULL");

        // Foreign key to Property
        builder.HasOne<Property>()
            .WithMany()
            .HasForeignKey(pa => pa.PropertyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
