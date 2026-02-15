using FlatRate.Domain.Common;
using FlatRate.Domain.ValueObjects;

namespace FlatRate.Domain.Aggregates.Bills;

/// <summary>
/// Represents a utility bill for a rental property.
/// </summary>
public sealed class Bill : AggregateRoot
{
    public string InvoiceNumber { get; private set; } = string.Empty;
    public Guid PropertyId { get; private set; }

    public DateTimeOffset PeriodStart { get; private set; }
    public DateTimeOffset PeriodEnd { get; private set; }

    // Meter readings
    public MeterReading ElectricityReading { get; private set; } = null!;
    public MeterReading WaterReading { get; private set; } = null!;
    public MeterReading SanitationReading { get; private set; } = null!;

    // Tariffs
    public Tariff ElectricityTariff { get; private set; } = null!;
    public Tariff WaterTariff { get; private set; } = null!;
    public Tariff SanitationTariff { get; private set; } = null!;

    // Calculated costs (stored for historical accuracy)
    public decimal ElectricityCost { get; private set; }
    public decimal WaterCost { get; private set; }
    public decimal SanitationCost { get; private set; }
    public decimal Subtotal { get; private set; }
    public decimal VatAmount { get; private set; }
    public decimal Total { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    private Bill() : base()
    {
    }

    public static Bill Create(
        string invoiceNumber,
        Guid propertyId,
        DateTimeOffset periodStart,
        DateTimeOffset periodEnd,
        MeterReading electricityReading,
        MeterReading waterReading,
        MeterReading sanitationReading,
        Tariff electricityTariff,
        Tariff waterTariff,
        Tariff sanitationTariff)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(invoiceNumber))
            throw new ArgumentException("Invoice number cannot be empty.", nameof(invoiceNumber));

        if (propertyId == Guid.Empty)
            throw new ArgumentException("Property ID cannot be empty.", nameof(propertyId));

        if (periodEnd < periodStart)
            throw new ArgumentException("Period end date must be greater than or equal to period start date.", nameof(periodEnd));

        ArgumentNullException.ThrowIfNull(electricityReading);
        ArgumentNullException.ThrowIfNull(waterReading);
        ArgumentNullException.ThrowIfNull(sanitationReading);
        ArgumentNullException.ThrowIfNull(electricityTariff);
        ArgumentNullException.ThrowIfNull(waterTariff);
        ArgumentNullException.ThrowIfNull(sanitationTariff);

        var bill = new Bill
        {
            InvoiceNumber = invoiceNumber.Trim(),
            PropertyId = propertyId,
            PeriodStart = periodStart.ToUniversalTime(),
            PeriodEnd = periodEnd.ToUniversalTime(),
            ElectricityReading = electricityReading,
            WaterReading = waterReading,
            SanitationReading = sanitationReading,
            ElectricityTariff = electricityTariff,
            WaterTariff = waterTariff,
            SanitationTariff = sanitationTariff,
            CreatedAt = DateTimeOffset.UtcNow
        };

        // Calculate costs
        bill.CalculateCosts();

        return bill;
    }

    private void CalculateCosts()
    {
        // Calculate individual utility costs
        ElectricityCost = BillingCalculator.CalculateElectricityCost(
            ElectricityReading.UnitsUsed,
            ElectricityTariff);

        WaterCost = BillingCalculator.CalculateTieredCost(
            WaterReading.UnitsUsed,
            WaterTariff);

        SanitationCost = BillingCalculator.CalculateTieredCost(
            SanitationReading.UnitsUsed,
            SanitationTariff);

        // Calculate totals
        Subtotal = ElectricityCost + WaterCost + SanitationCost;
        VatAmount = BillingCalculator.CalculateVat(Subtotal);
        Total = Subtotal + VatAmount;
    }

    /// <summary>
    /// Recalculates costs based on current readings and tariffs.
    /// Useful for previewing or updating a bill.
    /// </summary>
    public void Recalculate()
    {
        CalculateCosts();
    }
}
