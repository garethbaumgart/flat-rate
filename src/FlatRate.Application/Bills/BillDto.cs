namespace FlatRate.Application.Bills;

/// <summary>
/// Data transfer object for Bill.
/// </summary>
public sealed record BillDto(
    Guid Id,
    string InvoiceNumber,
    Guid PropertyId,
    DateTimeOffset PeriodStart,
    DateTimeOffset PeriodEnd,
    MeterReadingDto ElectricityReading,
    MeterReadingDto WaterReading,
    MeterReadingDto SanitationReading,
    decimal ElectricityCost,
    decimal WaterCost,
    decimal SanitationCost,
    decimal Subtotal,
    decimal VatAmount,
    decimal Total,
    DateTimeOffset CreatedAt);

/// <summary>
/// Data transfer object for meter reading.
/// </summary>
public sealed record MeterReadingDto(
    decimal Opening,
    decimal Closing,
    decimal UnitsUsed);
