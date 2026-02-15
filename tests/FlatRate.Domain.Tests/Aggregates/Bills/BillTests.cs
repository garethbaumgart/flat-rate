using FluentAssertions;
using FlatRate.Domain.Aggregates.Bills;
using FlatRate.Domain.ValueObjects;

namespace FlatRate.Domain.Tests.Aggregates.Bills;

public class BillTests
{
    private readonly Guid _propertyId = Guid.NewGuid();
    private readonly DateTimeOffset _periodStart = new(2024, 4, 1, 0, 0, 0, TimeSpan.Zero);
    private readonly DateTimeOffset _periodEnd = new(2024, 4, 30, 0, 0, 0, TimeSpan.Zero);

    private MeterReading CreateElectricityReading() => MeterReading.Create(12720, 12850);
    private MeterReading CreateWaterReading() => MeterReading.Create(222, 230);
    private MeterReading CreateSanitationReading() => MeterReading.Create(222, 230);
    private Tariff CreateElectricityTariff() => Tariff.CreateFlatRate(3.40m);
    private Tariff CreateWaterTariff() => Tariff.CreateTiered(20.80m, 34.20m, 48.50m);
    private Tariff CreateSanitationTariff() => Tariff.CreateTiered(25.50m, 20.50m, 29.80m);

    #region Create - Valid Scenarios

    [Fact]
    public void Create_WithValidInputs_ReturnsBill()
    {
        // Arrange & Act
        var bill = Bill.Create(
            "UTIL-0001",
            _propertyId,
            _periodStart,
            _periodEnd,
            CreateElectricityReading(),
            CreateWaterReading(),
            CreateSanitationReading(),
            CreateElectricityTariff(),
            CreateWaterTariff(),
            CreateSanitationTariff());

        // Assert
        bill.InvoiceNumber.Should().Be("UTIL-0001");
        bill.PropertyId.Should().Be(_propertyId);
        bill.PeriodStart.Should().Be(_periodStart);
        bill.PeriodEnd.Should().Be(_periodEnd);
        bill.Id.Should().NotBeEmpty();
        bill.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Create_WithValidInputs_CalculatesCostsCorrectly()
    {
        // Arrange & Act - Using example from business logic doc
        var bill = Bill.Create(
            "UTIL-0001",
            _propertyId,
            _periodStart,
            _periodEnd,
            CreateElectricityReading(),
            CreateWaterReading(),
            CreateSanitationReading(),
            CreateElectricityTariff(),
            CreateWaterTariff(),
            CreateSanitationTariff());

        // Assert
        bill.ElectricityCost.Should().Be(442.00m);
        bill.WaterCost.Should().Be(193.20m);
        bill.SanitationCost.Should().Be(194.00m);
        bill.Subtotal.Should().Be(829.20m);
        bill.VatAmount.Should().Be(124.38m);
        bill.Total.Should().Be(953.58m);
    }

    [Fact]
    public void Create_WithSamePeriodStartAndEnd_ReturnsBill()
    {
        // Arrange
        var sameDate = new DateTimeOffset(2024, 4, 1, 0, 0, 0, TimeSpan.Zero);

        // Act
        var bill = Bill.Create(
            "UTIL-0001",
            _propertyId,
            sameDate,
            sameDate,
            CreateElectricityReading(),
            CreateWaterReading(),
            CreateSanitationReading(),
            CreateElectricityTariff(),
            CreateWaterTariff(),
            CreateSanitationTariff());

        // Assert
        bill.PeriodStart.Should().Be(sameDate);
        bill.PeriodEnd.Should().Be(sameDate);
    }

    [Fact]
    public void Create_TrimsInvoiceNumber()
    {
        // Arrange & Act
        var bill = Bill.Create(
            "  UTIL-0001  ",
            _propertyId,
            _periodStart,
            _periodEnd,
            CreateElectricityReading(),
            CreateWaterReading(),
            CreateSanitationReading(),
            CreateElectricityTariff(),
            CreateWaterTariff(),
            CreateSanitationTariff());

        // Assert
        bill.InvoiceNumber.Should().Be("UTIL-0001");
    }

    #endregion

    #region Create - Invalid Scenarios

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidInvoiceNumber_ThrowsArgumentException(string? invoiceNumber)
    {
        // Arrange & Act
        var act = () => Bill.Create(
            invoiceNumber!,
            _propertyId,
            _periodStart,
            _periodEnd,
            CreateElectricityReading(),
            CreateWaterReading(),
            CreateSanitationReading(),
            CreateElectricityTariff(),
            CreateWaterTariff(),
            CreateSanitationTariff());

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Invoice number cannot be empty*");
    }

    [Fact]
    public void Create_WithEmptyPropertyId_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => Bill.Create(
            "UTIL-0001",
            Guid.Empty,
            _periodStart,
            _periodEnd,
            CreateElectricityReading(),
            CreateWaterReading(),
            CreateSanitationReading(),
            CreateElectricityTariff(),
            CreateWaterTariff(),
            CreateSanitationTariff());

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Property ID cannot be empty*");
    }

