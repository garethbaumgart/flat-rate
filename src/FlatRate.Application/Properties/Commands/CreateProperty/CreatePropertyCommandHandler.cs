using FlatRate.Application.Common;
using FlatRate.Domain.Aggregates.Properties;
using FlatRate.Domain.Repositories;
using MediatR;

namespace FlatRate.Application.Properties.Commands.CreateProperty;

/// <summary>
/// Handler for CreatePropertyCommand.
/// </summary>
public sealed class CreatePropertyCommandHandler : IRequestHandler<CreatePropertyCommand, Guid>
{
    private readonly IPropertyRepository _propertyRepository;
    private readonly IPropertyAccessRepository _propertyAccessRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public CreatePropertyCommandHandler(
        IPropertyRepository propertyRepository,
        IPropertyAccessRepository propertyAccessRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _propertyRepository = propertyRepository;
        _propertyAccessRepository = propertyAccessRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreatePropertyCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User must be authenticated to create a property.");

        var property = Property.Create(request.Name, request.Address);
        await _propertyRepository.AddAsync(property, cancellationToken);

        // Create owner access for the current user
        var access = PropertyAccess.CreateForUser(property.Id, userId, PropertyRole.Owner);
        await _propertyAccessRepository.AddAsync(access, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return property.Id;
    }
}
