using FluentAssertions;
using FlatRate.Domain.ValueObjects;

namespace FlatRate.Domain.Tests.ValueObjects;

public class TariffTests
{
    #region TariffStep Tests

    [Fact]
    public void TariffStep_Create_WithValidValues_ReturnsTariffStep()
    {
        // Arrange & Act
        var step = TariffStep.Create(6, 20.80m);

        // Assert
        step.UpperLimit.Should().Be(6);
        step.Rate.Should().Be(20.80m);
    }

    [Fact]
    public void TariffStep_Create_WithZeroRate_ReturnsTariffStep()
    {
        // Arrange & Act
        var step = TariffStep.Create(6, 0m);

        // Assert
        step.Rate.Should().Be(0m);
    }

    [Fact]
    public void TariffStep_Create_WithZeroUpperLimit_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => TariffStep.Create(0, 20.80m);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Upper limit must be greater than zero*");
    }

    [Fact]
    public void TariffStep_Create_WithNegativeUpperLimit_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => TariffStep.Create(-1, 20.80m);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Upper limit must be greater than zero*");
    }

    [Fact]
    public void TariffStep_Create_WithNegativeRate_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => TariffStep.Create(6, -1m);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Rate cannot be negative*");
    }

    #endregion

    #region CreateFlatRate Tests

    [Fact]
    public void CreateFlatRate_WithValidRate_ReturnsTariffWithSingleStep()
    {
        // Arrange & Act
        var tariff = Tariff.CreateFlatRate(3.40m);

        // Assert
        tariff.Steps.Should().HaveCount(1);
        tariff.Steps[0].Rate.Should().Be(3.40m);
        tariff.Steps[0].UpperLimit.Should().Be(double.MaxValue);
    }

    [Fact]
    public void CreateFlatRate_WithZeroRate_ReturnsTariff()
    {
        // Arrange & Act
        var tariff = Tariff.CreateFlatRate(0m);

        // Assert
        tariff.Steps.Should().HaveCount(1);
        tariff.Steps[0].Rate.Should().Be(0m);
    }

    [Fact]
    public void CreateFlatRate_WithNegativeRate_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => Tariff.CreateFlatRate(-1m);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Rate cannot be negative*");
    }

    #endregion

    #region CreateTiered Tests

    [Fact]
    public void CreateTiered_WithValidRates_ReturnsTariffWithThreeSteps()
    {
        // Arrange & Act
        var tariff = Tariff.CreateTiered(20.80m, 34.20m, 48.50m);

        // Assert
        tariff.Steps.Should().HaveCount(3);
        tariff.Steps[0].UpperLimit.Should().Be(6);
        tariff.Steps[0].Rate.Should().Be(20.80m);
        tariff.Steps[1].UpperLimit.Should().Be(15);
        tariff.Steps[1].Rate.Should().Be(34.20m);
        tariff.Steps[2].UpperLimit.Should().Be(double.MaxValue);
        tariff.Steps[2].Rate.Should().Be(48.50m);
    }

    [Fact]
    public void CreateTiered_WithNegativeTier1Rate_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => Tariff.CreateTiered(-1m, 34.20m, 48.50m);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Tier 1 rate cannot be negative*");
    }

    [Fact]
    public void CreateTiered_WithNegativeTier2Rate_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => Tariff.CreateTiered(20.80m, -1m, 48.50m);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Tier 2 rate cannot be negative*");
    }

    [Fact]
    public void CreateTiered_WithNegativeTier3Rate_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => Tariff.CreateTiered(20.80m, 34.20m, -1m);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Tier 3 rate cannot be negative*");
    }

    #endregion

    #region Create (Custom Steps) Tests

    [Fact]
    public void Create_WithValidSteps_ReturnsTariff()
    {
        // Arrange
        var steps = new[]
        {
            TariffStep.Create(10, 5m),
            TariffStep.Create(20, 10m)
        };

        // Act
        var tariff = Tariff.Create(steps);

        // Assert
        tariff.Steps.Should().HaveCount(2);
    }

    [Fact]
    public void Create_WithUnorderedSteps_ReturnsOrderedTariff()
    {
        // Arrange
        var steps = new[]
        {
            TariffStep.Create(20, 10m),
            TariffStep.Create(10, 5m)
        };

        // Act
        var tariff = Tariff.Create(steps);

        // Assert
        tariff.Steps[0].UpperLimit.Should().Be(10);
        tariff.Steps[1].UpperLimit.Should().Be(20);
    }

    [Fact]
    public void Create_WithEmptySteps_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => Tariff.Create(Array.Empty<TariffStep>());

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Tariff must have at least one step*");
    }

    #endregion
}
