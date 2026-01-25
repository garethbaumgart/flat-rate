using FlatRate.Application.Common;
using FlatRate.Domain.Aggregates.Properties;
using FlatRate.Domain.Repositories;
using MediatR;

namespace FlatRate.Application.Properties.Commands.DeleteProperty;

/// <summary>
/// Handler for DeletePropertyCommand.
/// </summary>
public sealed class DeletePropertyCommandHandler : IRequestHandler<DeletePropertyCommand, bool>
{
    private readonly IPropertyRepository _propertyRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public DeletePropertyCommandHandler(
        IPropertyRepository propertyRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _propertyRepository = propertyRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<bool> Handle(DeletePropertyCommand request, CancellationToken cancellationToken)
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

        // Only Owner can delete a property
        var role = await _propertyRepository.GetUserRoleAsync(request.Id, _currentUserService.UserId.Value, cancellationToken);
        if (role != PropertyRole.Owner)
        {
            return false;
        }

        _propertyRepository.Delete(property);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
