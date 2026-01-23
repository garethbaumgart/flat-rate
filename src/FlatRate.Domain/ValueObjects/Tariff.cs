namespace FlatRate.Domain.ValueObjects;

/// <summary>
/// Represents a tariff structure with one or more steps/tiers.
/// Uses decimal for precision in financial calculations.
/// </summary>
public sealed record Tariff
{
    /// <summary>
    /// Standard tier 1 upper limit (0-6 kL).
    /// </summary>
    public const decimal Tier1UpperLimit = 6m;

    /// <summary>
    /// Standard tier 2 upper limit (7-15 kL).
    /// </summary>
    public const decimal Tier2UpperLimit = 15m;

    public IReadOnlyList<TariffStep> Steps { get; }

    private Tariff(IReadOnlyList<TariffStep> steps)
    {
        Steps = steps;
    }

    /// <summary>
    /// Creates a flat-rate tariff (single tier).
    /// </summary>
    public static Tariff CreateFlatRate(decimal rate)
    {
        if (rate < 0)
            throw new ArgumentException("Rate cannot be negative.", nameof(rate));

        var steps = new List<TariffStep>
        {
            TariffStep.Create(decimal.MaxValue, rate)
        };

        return new Tariff(steps);
    }

    /// <summary>
    /// Creates a tiered tariff with the standard 3-tier structure for water/sanitation.
    /// Tier 1: 0-6 units
    /// Tier 2: 7-15 units
    /// Tier 3: 16+ units
    /// </summary>
    public static Tariff CreateTiered(decimal tier1Rate, decimal tier2Rate, decimal tier3Rate)
    {
        if (tier1Rate < 0)
            throw new ArgumentException("Tier 1 rate cannot be negative.", nameof(tier1Rate));
        if (tier2Rate < 0)
            throw new ArgumentException("Tier 2 rate cannot be negative.", nameof(tier2Rate));
        if (tier3Rate < 0)
            throw new ArgumentException("Tier 3 rate cannot be negative.", nameof(tier3Rate));

        var steps = new List<TariffStep>
        {
            TariffStep.Create(Tier1UpperLimit, tier1Rate),
            TariffStep.Create(Tier2UpperLimit, tier2Rate),
            TariffStep.Create(decimal.MaxValue, tier3Rate)
        };

        return new Tariff(steps);
    }

    /// <summary>
    /// Creates a tariff from a list of steps.
    /// The final step must have an upper limit of decimal.MaxValue to ensure all units are billed.
    /// </summary>
    public static Tariff Create(IEnumerable<TariffStep> steps)
    {
        ArgumentNullException.ThrowIfNull(steps);

        var stepsList = steps.ToList();

        if (stepsList.Count == 0)
            throw new ArgumentException("Tariff must have at least one step.", nameof(steps));

        // Ensure steps are ordered by upper limit
        var orderedSteps = stepsList.OrderBy(s => s.UpperLimit).ToList();

        // Check for duplicate upper limits
        var upperLimits = orderedSteps.Select(s => s.UpperLimit).ToList();
        if (upperLimits.Distinct().Count() != upperLimits.Count)
            throw new ArgumentException("Tariff steps cannot have duplicate upper limits.", nameof(steps));

        // Ensure final tier covers unlimited usage to prevent unbilled units
        if (orderedSteps[^1].UpperLimit != decimal.MaxValue)
            throw new ArgumentException("Final tariff step must have an unlimited upper limit (decimal.MaxValue) to ensure all units are billed.", nameof(steps));

        return new Tariff(orderedSteps);
    }
}
