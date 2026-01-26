using FlatRate.Application.Common;
using FlatRate.Domain.Aggregates.Bills;
using FlatRate.Domain.Aggregates.Properties;
using FlatRate.Domain.Repositories;
using MediatR;

namespace FlatRate.Application.Bills.Commands.DeleteBill;

/// <summary>
/// Handler for DeleteBillCommand.
/// </summary>
public sealed class DeleteBillCommandHandler : IRequestHandler<DeleteBillCommand, bool>
{
    private readonly IBillRepository _billRepository;
    private readonly IPropertyRepository _propertyRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public DeleteBillCommandHandler(
        IBillRepository billRepository,
        IPropertyRepository propertyRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _billRepository = billRepository;
        _propertyRepository = propertyRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<bool> Handle(DeleteBillCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId is null)
        {
            return false;
        }

        var bill = await _billRepository.GetByIdAsync(request.Id, cancellationToken);

        if (bill is null)
        {
            return false;
        }

        // Verify user has access to the property this bill belongs to
        var hasAccess = await _propertyRepository.UserHasAccessAsync(
            bill.PropertyId,
            _currentUserService.UserId.Value,
            cancellationToken);

        if (!hasAccess)
        {
            return false;
        }

        _billRepository.Delete(bill);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
