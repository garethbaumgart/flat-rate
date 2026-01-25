using FlatRate.Application.Common;
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
    private readonly ICurrentUserService _currentUserService;

    public SetPropertyRatesCommandHandler(
        IPropertyRepository propertyRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _propertyRepository = propertyRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<bool> Handle(SetPropertyRatesCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId is null)
        {
            return false;
        }

        var property = await _propertyRepository.GetByIdAsync(request.PropertyId, cancellationToken);

        if (property is null)
        {
            return false;
        }

        // Verify user has access to this property
        var hasAccess = await _propertyRepository.UserHasAccessAsync(request.PropertyId, _currentUserService.UserId.Value, cancellationToken);
        if (!hasAccess)
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
