using MediatR;

namespace FlatRate.Application.Bills.Commands.CreateBill;

/// <summary>
/// Command to create a new bill.
/// </summary>
public sealed record CreateBillCommand(
    Guid PropertyId,
    DateTimeOffset PeriodStart,
    DateTimeOffset PeriodEnd,
    decimal ElectricityReadingOpening,
    decimal ElectricityReadingClosing,
    decimal WaterReadingOpening,
    decimal WaterReadingClosing,
    decimal SanitationReadingOpening,
    decimal SanitationReadingClosing,
    decimal ElectricityRate,
    decimal WaterRateTier1,
    decimal WaterRateTier2,
    decimal WaterRateTier3,
    decimal SanitationRateTier1,
    decimal SanitationRateTier2,
    decimal SanitationRateTier3) : IRequest<Guid>;
