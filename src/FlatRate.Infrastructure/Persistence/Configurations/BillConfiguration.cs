using FlatRate.Domain.Aggregates.Bills;
using FlatRate.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlatRate.Infrastructure.Persistence.Configurations;

public class BillConfiguration : IEntityTypeConfiguration<Bill>
{
    public void Configure(EntityTypeBuilder<Bill> builder)
    {
        builder.ToTable("Bills");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.InvoiceNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(b => b.InvoiceNumber)
            .IsUnique();

        builder.Property(b => b.PropertyId)
            .IsRequired();

        builder.HasIndex(b => b.PropertyId);

        builder.Property(b => b.PeriodStart)
            .IsRequired();

        builder.Property(b => b.PeriodEnd)
            .IsRequired();

        // Electricity Reading (owned entity)
        builder.OwnsOne(b => b.ElectricityReading, reading =>
        {
            reading.Property(r => r.Opening)
                .HasColumnName("ElectricityReadingOpening")
                .HasPrecision(18, 4)
                .IsRequired();

            reading.Property(r => r.Closing)
                .HasColumnName("ElectricityReadingClosing")
                .HasPrecision(18, 4)
                .IsRequired();

            reading.Ignore(r => r.UnitsUsed);
        });

        // Water Reading (owned entity)
        builder.OwnsOne(b => b.WaterReading, reading =>
        {
            reading.Property(r => r.Opening)
                .HasColumnName("WaterReadingOpening")
                .HasPrecision(18, 4)
                .IsRequired();

            reading.Property(r => r.Closing)
                .HasColumnName("WaterReadingClosing")
                .HasPrecision(18, 4)
                .IsRequired();

            reading.Ignore(r => r.UnitsUsed);
        });

        // Sanitation Reading (owned entity)
        builder.OwnsOne(b => b.SanitationReading, reading =>
        {
            reading.Property(r => r.Opening)
                .HasColumnName("SanitationReadingOpening")
                .HasPrecision(18, 4)
                .IsRequired();

            reading.Property(r => r.Closing)
                .HasColumnName("SanitationReadingClosing")
                .HasPrecision(18, 4)
                .IsRequired();

            reading.Ignore(r => r.UnitsUsed);
        });

        // Store tariffs as JSON for flexibility
        builder.OwnsOne(b => b.ElectricityTariff, ConfigureTariff("Electricity"));
        builder.OwnsOne(b => b.WaterTariff, ConfigureTariff("Water"));
        builder.OwnsOne(b => b.SanitationTariff, ConfigureTariff("Sanitation"));

        // Calculated costs
        builder.Property(b => b.ElectricityCost)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(b => b.WaterCost)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(b => b.SanitationCost)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(b => b.Subtotal)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(b => b.VatAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(b => b.Total)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(b => b.CreatedAt)
            .IsRequired();
    }

    private static Action<OwnedNavigationBuilder<Bill, Tariff>> ConfigureTariff(string prefix)
    {
        return tariff =>
        {
            tariff.ToJson($"{prefix}Tariff");
            tariff.OwnsMany(t => t.Steps);
        };
    }
}
