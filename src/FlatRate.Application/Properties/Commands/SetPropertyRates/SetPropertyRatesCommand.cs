using MediatR;

namespace FlatRate.Application.Properties.Commands.SetPropertyRates;

/// <summary>
/// Command to set default utility rates for a property.
/// </summary>
public sealed record SetPropertyRatesCommand(
    Guid PropertyId,
    decimal? ElectricityRate,
    decimal? WaterRateTier1,
    decimal? WaterRateTier2,
    decimal? WaterRateTier3,
    decimal? SanitationRateTier1,
    decimal? SanitationRateTier2,
    decimal? SanitationRateTier3) : IRequest<bool>;
