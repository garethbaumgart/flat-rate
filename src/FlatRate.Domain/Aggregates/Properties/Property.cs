using FlatRate.Domain.Common;

namespace FlatRate.Domain.Aggregates.Properties;

/// <summary>
/// Represents a rental property with default utility rates.
/// </summary>
public sealed class Property : AggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public string Address { get; private set; } = string.Empty;

    // Default electricity rate (flat rate per kWh)
    public decimal? DefaultElectricityRate { get; private set; }

    // Default water rates (tiered per kL)
    public decimal? DefaultWaterRateTier1 { get; private set; }
    public decimal? DefaultWaterRateTier2 { get; private set; }
    public decimal? DefaultWaterRateTier3 { get; private set; }

    // Default sanitation rates (tiered per kL)
    public decimal? DefaultSanitationRateTier1 { get; private set; }
    public decimal? DefaultSanitationRateTier2 { get; private set; }
    public decimal? DefaultSanitationRateTier3 { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private Property() : base()
    {
    }

    public static Property Create(string name, string address)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Property name cannot be empty.", nameof(name));

        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException("Property address cannot be empty.", nameof(address));

        return new Property
        {
            Name = name.Trim(),
            Address = address.Trim(),
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string name, string address)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Property name cannot be empty.", nameof(name));

        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException("Property address cannot be empty.", nameof(address));

        Name = name.Trim();
        Address = address.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetDefaultElectricityRate(decimal rate)
    {
        if (rate < 0)
            throw new ArgumentException("Electricity rate cannot be negative.", nameof(rate));

        DefaultElectricityRate = rate;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetDefaultWaterRates(decimal tier1Rate, decimal tier2Rate, decimal tier3Rate)
    {
        if (tier1Rate < 0)
            throw new ArgumentException("Tier 1 rate cannot be negative.", nameof(tier1Rate));
        if (tier2Rate < 0)
            throw new ArgumentException("Tier 2 rate cannot be negative.", nameof(tier2Rate));
        if (tier3Rate < 0)
            throw new ArgumentException("Tier 3 rate cannot be negative.", nameof(tier3Rate));

        DefaultWaterRateTier1 = tier1Rate;
        DefaultWaterRateTier2 = tier2Rate;
        DefaultWaterRateTier3 = tier3Rate;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetDefaultSanitationRates(decimal tier1Rate, decimal tier2Rate, decimal tier3Rate)
    {
        if (tier1Rate < 0)
            throw new ArgumentException("Tier 1 rate cannot be negative.", nameof(tier1Rate));
        if (tier2Rate < 0)
            throw new ArgumentException("Tier 2 rate cannot be negative.", nameof(tier2Rate));
        if (tier3Rate < 0)
            throw new ArgumentException("Tier 3 rate cannot be negative.", nameof(tier3Rate));

        DefaultSanitationRateTier1 = tier1Rate;
        DefaultSanitationRateTier2 = tier2Rate;
        DefaultSanitationRateTier3 = tier3Rate;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ClearDefaultRates()
    {
        DefaultElectricityRate = null;
        DefaultWaterRateTier1 = null;
        DefaultWaterRateTier2 = null;
        DefaultWaterRateTier3 = null;
        DefaultSanitationRateTier1 = null;
        DefaultSanitationRateTier2 = null;
        DefaultSanitationRateTier3 = null;
        UpdatedAt = DateTime.UtcNow;
    }
}
