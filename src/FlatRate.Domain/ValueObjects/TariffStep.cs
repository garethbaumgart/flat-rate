namespace FlatRate.Domain.ValueObjects;

/// <summary>
/// Represents a single tier/step in a tariff structure.
/// </summary>
public sealed record TariffStep
{
    /// <summary>
    /// The upper limit of units for this tier (inclusive).
    /// </summary>
    public double UpperLimit { get; }

    /// <summary>
    /// The rate per unit for this tier.
    /// </summary>
    public decimal Rate { get; }

    private TariffStep(double upperLimit, decimal rate)
    {
        UpperLimit = upperLimit;
        Rate = rate;
    }

    public static TariffStep Create(double upperLimit, decimal rate)
    {
        if (upperLimit <= 0)
            throw new ArgumentException("Upper limit must be greater than zero.", nameof(upperLimit));

        if (rate < 0)
            throw new ArgumentException("Rate cannot be negative.", nameof(rate));

        return new TariffStep(upperLimit, rate);
    }
}
