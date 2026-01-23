using FluentAssertions;
using FlatRate.Domain.ValueObjects;

namespace FlatRate.Domain.Tests.ValueObjects;

public class MeterReadingTests
{
    #region Create - Valid Scenarios

    [Fact]
    public void Create_WithValidReadings_ReturnsMeterReading()
    {
        // Arrange
        var opening = 100m;
        var closing = 150m;

        // Act
        var reading = MeterReading.Create(opening, closing);

        // Assert
        reading.Opening.Should().Be(opening);
        reading.Closing.Should().Be(closing);
    }

    [Fact]
    public void Create_WithZeroReadings_ReturnsMeterReading()
    {
        // Arrange & Act
        var reading = MeterReading.Create(0, 0);

        // Assert
        reading.Opening.Should().Be(0);
        reading.Closing.Should().Be(0);
    }

    [Fact]
    public void Create_WithEqualReadings_ReturnsMeterReading()
    {
        // Arrange & Act
        var reading = MeterReading.Create(100, 100);

        // Assert
        reading.Opening.Should().Be(100);
        reading.Closing.Should().Be(100);
    }

    #endregion

    #region Create - Invalid Scenarios

    [Fact]
    public void Create_WithNegativeOpening_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => MeterReading.Create(-1, 100);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Opening reading cannot be negative*");
    }

    [Fact]
    public void Create_WithNegativeClosing_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => MeterReading.Create(0, -1);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Closing reading cannot be negative*");
    }

    [Fact]
    public void Create_WithClosingLessThanOpening_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => MeterReading.Create(100, 50);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Closing reading must be greater than or equal to opening reading*");
    }

    #endregion

    #region UnitsUsed Calculation

    [Theory]
    [InlineData(100, 150, 50)]
    [InlineData(0, 100, 100)]
    [InlineData(12720, 12850, 130)]
    [InlineData(222, 230, 8)]
    [InlineData(0, 0, 0)]
    [InlineData(100, 100, 0)]
    public void UnitsUsed_ReturnsCorrectValue(int opening, int closing, int expected)
    {
        // Arrange
        var reading = MeterReading.Create(opening, closing);

        // Act & Assert
        reading.UnitsUsed.Should().Be(expected);
    }

    #endregion

    #region Equality

    [Fact]
    public void Equals_WithSameValues_ReturnsTrue()
    {
        // Arrange
        var reading1 = MeterReading.Create(100, 150);
        var reading2 = MeterReading.Create(100, 150);

        // Act & Assert
        reading1.Should().Be(reading2);
    }

    [Fact]
    public void Equals_WithDifferentValues_ReturnsFalse()
    {
        // Arrange
        var reading1 = MeterReading.Create(100, 150);
        var reading2 = MeterReading.Create(100, 160);

        // Act & Assert
        reading1.Should().NotBe(reading2);
    }

    #endregion
}
