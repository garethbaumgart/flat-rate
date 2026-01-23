using FluentAssertions;
using FlatRate.Domain.Aggregates.Bills;
using FlatRate.Domain.ValueObjects;

namespace FlatRate.Domain.Tests.Aggregates.Bills;

public class BillingCalculatorTests
{
    #region CalculateElectricityCost Tests

    [Theory]
    [InlineData(0, 3.40, 0)]
    [InlineData(1, 3.40, 3.40)]
    [InlineData(100, 3.40, 340)]
    [InlineData(130, 3.40, 442)]
    [InlineData(50.5, 2.00, 101)]
    public void CalculateElectricityCost_WithValidInputs_ReturnsCorrectCost(
        double unitsUsed, decimal rate, decimal expectedCost)
    {
        // Arrange
        var tariff = Tariff.CreateFlatRate(rate);

        // Act
        var cost = BillingCalculator.CalculateElectricityCost((decimal)unitsUsed, tariff);

        // Assert
        cost.Should().Be(expectedCost);
    }

    [Fact]
    public void CalculateElectricityCost_WithNegativeUnits_ThrowsArgumentException()
    {
        // Arrange
        var tariff = Tariff.CreateFlatRate(3.40m);

        // Act
        var act = () => BillingCalculator.CalculateElectricityCost(-1, tariff);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Units used cannot be negative*");
    }

