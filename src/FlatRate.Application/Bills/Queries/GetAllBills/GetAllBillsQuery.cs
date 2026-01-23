using MediatR;

namespace FlatRate.Application.Bills.Queries.GetAllBills;

/// <summary>
/// Query to get all bills for a property.
/// </summary>
public sealed record GetAllBillsQuery(Guid? PropertyId = null) : IRequest<IReadOnlyList<BillDto>>;
