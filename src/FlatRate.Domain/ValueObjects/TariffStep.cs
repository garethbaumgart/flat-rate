namespace FlatRate.Domain.ValueObjects;

/// <summary>
/// Represents a single tier/step in a tariff structure.
/// Uses decimal for precision in financial calculations.
/// </summary>
public sealed record TariffStep
{
    /// <summary>
    /// The upper limit of units for this tier (inclusive).
    /// </summary>
    public decimal UpperLimit { get; }

    /// <summary>
    /// The rate per unit for this tier.
    /// </summary>
    public decimal Rate { get; }

    /// <summary>
    /// Private parameterless constructor for EF Core materialization.
    /// </summary>
    private TariffStep()
    {
        UpperLimit = 0;
        Rate = 0;
    }

    private TariffStep(decimal upperLimit, decimal rate)
    {
        UpperLimit = upperLimit;
        Rate = rate;
    }

    public static TariffStep Create(decimal upperLimit, decimal rate)
    {
        if (upperLimit <= 0)
            throw new ArgumentException("Upper limit must be greater than zero.", nameof(upperLimit));

        if (rate < 0)
            throw new ArgumentException("Rate cannot be negative.", nameof(rate));

        return new TariffStep(upperLimit, rate);
    }
}
