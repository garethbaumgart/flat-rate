using FlatRate.Domain.Aggregates.Bills;
using MediatR;

namespace FlatRate.Application.Bills.Queries.GetAllBills;

/// <summary>
/// Handler for GetAllBillsQuery.
/// </summary>
public sealed class GetAllBillsQueryHandler : IRequestHandler<GetAllBillsQuery, IReadOnlyList<BillDto>>
{
    private readonly IBillRepository _billRepository;

    public GetAllBillsQueryHandler(IBillRepository billRepository)
    {
        _billRepository = billRepository;
    }

    public async Task<IReadOnlyList<BillDto>> Handle(GetAllBillsQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyList<Bill> bills;

        if (request.PropertyId.HasValue)
        {
            bills = await _billRepository.GetByPropertyIdAsync(request.PropertyId.Value, cancellationToken);
        }
        else
        {
            bills = await _billRepository.GetAllAsync(cancellationToken);
        }

        return bills.Select(MapToDto).ToList();
    }

    private static BillDto MapToDto(Bill bill) => new(
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
