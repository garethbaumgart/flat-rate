using FlatRate.Application.Common;
using FlatRate.Domain.Aggregates.Properties;
using FlatRate.Domain.Repositories;
using MediatR;

namespace FlatRate.Application.Properties.Commands.RevokePropertyAccess;

/// <summary>
/// Handler for RevokePropertyAccessCommand.
/// </summary>
public sealed class RevokePropertyAccessCommandHandler : IRequestHandler<RevokePropertyAccessCommand, RevokePropertyAccessResult>
{
    private readonly IPropertyRepository _propertyRepository;
    private readonly IPropertyAccessRepository _propertyAccessRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public RevokePropertyAccessCommandHandler(
        IPropertyRepository propertyRepository,
        IPropertyAccessRepository propertyAccessRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _propertyRepository = propertyRepository;
        _propertyAccessRepository = propertyAccessRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<RevokePropertyAccessResult> Handle(RevokePropertyAccessCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId is null)
        {
            return new RevokePropertyAccessResult(false, "User must be authenticated.");
        }

        // Verify property exists
        var property = await _propertyRepository.GetByIdAsync(request.PropertyId, cancellationToken);
        if (property is null)
        {
            return new RevokePropertyAccessResult(false, "Property not found.");
        }

        // Only owner can revoke access
        var currentUserRole = await _propertyRepository.GetUserRoleAsync(request.PropertyId, _currentUserService.UserId.Value, cancellationToken);
        if (currentUserRole != PropertyRole.Owner)
        {
            return new RevokePropertyAccessResult(false, "Only the property owner can revoke access.");
        }

        // Cannot revoke your own access
        if (request.UserId == _currentUserService.UserId.Value)
        {
            return new RevokePropertyAccessResult(false, "You cannot revoke your own access.");
        }

        // Find the access record
        var access = await _propertyAccessRepository.GetByPropertyAndUserAsync(request.PropertyId, request.UserId, cancellationToken);
        if (access is null)
        {
            return new RevokePropertyAccessResult(false, "User does not have access to this property.");
        }

        _propertyAccessRepository.Delete(access);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new RevokePropertyAccessResult(true);
    }
}
