using FlatRate.Domain.Aggregates.Properties;
using FlatRate.Domain.Repositories;
using MediatR;

namespace FlatRate.Application.Properties.Commands.SetPropertyRates;

/// <summary>
/// Handler for SetPropertyRatesCommand.
/// </summary>
public sealed class SetPropertyRatesCommandHandler : IRequestHandler<SetPropertyRatesCommand, bool>
{
    private readonly IPropertyRepository _propertyRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SetPropertyRatesCommandHandler(IPropertyRepository propertyRepository, IUnitOfWork unitOfWork)
    {
        _propertyRepository = propertyRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(SetPropertyRatesCommand request, CancellationToken cancellationToken)
    {
        var property = await _propertyRepository.GetByIdAsync(request.PropertyId, cancellationToken);

        if (property is null)
        {
            return false;
        }

        if (request.ElectricityRate.HasValue)
        {
            property.SetDefaultElectricityRate(request.ElectricityRate.Value);
        }

        if (request.WaterRateTier1.HasValue && request.WaterRateTier2.HasValue && request.WaterRateTier3.HasValue)
        {
            property.SetDefaultWaterRates(
                request.WaterRateTier1.Value,
                request.WaterRateTier2.Value,
                request.WaterRateTier3.Value);
        }

        if (request.SanitationRateTier1.HasValue && request.SanitationRateTier2.HasValue && request.SanitationRateTier3.HasValue)
        {
            property.SetDefaultSanitationRates(
                request.SanitationRateTier1.Value,
                request.SanitationRateTier2.Value,
                request.SanitationRateTier3.Value);
        }

        // EF Core tracks changes automatically, just need to save
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
