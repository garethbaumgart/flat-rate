namespace FlatRate.Application.Properties;

/// <summary>
/// Data transfer object for Property.
/// </summary>
public sealed record PropertyDto(
    Guid Id,
    string Name,
    string Address,
    decimal? DefaultElectricityRate,
    decimal? DefaultWaterRateTier1,
    decimal? DefaultWaterRateTier2,
    decimal? DefaultWaterRateTier3,
    decimal? DefaultSanitationRateTier1,
    decimal? DefaultSanitationRateTier2,
    decimal? DefaultSanitationRateTier3,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
