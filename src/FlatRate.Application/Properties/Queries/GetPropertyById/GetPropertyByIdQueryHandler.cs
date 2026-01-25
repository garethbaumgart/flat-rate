using FlatRate.Application.Common;
using FlatRate.Domain.Aggregates.Properties;
using MediatR;

namespace FlatRate.Application.Properties.Queries.GetPropertyById;

/// <summary>
/// Handler for GetPropertyByIdQuery.
/// Validates that the current user has access to the property.
/// </summary>
public sealed class GetPropertyByIdQueryHandler : IRequestHandler<GetPropertyByIdQuery, PropertyDto?>
{
    private readonly IPropertyRepository _propertyRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetPropertyByIdQueryHandler(
        IPropertyRepository propertyRepository,
        ICurrentUserService currentUserService)
    {
        _propertyRepository = propertyRepository;
        _currentUserService = currentUserService;
    }

    public async Task<PropertyDto?> Handle(GetPropertyByIdQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User must be authenticated.");

        var property = await _propertyRepository.GetByIdAsync(request.Id, cancellationToken);

        if (property is null)
        {
            return null;
        }

        // Validate user has access
        var role = await _propertyRepository.GetUserRoleAsync(property.Id, userId, cancellationToken);
        if (role is null)
        {
            return null; // User doesn't have access - treat as not found
        }

        return new PropertyDto(
            property.Id,
            property.Name,
            property.Address,
            property.DefaultElectricityRate,
            property.DefaultWaterRateTier1,
            property.DefaultWaterRateTier2,
            property.DefaultWaterRateTier3,
            property.DefaultSanitationRateTier1,
            property.DefaultSanitationRateTier2,
            property.DefaultSanitationRateTier3,
            property.CreatedAt,
            property.UpdatedAt,
            role.Value
        );
    }
}
