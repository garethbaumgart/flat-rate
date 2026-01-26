using FlatRate.Application.Common;
using FlatRate.Domain.Aggregates.Properties;
using MediatR;

namespace FlatRate.Application.Properties.Queries.GetAllProperties;

/// <summary>
/// Handler for GetAllPropertiesQuery.
/// Returns only properties the current user has access to.
/// </summary>
public sealed class GetAllPropertiesQueryHandler : IRequestHandler<GetAllPropertiesQuery, IReadOnlyList<PropertyDto>>
{
    private readonly IPropertyRepository _propertyRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetAllPropertiesQueryHandler(
        IPropertyRepository propertyRepository,
        ICurrentUserService currentUserService)
    {
        _propertyRepository = propertyRepository;
        _currentUserService = currentUserService;
    }

    public async Task<IReadOnlyList<PropertyDto>> Handle(GetAllPropertiesQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User must be authenticated.");

        // Get only properties the user has access to
        var properties = await _propertyRepository.GetByUserIdAsync(userId, cancellationToken);

        var result = new List<PropertyDto>();
        foreach (var p in properties)
        {
            var role = await _propertyRepository.GetUserRoleAsync(p.Id, userId, cancellationToken);
            result.Add(new PropertyDto(
                p.Id,
                p.Name,
                p.Address,
                p.DefaultElectricityRate,
                p.DefaultWaterRateTier1,
                p.DefaultWaterRateTier2,
                p.DefaultWaterRateTier3,
                p.DefaultSanitationRateTier1,
                p.DefaultSanitationRateTier2,
                p.DefaultSanitationRateTier3,
                p.CreatedAt,
                p.UpdatedAt,
                role ?? PropertyRole.Editor // Default to Editor if somehow role is missing
            ));
        }

        return result;
    }
}
