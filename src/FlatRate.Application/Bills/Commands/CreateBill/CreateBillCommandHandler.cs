using FlatRate.Domain.Aggregates.Bills;
using FlatRate.Domain.Repositories;
using FlatRate.Domain.ValueObjects;
using MediatR;

namespace FlatRate.Application.Bills.Commands.CreateBill;

/// <summary>
/// Handler for CreateBillCommand.
/// </summary>
public sealed class CreateBillCommandHandler : IRequestHandler<CreateBillCommand, Guid>
{
    private readonly IBillRepository _billRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateBillCommandHandler(IBillRepository billRepository, IUnitOfWork unitOfWork)
    {
        _billRepository = billRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateBillCommand request, CancellationToken cancellationToken)
    {
        var invoiceNumber = await _billRepository.GetNextInvoiceNumberAsync(cancellationToken);

        var electricityReading = MeterReading.Create(request.ElectricityReadingOpening, request.ElectricityReadingClosing);
        var waterReading = MeterReading.Create(request.WaterReadingOpening, request.WaterReadingClosing);
        var sanitationReading = MeterReading.Create(request.SanitationReadingOpening, request.SanitationReadingClosing);

        var electricityTariff = Tariff.CreateFlatRate(request.ElectricityRate);
        var waterTariff = Tariff.CreateTiered(request.WaterRateTier1, request.WaterRateTier2, request.WaterRateTier3);
        var sanitationTariff = Tariff.CreateTiered(request.SanitationRateTier1, request.SanitationRateTier2, request.SanitationRateTier3);

        var bill = Bill.Create(
            invoiceNumber,
            request.PropertyId,
            request.PeriodStart,
            request.PeriodEnd,
            electricityReading,
            waterReading,
            sanitationReading,
            electricityTariff,
            waterTariff,
            sanitationTariff);

        await _billRepository.AddAsync(bill, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return bill.Id;
    }
}