    [Fact]
    public void Create_WithPeriodEndBeforeStart_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => Bill.Create(
            "UTIL-0001",
            _propertyId,
            _periodEnd,  // Later date as start
            _periodStart, // Earlier date as end
            CreateElectricityReading(),
            CreateWaterReading(),
            CreateSanitationReading(),
            CreateElectricityTariff(),
            CreateWaterTariff(),
            CreateSanitationTariff());

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Period end date must be greater than or equal to period start date*");
    }

    [Fact]
    public void Create_WithNullElectricityReading_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => Bill.Create(
            "UTIL-0001",
            _propertyId,
            _periodStart,
            _periodEnd,
            null!,
            CreateWaterReading(),
            CreateSanitationReading(),
            CreateElectricityTariff(),
            CreateWaterTariff(),
            CreateSanitationTariff());

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Create_WithNullWaterReading_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => Bill.Create(
            "UTIL-0001",
            _propertyId,
            _periodStart,
            _periodEnd,
            CreateElectricityReading(),
            null!,
            CreateSanitationReading(),
            CreateElectricityTariff(),
            CreateWaterTariff(),
            CreateSanitationTariff());

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Create_WithNullSanitationReading_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => Bill.Create(
            "UTIL-0001",
            _propertyId,
            _periodStart,
            _periodEnd,
            CreateElectricityReading(),
            CreateWaterReading(),
            null!,
            CreateElectricityTariff(),
            CreateWaterTariff(),
            CreateSanitationTariff());

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Create_WithNullElectricityTariff_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => Bill.Create(
            "UTIL-0001",
            _propertyId,
            _periodStart,
            _periodEnd,
            CreateElectricityReading(),
            CreateWaterReading(),
            CreateSanitationReading(),
            null!,
            CreateWaterTariff(),
            CreateSanitationTariff());

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Create_WithNullWaterTariff_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => Bill.Create(
            "UTIL-0001",
            _propertyId,
            _periodStart,
            _periodEnd,
            CreateElectricityReading(),
            CreateWaterReading(),
            CreateSanitationReading(),
            CreateElectricityTariff(),
            null!,
            CreateSanitationTariff());

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Create_WithNullSanitationTariff_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => Bill.Create(
            "UTIL-0001",
            _propertyId,
            _periodStart,
            _periodEnd,
            CreateElectricityReading(),
            CreateWaterReading(),
            CreateSanitationReading(),
            CreateElectricityTariff(),
            CreateWaterTariff(),
            null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region Readings and Tariffs

    [Fact]
    public void Create_StoresMeterReadings()
    {
        // Arrange
        var electricityReading = CreateElectricityReading();
        var waterReading = CreateWaterReading();
        var sanitationReading = CreateSanitationReading();

        // Act
        var bill = Bill.Create(
            "UTIL-0001",
            _propertyId,
            _periodStart,
            _periodEnd,
            electricityReading,
            waterReading,
            sanitationReading,
            CreateElectricityTariff(),
            CreateWaterTariff(),
            CreateSanitationTariff());

        // Assert
        bill.ElectricityReading.Should().Be(electricityReading);
        bill.WaterReading.Should().Be(waterReading);
        bill.SanitationReading.Should().Be(sanitationReading);

        bill.ElectricityReading.UnitsUsed.Should().Be(130);
        bill.WaterReading.UnitsUsed.Should().Be(8);
        bill.SanitationReading.UnitsUsed.Should().Be(8);
    }

    [Fact]
    public void Create_StoresTariffs()
    {
        // Arrange
        var electricityTariff = CreateElectricityTariff();
        var waterTariff = CreateWaterTariff();
        var sanitationTariff = CreateSanitationTariff();

        // Act
        var bill = Bill.Create(
            "UTIL-0001",
            _propertyId,
            _periodStart,
            _periodEnd,
            CreateElectricityReading(),
            CreateWaterReading(),
            CreateSanitationReading(),
            electricityTariff,
            waterTariff,
            sanitationTariff);

        // Assert
        bill.ElectricityTariff.Should().Be(electricityTariff);
        bill.WaterTariff.Should().Be(waterTariff);
        bill.SanitationTariff.Should().Be(sanitationTariff);
    }

    #endregion

    #region Zero Usage Scenarios

    [Fact]
    public void Create_WithZeroUsage_ReturnsZeroTotals()
    {
        // Arrange
        var zeroReading = MeterReading.Create(100, 100);

        // Act
        var bill = Bill.Create(
            "UTIL-0001",
            _propertyId,
            _periodStart,
            _periodEnd,
            zeroReading,
            zeroReading,
            zeroReading,
            CreateElectricityTariff(),
            CreateWaterTariff(),
            CreateSanitationTariff());

        // Assert
        bill.ElectricityCost.Should().Be(0m);
        bill.WaterCost.Should().Be(0m);
        bill.SanitationCost.Should().Be(0m);
        bill.Subtotal.Should().Be(0m);
        bill.VatAmount.Should().Be(0m);
        bill.Total.Should().Be(0m);
    }

    #endregion

    #region Recalculate Tests

    [Fact]
    public void Recalculate_RecalculatesCostsFromCurrentReadingsAndTariffs()
    {
        // Arrange
        var bill = Bill.Create(
            "UTIL-0001",
            _propertyId,
            _periodStart,
            _periodEnd,
            CreateElectricityReading(),
            CreateWaterReading(),
            CreateSanitationReading(),
            CreateElectricityTariff(),
            CreateWaterTariff(),
            CreateSanitationTariff());

        var originalTotal = bill.Total;

        // Act
        bill.Recalculate();

        // Assert - costs should remain the same when recalculated with same data
        bill.ElectricityCost.Should().Be(442.00m);
        bill.WaterCost.Should().Be(193.20m);
        bill.SanitationCost.Should().Be(194.00m);
        bill.Total.Should().Be(originalTotal);
    }

    #endregion

    #region DateTimeOffset Tests

    [Fact]
    public void Create_ShouldStoreCreatedAtAsUtc()
    {
        // Arrange & Act
        var bill = Bill.Create(
            "UTIL-0001",
            _propertyId,
            _periodStart,
            _periodEnd,
            CreateElectricityReading(),
            CreateWaterReading(),
            CreateSanitationReading(),
            CreateElectricityTariff(),
            CreateWaterTariff(),
            CreateSanitationTariff());

        // Assert
        bill.CreatedAt.Offset.Should().Be(TimeSpan.Zero, "CreatedAt should be stored as UTC");
    }

    [Fact]
    public void Create_WithUtcOffsetDates_ShouldPreserveValues()
    {
        // Arrange
        var start = new DateTimeOffset(2024, 4, 1, 0, 0, 0, TimeSpan.Zero);
        var end = new DateTimeOffset(2024, 4, 30, 0, 0, 0, TimeSpan.Zero);

        // Act
        var bill = Bill.Create(
            "UTIL-0001",
            _propertyId,
            start,
            end,
            CreateElectricityReading(),
            CreateWaterReading(),
            CreateSanitationReading(),
            CreateElectricityTariff(),
            CreateWaterTariff(),
            CreateSanitationTariff());

        // Assert
        bill.PeriodStart.Should().Be(start);
        bill.PeriodEnd.Should().Be(end);
    }

    [Fact]
    public void Create_WithPositiveOffsetDates_ShouldNormalizeToUtc()
    {
        // Arrange - South Africa is UTC+2
        var saOffset = TimeSpan.FromHours(2);
        var start = new DateTimeOffset(2024, 4, 1, 0, 0, 0, saOffset);
        var end = new DateTimeOffset(2024, 4, 30, 0, 0, 0, saOffset);

        // Act
        var bill = Bill.Create(
            "UTIL-0001",
            _propertyId,
            start,
            end,
            CreateElectricityReading(),
            CreateWaterReading(),
            CreateSanitationReading(),
            CreateElectricityTariff(),
            CreateWaterTariff(),
            CreateSanitationTariff());

        // Assert - Values should be converted to UTC (offset = 0)
        bill.PeriodStart.Offset.Should().Be(TimeSpan.Zero, "PeriodStart should be normalized to UTC");
        bill.PeriodEnd.Offset.Should().Be(TimeSpan.Zero, "PeriodEnd should be normalized to UTC");
        // UTC+2 midnight = UTC 22:00 previous day
        bill.PeriodStart.Should().Be(new DateTimeOffset(2024, 3, 31, 22, 0, 0, TimeSpan.Zero));
        bill.PeriodEnd.Should().Be(new DateTimeOffset(2024, 4, 29, 22, 0, 0, TimeSpan.Zero));
    }

    [Fact]
    public void Create_WithUtcMidnightDates_PreservesCalendarDate()
    {
        // Arrange - When the frontend sends "2025-01-01" as a date-only string,
        // the API parses it as UTC midnight. This test verifies the calendar date
        // is preserved through Bill.Create (i.e., Jan 1 stays Jan 1).
        var start = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var end = new DateTimeOffset(2025, 1, 31, 0, 0, 0, TimeSpan.Zero);

        // Act
        var bill = Bill.Create(
            "UTIL-0001",
            _propertyId,
            start,
            end,
            CreateElectricityReading(),
            CreateWaterReading(),
            CreateSanitationReading(),
            CreateElectricityTariff(),
            CreateWaterTariff(),
            CreateSanitationTariff());

        // Assert - Calendar dates should be preserved as-is
        bill.PeriodStart.Year.Should().Be(2025);
        bill.PeriodStart.Month.Should().Be(1);
        bill.PeriodStart.Day.Should().Be(1);
        bill.PeriodEnd.Year.Should().Be(2025);
        bill.PeriodEnd.Month.Should().Be(1);
        bill.PeriodEnd.Day.Should().Be(31);
    }

    [Fact]
    public void Create_WithDifferentOffsets_ShouldAllNormalizeToSameUtcInstant()
    {
        // Arrange - Same instant expressed in different timezones
        var utcStart = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var saStart = new DateTimeOffset(2025, 1, 1, 2, 0, 0, TimeSpan.FromHours(2));

        // Act
        var billUtc = Bill.Create(
            "UTIL-UTC",
            _propertyId,
            utcStart,
            utcStart,
            CreateElectricityReading(),
            CreateWaterReading(),
            CreateSanitationReading(),
            CreateElectricityTariff(),
            CreateWaterTariff(),
            CreateSanitationTariff());

        var billSa = Bill.Create(
            "UTIL-SA",
            _propertyId,
            saStart,
            saStart,
            CreateElectricityReading(),
            CreateWaterReading(),
            CreateSanitationReading(),
            CreateElectricityTariff(),
            CreateWaterTariff(),
            CreateSanitationTariff());

        // Assert - Both should resolve to the same UTC instant
        billUtc.PeriodStart.UtcDateTime.Should().Be(billSa.PeriodStart.UtcDateTime);
    }

    #endregion
}
