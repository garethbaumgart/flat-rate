using FlatRate.Domain.ValueObjects;

namespace FlatRate.Domain.Aggregates.Bills;

/// <summary>
/// Handles all billing calculations for utilities.
/// </summary>
public static class BillingCalculator
{
    /// <summary>
    /// VAT rate in South Africa (15%).
    /// </summary>
    public const decimal VatRate = 0.15m;

    /// <summary>
    /// Calculates the cost for electricity using a flat rate.
    /// </summary>
    /// <param name="unitsUsed">Units consumed in kWh.</param>
    /// <param name="tariff">The electricity tariff (flat rate).</param>
    /// <returns>The total cost before VAT.</returns>
    public static decimal CalculateElectricityCost(decimal unitsUsed, Tariff tariff)
    {
        ArgumentNullException.ThrowIfNull(tariff);

        if (unitsUsed < 0)
            throw new ArgumentException("Units used cannot be negative.", nameof(unitsUsed));

        if (tariff.Steps.Count == 0)
            throw new ArgumentException("Tariff must have at least one step.", nameof(tariff));

        // For electricity, we use flat rate (first step)
        var rate = tariff.Steps[0].Rate;
        return unitsUsed * rate;
    }

    /// <summary>
    /// Calculates the cost for water or sanitation using tiered pricing.
    /// Tier 1: 0-6 kL
    /// Tier 2: 7-15 kL (next 9 kL)
    /// Tier 3: 16+ kL (remaining)
    /// </summary>
    /// <param name="unitsUsed">Units consumed in kL.</param>
    /// <param name="tariff">The tiered tariff.</param>
    /// <returns>The total cost before VAT.</returns>
    public static decimal CalculateTieredCost(decimal unitsUsed, Tariff tariff)
    {
        ArgumentNullException.ThrowIfNull(tariff);

        if (unitsUsed < 0)
            throw new ArgumentException("Units used cannot be negative.", nameof(unitsUsed));

        if (tariff.Steps.Count == 0)
            throw new ArgumentException("Tariff must have at least one step.", nameof(tariff));

        var remainingUnits = unitsUsed;
        var totalCost = 0m;
        var previousLimit = 0m;

        // Steps are already ordered by Tariff.Create
        foreach (var step in tariff.Steps)
        {
            if (remainingUnits <= 0)
                break;

            // Calculate how many units fall within this tier
            var tierCapacity = step.UpperLimit - previousLimit;
            var unitsInTier = Math.Min(remainingUnits, tierCapacity);

            totalCost += unitsInTier * step.Rate;
            remainingUnits -= unitsInTier;
            previousLimit = step.UpperLimit;
        }

        return totalCost;
    }

    /// <summary>
    /// Calculates VAT on a given amount.
    /// </summary>
    /// <param name="amount">The amount before VAT.</param>
    /// <returns>The VAT amount.</returns>
    public static decimal CalculateVat(decimal amount)
    {
        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative.", nameof(amount));

        return amount * VatRate;
    }

    /// <summary>
    /// Calculates the total including VAT.
    /// </summary>
    /// <param name="subtotal">The subtotal before VAT.</param>
    /// <returns>The total including VAT.</returns>
    public static decimal CalculateTotalWithVat(decimal subtotal)
    {
        return subtotal + CalculateVat(subtotal);
    }
}
