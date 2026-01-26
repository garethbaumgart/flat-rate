using FlatRate.Application.Common;
using FlatRate.Domain.Aggregates.Properties;
using FlatRate.Domain.Aggregates.Users;
using MediatR;

namespace FlatRate.Application.Properties.Queries.GetPropertyCollaborators;

/// <summary>
/// Handler for GetPropertyCollaboratorsQuery.
/// </summary>
public sealed class GetPropertyCollaboratorsQueryHandler : IRequestHandler<GetPropertyCollaboratorsQuery, IReadOnlyList<CollaboratorDto>>
{
    private readonly IPropertyRepository _propertyRepository;
    private readonly IPropertyAccessRepository _propertyAccessRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetPropertyCollaboratorsQueryHandler(
        IPropertyRepository propertyRepository,
        IPropertyAccessRepository propertyAccessRepository,
        IUserRepository userRepository,
        ICurrentUserService currentUserService)
    {
        _propertyRepository = propertyRepository;
        _propertyAccessRepository = propertyAccessRepository;
        _userRepository = userRepository;
        _currentUserService = currentUserService;
    }

    public async Task<IReadOnlyList<CollaboratorDto>> Handle(GetPropertyCollaboratorsQuery request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId is null)
        {
            return [];
        }

        // Verify user has access to this property
        var hasAccess = await _propertyRepository.UserHasAccessAsync(request.PropertyId, _currentUserService.UserId.Value, cancellationToken);
        if (!hasAccess)
        {
            return [];
        }

        var accessRecords = await _propertyAccessRepository.GetByPropertyIdAsync(request.PropertyId, cancellationToken);
        var collaborators = new List<CollaboratorDto>();

        foreach (var access in accessRecords)
        {
            if (access.UserId.HasValue)
            {
                var user = await _userRepository.GetByIdAsync(access.UserId.Value, cancellationToken);
                collaborators.Add(new CollaboratorDto(
                    access.UserId,
                    user?.Email,
                    user?.Name,
                    access.Role,
                    IsPending: false,
                    access.CreatedAt,
                    access.AcceptedAt));
            }
            else if (!string.IsNullOrEmpty(access.InvitedEmail))
            {
                collaborators.Add(new CollaboratorDto(
                    null,
                    access.InvitedEmail,
                    null,
                    access.Role,
                    IsPending: true,
                    access.CreatedAt,
                    null));
            }
        }

        return collaborators;
    }
}
