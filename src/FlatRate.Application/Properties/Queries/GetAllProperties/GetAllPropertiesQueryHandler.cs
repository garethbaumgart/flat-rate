using FlatRate.Domain.Aggregates.Properties;
using MediatR;

namespace FlatRate.Application.Properties.Queries.GetAllProperties;

/// <summary>
/// Handler for GetAllPropertiesQuery.
/// </summary>
public sealed class GetAllPropertiesQueryHandler : IRequestHandler<GetAllPropertiesQuery, IReadOnlyList<PropertyDto>>
{
    private readonly IPropertyRepository _propertyRepository;

    public GetAllPropertiesQueryHandler(IPropertyRepository propertyRepository)
    {
        _propertyRepository = propertyRepository;
    }

    public async Task<IReadOnlyList<PropertyDto>> Handle(GetAllPropertiesQuery request, CancellationToken cancellationToken)
    {
        var properties = await _propertyRepository.GetAllAsync(cancellationToken);

        return properties.Select(p => new PropertyDto(
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
            p.UpdatedAt
        )).ToList();
    }
}
