using MediatR;

namespace FlatRate.Application.Bills.Commands.DeleteBill;

/// <summary>
/// Command to delete a bill.
/// </summary>
public sealed record DeleteBillCommand(Guid Id) : IRequest<bool>;
