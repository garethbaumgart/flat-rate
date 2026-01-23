using FlatRate.Domain.Aggregates.Properties;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlatRate.Infrastructure.Persistence.Configurations;

public class PropertyConfiguration : IEntityTypeConfiguration<Property>
{
    public void Configure(EntityTypeBuilder<Property> builder)
    {
        builder.ToTable("Properties");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Address)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(p => p.DefaultElectricityRate)
            .HasPrecision(18, 4);

        builder.Property(p => p.DefaultWaterRateTier1)
            .HasPrecision(18, 4);

        builder.Property(p => p.DefaultWaterRateTier2)
            .HasPrecision(18, 4);

        builder.Property(p => p.DefaultWaterRateTier3)
            .HasPrecision(18, 4);

        builder.Property(p => p.DefaultSanitationRateTier1)
            .HasPrecision(18, 4);

        builder.Property(p => p.DefaultSanitationRateTier2)
            .HasPrecision(18, 4);

        builder.Property(p => p.DefaultSanitationRateTier3)
            .HasPrecision(18, 4);

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        builder.Property(p => p.UpdatedAt);
    }
}
