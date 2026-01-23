using MediatR;

namespace FlatRate.Application.Bills.Queries.GetBillById;

/// <summary>
/// Query to get a bill by ID.
/// </summary>
public sealed record GetBillByIdQuery(Guid Id) : IRequest<BillDto?>;
