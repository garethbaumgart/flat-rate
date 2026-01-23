using FluentAssertions;
using FlatRate.Domain.Aggregates.Properties;

namespace FlatRate.Domain.Tests.Aggregates.Properties;

public class PropertyTests
{
    #region Create - Valid Scenarios

    [Fact]
    public void Create_WithValidInputs_ReturnsProperty()
    {
        // Arrange & Act
        var property = Property.Create("Flat 1", "123 Main Street");

        // Assert
        property.Name.Should().Be("Flat 1");
        property.Address.Should().Be("123 Main Street");
        property.Id.Should().NotBeEmpty();
        property.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        property.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public void Create_TrimsNameAndAddress()
    {
        // Arrange & Act
        var property = Property.Create("  Flat 1  ", "  123 Main Street  ");

        // Assert
        property.Name.Should().Be("Flat 1");
        property.Address.Should().Be("123 Main Street");
    }

    [Fact]
    public void Create_DefaultRatesAreNull()
    {
        // Arrange & Act
        var property = Property.Create("Flat 1", "123 Main Street");

        // Assert
        property.DefaultElectricityRate.Should().BeNull();
        property.DefaultWaterRateTier1.Should().BeNull();
        property.DefaultWaterRateTier2.Should().BeNull();
        property.DefaultWaterRateTier3.Should().BeNull();
        property.DefaultSanitationRateTier1.Should().BeNull();
        property.DefaultSanitationRateTier2.Should().BeNull();
        property.DefaultSanitationRateTier3.Should().BeNull();
    }

    #endregion

    #region Create - Invalid Scenarios

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidName_ThrowsArgumentException(string? name)
    {
        // Arrange & Act
        var act = () => Property.Create(name!, "123 Main Street");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Property name cannot be empty*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidAddress_ThrowsArgumentException(string? address)
    {
        // Arrange & Act
        var act = () => Property.Create("Flat 1", address!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Property address cannot be empty*");
    }

    #endregion

    #region Update Tests

    [Fact]
    public void Update_WithValidInputs_UpdatesProperty()
    {
        // Arrange
        var property = Property.Create("Flat 1", "123 Main Street");
        var originalCreatedAt = property.CreatedAt;

        // Act
        property.Update("Flat 2", "456 Other Street");

        // Assert
        property.Name.Should().Be("Flat 2");
        property.Address.Should().Be("456 Other Street");
        property.CreatedAt.Should().Be(originalCreatedAt);
        property.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Update_TrimsNameAndAddress()
    {
        // Arrange
        var property = Property.Create("Flat 1", "123 Main Street");

        // Act
        property.Update("  Flat 2  ", "  456 Other Street  ");

        // Assert
        property.Name.Should().Be("Flat 2");
        property.Address.Should().Be("456 Other Street");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_WithInvalidName_ThrowsArgumentException(string? name)
    {
        // Arrange
        var property = Property.Create("Flat 1", "123 Main Street");

        // Act
        var act = () => property.Update(name!, "456 Other Street");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Property name cannot be empty*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_WithInvalidAddress_ThrowsArgumentException(string? address)
    {
        // Arrange
        var property = Property.Create("Flat 1", "123 Main Street");

        // Act
        var act = () => property.Update("Flat 2", address!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Property address cannot be empty*");
    }

    #endregion

    #region SetDefaultElectricityRate Tests

    [Theory]
    [InlineData(0)]
    [InlineData(3.40)]
    [InlineData(100.50)]
    public void SetDefaultElectricityRate_WithValidRate_SetsRate(decimal rate)
    {
        // Arrange
        var property = Property.Create("Flat 1", "123 Main Street");

        // Act
        property.SetDefaultElectricityRate(rate);

        // Assert
        property.DefaultElectricityRate.Should().Be(rate);
        property.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void SetDefaultElectricityRate_WithNegativeRate_ThrowsArgumentException()
    {
        // Arrange
        var property = Property.Create("Flat 1", "123 Main Street");

        // Act
        var act = () => property.SetDefaultElectricityRate(-1);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Electricity rate cannot be negative*");
    }

    #endregion

    #region SetDefaultWaterRates Tests

    [Fact]
    public void SetDefaultWaterRates_WithValidRates_SetsRates()
    {
        // Arrange
        var property = Property.Create("Flat 1", "123 Main Street");

        // Act
        property.SetDefaultWaterRates(20.80m, 34.20m, 48.50m);

        // Assert
        property.DefaultWaterRateTier1.Should().Be(20.80m);
        property.DefaultWaterRateTier2.Should().Be(34.20m);
        property.DefaultWaterRateTier3.Should().Be(48.50m);
        property.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void SetDefaultWaterRates_WithNegativeTier1_ThrowsArgumentException()
    {
        // Arrange
        var property = Property.Create("Flat 1", "123 Main Street");

        // Act
        var act = () => property.SetDefaultWaterRates(-1, 34.20m, 48.50m);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Tier 1 rate cannot be negative*");
    }

    [Fact]
    public void SetDefaultWaterRates_WithNegativeTier2_ThrowsArgumentException()
    {
        // Arrange
        var property = Property.Create("Flat 1", "123 Main Street");

        // Act
        var act = () => property.SetDefaultWaterRates(20.80m, -1, 48.50m);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Tier 2 rate cannot be negative*");
    }

    [Fact]
    public void SetDefaultWaterRates_WithNegativeTier3_ThrowsArgumentException()
    {
        // Arrange
        var property = Property.Create("Flat 1", "123 Main Street");

        // Act
        var act = () => property.SetDefaultWaterRates(20.80m, 34.20m, -1);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Tier 3 rate cannot be negative*");
    }

    #endregion

    #region SetDefaultSanitationRates Tests

    [Fact]
    public void SetDefaultSanitationRates_WithValidRates_SetsRates()
    {
        // Arrange
        var property = Property.Create("Flat 1", "123 Main Street");

        // Act
        property.SetDefaultSanitationRates(25.50m, 20.50m, 29.80m);

        // Assert
        property.DefaultSanitationRateTier1.Should().Be(25.50m);
        property.DefaultSanitationRateTier2.Should().Be(20.50m);
        property.DefaultSanitationRateTier3.Should().Be(29.80m);
        property.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void SetDefaultSanitationRates_WithNegativeTier1_ThrowsArgumentException()
    {
        // Arrange
        var property = Property.Create("Flat 1", "123 Main Street");

        // Act
        var act = () => property.SetDefaultSanitationRates(-1, 20.50m, 29.80m);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Tier 1 rate cannot be negative*");
    }

    [Fact]
    public void SetDefaultSanitationRates_WithNegativeTier2_ThrowsArgumentException()
    {
        // Arrange
        var property = Property.Create("Flat 1", "123 Main Street");

        // Act
        var act = () => property.SetDefaultSanitationRates(25.50m, -1, 29.80m);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Tier 2 rate cannot be negative*");
    }

    [Fact]
    public void SetDefaultSanitationRates_WithNegativeTier3_ThrowsArgumentException()
    {
        // Arrange
        var property = Property.Create("Flat 1", "123 Main Street");

        // Act
        var act = () => property.SetDefaultSanitationRates(25.50m, 20.50m, -1);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Tier 3 rate cannot be negative*");
    }

    #endregion

    #region ClearDefaultRates Tests

    [Fact]
    public void ClearDefaultRates_ClearsAllRates()
    {
        // Arrange
        var property = Property.Create("Flat 1", "123 Main Street");
        property.SetDefaultElectricityRate(3.40m);
        property.SetDefaultWaterRates(20.80m, 34.20m, 48.50m);
        property.SetDefaultSanitationRates(25.50m, 20.50m, 29.80m);

        // Act
        property.ClearDefaultRates();

        // Assert
        property.DefaultElectricityRate.Should().BeNull();
        property.DefaultWaterRateTier1.Should().BeNull();
        property.DefaultWaterRateTier2.Should().BeNull();
        property.DefaultWaterRateTier3.Should().BeNull();
        property.DefaultSanitationRateTier1.Should().BeNull();
        property.DefaultSanitationRateTier2.Should().BeNull();
        property.DefaultSanitationRateTier3.Should().BeNull();
        property.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    #endregion
}
