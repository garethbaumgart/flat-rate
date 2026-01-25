using FlatRate.Application.Common;
using FlatRate.Domain.Aggregates.Bills;
using FlatRate.Domain.Aggregates.Properties;
using MediatR;

namespace FlatRate.Application.Bills.Queries.GetAllBills;

/// <summary>
/// Handler for GetAllBillsQuery.
/// </summary>
public sealed class GetAllBillsQueryHandler : IRequestHandler<GetAllBillsQuery, IReadOnlyList<BillDto>>
{
    private readonly IBillRepository _billRepository;
    private readonly IPropertyRepository _propertyRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetAllBillsQueryHandler(
        IBillRepository billRepository,
        IPropertyRepository propertyRepository,
        ICurrentUserService currentUserService)
    {
        _billRepository = billRepository;
        _propertyRepository = propertyRepository;
        _currentUserService = currentUserService;
    }

    public async Task<IReadOnlyList<BillDto>> Handle(GetAllBillsQuery request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId is null)
        {
            return [];
        }

        IReadOnlyList<Bill> bills;

        if (request.PropertyId.HasValue)
        {
            // Verify user has access to this property
            var hasAccess = await _propertyRepository.UserHasAccessAsync(
                request.PropertyId.Value,
                _currentUserService.UserId.Value,
                cancellationToken);

            if (!hasAccess)
            {
                return [];
            }

            bills = await _billRepository.GetByPropertyIdAsync(request.PropertyId.Value, cancellationToken);
        }
        else
        {
            // Get bills only for properties the user has access to
            var accessibleProperties = await _propertyRepository.GetByUserIdAsync(_currentUserService.UserId.Value, cancellationToken);
            var propertyIds = accessibleProperties.Select(p => p.Id).ToList();

            bills = await _billRepository.GetByPropertyIdsAsync(propertyIds, cancellationToken);
        }

        return bills.Select(BillMapper.ToDto).ToList();
    }
}