    [Fact]
    public void CalculateElectricityCost_WithNullTariff_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => BillingCalculator.CalculateElectricityCost(100, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void CalculateElectricityCost_WithValidTariff_DoesNotThrow()
    {
        // Arrange
        var tariff = Tariff.Create(new[] { TariffStep.Create(10, 5m) });

        // Act
        var act = () => BillingCalculator.CalculateElectricityCost(100, tariff);

        // Assert
        act.Should().NotThrow();
    }

    #endregion

    #region CalculateTieredCost Tests - Water/Sanitation

    [Fact]
    public void CalculateTieredCost_WithZeroUnits_ReturnsZero()
    {
        // Arrange
        var tariff = Tariff.CreateTiered(20.80m, 34.20m, 48.50m);

        // Act
        var cost = BillingCalculator.CalculateTieredCost(0, tariff);

        // Assert
        cost.Should().Be(0m);
    }

    [Theory]
    [InlineData(1, 20.80)]      // 1 unit at tier 1
    [InlineData(6, 124.80)]     // 6 units at tier 1: 6 × 20.80 = 124.80
    public void CalculateTieredCost_WithinTier1_ReturnsCorrectCost(
        int unitsUsed, decimal expectedCost)
    {
        // Arrange
        var tariff = Tariff.CreateTiered(20.80m, 34.20m, 48.50m);

        // Act
        var cost = BillingCalculator.CalculateTieredCost(unitsUsed, tariff);

        // Assert
        cost.Should().Be(expectedCost);
    }

    [Fact]
    public void CalculateTieredCost_SpanningTier1AndTier2_ReturnsCorrectCost()
    {
        // Arrange - 8 kL example from business logic doc
        // First 6 kL @ 20.80 = 124.80
        // Next 2 kL @ 34.20 = 68.40
        // Total = 193.20
        var tariff = Tariff.CreateTiered(20.80m, 34.20m, 48.50m);

        // Act
        var cost = BillingCalculator.CalculateTieredCost(8, tariff);

        // Assert
        cost.Should().Be(193.20m);
    }

    [Fact]
    public void CalculateTieredCost_ExactlyAtTier2Boundary_ReturnsCorrectCost()
    {
        // Arrange - 15 kL (end of tier 2)
        // First 6 kL @ 20.80 = 124.80
        // Next 9 kL @ 34.20 = 307.80
        // Total = 432.60
        var tariff = Tariff.CreateTiered(20.80m, 34.20m, 48.50m);

        // Act
        var cost = BillingCalculator.CalculateTieredCost(15, tariff);

        // Assert
        cost.Should().Be(432.60m);
    }

    [Fact]
    public void CalculateTieredCost_SpanningAllThreeTiers_ReturnsCorrectCost()
    {
        // Arrange - 20 kL
        // First 6 kL @ 20.80 = 124.80
        // Next 9 kL @ 34.20 = 307.80
        // Next 5 kL @ 48.50 = 242.50
        // Total = 675.10
        var tariff = Tariff.CreateTiered(20.80m, 34.20m, 48.50m);

        // Act
        var cost = BillingCalculator.CalculateTieredCost(20, tariff);

        // Assert
        cost.Should().Be(675.10m);
    }

    [Fact]
    public void CalculateTieredCost_LargeUsage_ReturnsCorrectCost()
    {
        // Arrange - 50 kL
        // First 6 kL @ 20.80 = 124.80
        // Next 9 kL @ 34.20 = 307.80
        // Next 35 kL @ 48.50 = 1697.50
        // Total = 2130.10
        var tariff = Tariff.CreateTiered(20.80m, 34.20m, 48.50m);

        // Act
        var cost = BillingCalculator.CalculateTieredCost(50, tariff);

        // Assert
        cost.Should().Be(2130.10m);
    }

    [Fact]
    public void CalculateTieredCost_WithSanitationRates_ReturnsCorrectCost()
    {
        // Arrange - 8 kL with sanitation rates from business logic doc
        // First 6 kL @ 25.50 = 153.00
        // Next 2 kL @ 20.50 = 41.00
        // Total = 194.00
        var tariff = Tariff.CreateTiered(25.50m, 20.50m, 29.80m);

        // Act
        var cost = BillingCalculator.CalculateTieredCost(8, tariff);

        // Assert
        cost.Should().Be(194.00m);
    }

    [Fact]
    public void CalculateTieredCost_WithNegativeUnits_ThrowsArgumentException()
    {
        // Arrange
        var tariff = Tariff.CreateTiered(20.80m, 34.20m, 48.50m);

        // Act
        var act = () => BillingCalculator.CalculateTieredCost(-1, tariff);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Units used cannot be negative*");
    }

    [Fact]
    public void CalculateTieredCost_WithNullTariff_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => BillingCalculator.CalculateTieredCost(8, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region VAT Calculation Tests

    [Theory]
    [InlineData(0, 0)]
    [InlineData(100, 15)]
    [InlineData(829.20, 124.38)]
    [InlineData(1000, 150)]
    public void CalculateVat_WithValidAmount_ReturnsCorrectVat(decimal amount, decimal expectedVat)
    {
        // Act
        var vat = BillingCalculator.CalculateVat(amount);

        // Assert
        vat.Should().Be(expectedVat);
    }

    [Fact]
    public void CalculateVat_WithNegativeAmount_ThrowsArgumentException()
    {
        // Act
        var act = () => BillingCalculator.CalculateVat(-1);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Amount cannot be negative*");
    }

    [Fact]
    public void VatRate_Is15Percent()
    {
        // Assert
        BillingCalculator.VatRate.Should().Be(0.15m);
    }

    #endregion

    #region CalculateTotalWithVat Tests

    [Theory]
    [InlineData(0, 0)]
    [InlineData(100, 115)]
    [InlineData(829.20, 953.58)]
    [InlineData(1000, 1150)]
    public void CalculateTotalWithVat_ReturnsCorrectTotal(decimal subtotal, decimal expectedTotal)
    {
        // Act
        var total = BillingCalculator.CalculateTotalWithVat(subtotal);

        // Assert
        total.Should().Be(expectedTotal);
    }

    #endregion

    #region Complete Bill Calculation (Integration)

    [Fact]
    public void CompleteCalculation_MatchesBusinessLogicExample()
    {
        // Arrange - Example from business logic document
        // Electricity: 130 kWh @ 3.40 = 442.00
        // Water: 8 kL tiered = 193.20
        // Sanitation: 8 kL tiered = 194.00
        // Subtotal = 829.20
        // VAT = 124.38
        // Total = 953.58

        var electricityTariff = Tariff.CreateFlatRate(3.40m);
        var waterTariff = Tariff.CreateTiered(20.80m, 34.20m, 48.50m);
        var sanitationTariff = Tariff.CreateTiered(25.50m, 20.50m, 29.80m);

        // Act
        var electricityCost = BillingCalculator.CalculateElectricityCost(130, electricityTariff);
        var waterCost = BillingCalculator.CalculateTieredCost(8, waterTariff);
        var sanitationCost = BillingCalculator.CalculateTieredCost(8, sanitationTariff);
        var subtotal = electricityCost + waterCost + sanitationCost;
        var vat = BillingCalculator.CalculateVat(subtotal);
        var total = subtotal + vat;

        // Assert
        electricityCost.Should().Be(442.00m);
        waterCost.Should().Be(193.20m);
        sanitationCost.Should().Be(194.00m);
        subtotal.Should().Be(829.20m);
        vat.Should().Be(124.38m);
        total.Should().Be(953.58m);
    }

    #endregion
}
