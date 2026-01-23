using FlatRate.Domain.Aggregates.Bills;
using FlatRate.Domain.Repositories;
using MediatR;

namespace FlatRate.Application.Bills.Commands.DeleteBill;

/// <summary>
/// Handler for DeleteBillCommand.
/// </summary>
public sealed class DeleteBillCommandHandler : IRequestHandler<DeleteBillCommand, bool>
{
    private readonly IBillRepository _billRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteBillCommandHandler(IBillRepository billRepository, IUnitOfWork unitOfWork)
    {
        _billRepository = billRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteBillCommand request, CancellationToken cancellationToken)
    {
        var bill = await _billRepository.GetByIdAsync(request.Id, cancellationToken);

        if (bill is null)
        {
            return false;
        }

        _billRepository.Delete(bill);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
