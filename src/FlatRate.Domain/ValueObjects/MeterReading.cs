namespace FlatRate.Domain.ValueObjects;

/// <summary>
/// Represents a meter reading with opening and closing values.
/// </summary>
public sealed record MeterReading
{
    public double Opening { get; }
    public double Closing { get; }
    public double UnitsUsed => Closing - Opening;

    private MeterReading(double opening, double closing)
    {
        Opening = opening;
        Closing = closing;
    }

    public static MeterReading Create(double opening, double closing)
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
