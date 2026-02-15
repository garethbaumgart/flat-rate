using FluentAssertions;
using FlatRate.Application.Bills;
using FlatRate.Application.Bills.Commands.CreateBill;

namespace FlatRate.Application.Tests.Bills.Commands.CreateBill;

public class CreateBillCommandDateTimeOffsetTests
{
    [Fact]
    public void CreateBillCommand_ShouldAcceptDateTimeOffsetValues()
    {
        // Arrange & Act
        var cmd = new CreateBillCommand(
            Guid.NewGuid(),
            new DateTimeOffset(2024, 4, 1, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2024, 4, 30, 0, 0, 0, TimeSpan.Zero),
            100, 150,
            10, 20,
            10, 18,
            3.40m,
            20.80m, 34.20m, 48.50m,
            25.50m, 20.50m, 29.80m);

        // Assert
        cmd.PeriodStart.Offset.Should().Be(TimeSpan.Zero);
        cmd.PeriodEnd.Offset.Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public void CreateBillCommand_WithPositiveOffset_ShouldPreserveOffset()
    {
        // Arrange - South Africa is UTC+2
        var saOffset = TimeSpan.FromHours(2);

        // Act
        var cmd = new CreateBillCommand(
            Guid.NewGuid(),
            new DateTimeOffset(2024, 4, 1, 0, 0, 0, saOffset),
            new DateTimeOffset(2024, 4, 30, 0, 0, 0, saOffset),
            100, 150,
            10, 20,
            10, 18,
            3.40m,
            20.80m, 34.20m, 48.50m,
            25.50m, 20.50m, 29.80m);

        // Assert
        cmd.PeriodStart.Offset.Should().Be(saOffset);
        cmd.PeriodEnd.Offset.Should().Be(saOffset);
    }

    [Fact]
    public void BillDto_ShouldSerializeDateTimeOffsetCorrectly()
    {
        // Arrange & Act
        var dto = new BillDto(
            Guid.NewGuid(),
            "UTIL-001",
            Guid.NewGuid(),
            new DateTimeOffset(2024, 4, 1, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2024, 4, 30, 0, 0, 0, TimeSpan.Zero),
            new MeterReadingDto(100, 150, 50),
            new MeterReadingDto(10, 20, 10),
            new MeterReadingDto(10, 18, 8),
            375m, 295m, 204m, 874m, 131.10m, 1005.10m,
            DateTimeOffset.UtcNow);

        // Assert
        dto.PeriodStart.ToString("O").Should().EndWith("+00:00");
        dto.CreatedAt.Offset.Should().Be(TimeSpan.Zero);
    }
}
