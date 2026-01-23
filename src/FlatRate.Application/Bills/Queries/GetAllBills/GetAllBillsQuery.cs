using MediatR;

namespace FlatRate.Application.Bills.Queries.GetAllBills;

/// <summary>
/// Query to get bills. If PropertyId is provided, returns bills for that property.
/// If PropertyId is null, returns all bills.
/// </summary>
public sealed record GetAllBillsQuery(Guid? PropertyId = null) : IRequest<IReadOnlyList<BillDto>>;
