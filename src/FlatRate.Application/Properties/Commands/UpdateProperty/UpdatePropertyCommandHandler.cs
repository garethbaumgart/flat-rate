using FlatRate.Application.Common;
using FlatRate.Domain.Aggregates.Properties;
using FlatRate.Domain.Repositories;
using MediatR;

namespace FlatRate.Application.Properties.Commands.UpdateProperty;

/// <summary>
/// Handler for UpdatePropertyCommand.
/// </summary>
public sealed class UpdatePropertyCommandHandler : IRequestHandler<UpdatePropertyCommand, bool>
{
    private readonly IPropertyRepository _propertyRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public UpdatePropertyCommandHandler(
        IPropertyRepository propertyRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _propertyRepository = propertyRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<bool> Handle(UpdatePropertyCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId is null)
        {
            return false;
        }

        var property = await _propertyRepository.GetByIdAsync(request.Id, cancellationToken);

        if (property is null)
        {
            return false;
        }

        // Verify user has access to this property
        var hasAccess = await _propertyRepository.UserHasAccessAsync(request.Id, _currentUserService.UserId.Value, cancellationToken);
        if (!hasAccess)
        {
            return false;
        }

        property.Update(request.Name, request.Address);

        // EF Core tracks changes automatically, just need to save
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
