namespace FlatRate.Domain.ValueObjects;

/// <summary>
/// Represents a meter reading with opening and closing values.
/// Uses decimal for precision in financial calculations.
/// </summary>
public sealed record MeterReading
{
    public decimal Opening { get; }
    public decimal Closing { get; }
    public decimal UnitsUsed => Closing - Opening;

    private MeterReading(decimal opening, decimal closing)
    {
        Opening = opening;
        Closing = closing;
    }

    public static MeterReading Create(decimal opening, decimal closing)
    {
        if (opening < 0)
            throw new ArgumentException("Opening reading cannot be negative.", nameof(opening));

        if (closing < 0)
            throw new ArgumentException("Closing reading cannot be negative.", nameof(closing));

        if (closing < opening)
            throw new ArgumentException("Closing reading must be greater than or equal to opening reading.", nameof(closing));

        return new MeterReading(opening, closing);
    }
}
