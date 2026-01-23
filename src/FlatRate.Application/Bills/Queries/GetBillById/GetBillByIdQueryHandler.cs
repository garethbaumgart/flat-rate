using FlatRate.Domain.Aggregates.Bills;
using MediatR;

namespace FlatRate.Application.Bills.Queries.GetBillById;

/// <summary>
/// Handler for GetBillByIdQuery.
/// </summary>
public sealed class GetBillByIdQueryHandler : IRequestHandler<GetBillByIdQuery, BillDto?>
{
    private readonly IBillRepository _billRepository;

    public GetBillByIdQueryHandler(IBillRepository billRepository)
    {
        _billRepository = billRepository;
    }

    public async Task<BillDto?> Handle(GetBillByIdQuery request, CancellationToken cancellationToken)
    {
        var bill = await _billRepository.GetByIdAsync(request.Id, cancellationToken);

        if (bill is null)
        {
            return null;
        }

        return new BillDto(
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
}
