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

        return bill is null ? null : BillMapper.ToDto(bill);
    }
}
