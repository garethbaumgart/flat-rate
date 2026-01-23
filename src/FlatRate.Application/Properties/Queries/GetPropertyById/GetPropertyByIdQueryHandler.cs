using FlatRate.Domain.Aggregates.Properties;
using MediatR;

namespace FlatRate.Application.Properties.Queries.GetPropertyById;

/// <summary>
/// Handler for GetPropertyByIdQuery.
/// </summary>
public sealed class GetPropertyByIdQueryHandler : IRequestHandler<GetPropertyByIdQuery, PropertyDto?>
{
    private readonly IPropertyRepository _propertyRepository;

    public GetPropertyByIdQueryHandler(IPropertyRepository propertyRepository)
    {
        _propertyRepository = propertyRepository;
    }

    public async Task<PropertyDto?> Handle(GetPropertyByIdQuery request, CancellationToken cancellationToken)
    {
        var property = await _propertyRepository.GetByIdAsync(request.Id, cancellationToken);

        if (property is null)
        {
            return null;
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
            property.UpdatedAt
        );
    }
}
