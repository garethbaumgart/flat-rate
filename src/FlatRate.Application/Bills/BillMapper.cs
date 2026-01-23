using FlatRate.Domain.Aggregates.Bills;

namespace FlatRate.Application.Bills;

/// <summary>
/// Mapper for Bill aggregate to DTO conversions.
/// </summary>
public static class BillMapper
{
    public static BillDto ToDto(Bill bill) => new(
        bill.Id,
        bill.InvoiceNumber,
        bill.PropertyId,
        bill.PeriodStart,
        bill.PeriodEnd,
        new MeterReadingDto(bill.ElectricityReading.Opening, bill.ElectricityReading.Closing, bill.ElectricityReading.UnitsUsed),
        new MeterReadingDto(bill.WaterReading.Opening, bill.WaterReading.Closing, bill.WaterReading.UnitsUsed),
        new MeterReadingDto(bill.SanitationReading.Opening, bill.SanitationReading.Closing, bill.SanitationReading.UnitsUsed),
        bill.ElectricityCost,
        bill.WaterCost,
        bill.SanitationCost,
        bill.Subtotal,
        bill.VatAmount,
        bill.Total,
        bill.CreatedAt
    );
}
