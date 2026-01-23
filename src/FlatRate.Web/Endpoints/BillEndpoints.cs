using FlatRate.Application.Bills;
using FlatRate.Application.Bills.Commands.CreateBill;
using FlatRate.Application.Bills.Commands.DeleteBill;
using FlatRate.Application.Bills.Queries.GetAllBills;
using FlatRate.Application.Bills.Queries.GetBillById;
using FlatRate.Web.Services;
using MediatR;

namespace FlatRate.Web.Endpoints;

public static class BillEndpoints
{
    public static void MapBillEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/bills")
            .RequireAuthorization();

        group.MapGet("/", GetAll);
        group.MapGet("/{id:guid}", GetById);
        group.MapGet("/{id:guid}/pdf", DownloadPdf);
        group.MapPost("/", Create);
        group.MapDelete("/{id:guid}", Delete);
    }

    private static async Task<IResult> GetAll(Guid? propertyId, IMediator mediator, CancellationToken cancellationToken)
    {
        var bills = await mediator.Send(new GetAllBillsQuery(propertyId), cancellationToken);
        return Results.Ok(bills);
    }

    private static async Task<IResult> GetById(Guid id, IMediator mediator, CancellationToken cancellationToken)
    {
        var bill = await mediator.Send(new GetBillByIdQuery(id), cancellationToken);

        return bill is null
            ? Results.NotFound(new { error = "Bill not found" })
            : Results.Ok(bill);
    }

    private static async Task<IResult> DownloadPdf(
        Guid id,
        IMediator mediator,
        InvoicePdfService pdfService,
        CancellationToken cancellationToken)
    {
        var bill = await mediator.Send(new GetBillByIdQuery(id), cancellationToken);

        if (bill is null)
        {
            return Results.NotFound(new { error = "Bill not found" });
        }

        var pdfBytes = await pdfService.GenerateInvoicePdfAsync(bill, cancellationToken);
        return Results.File(pdfBytes, "application/pdf", $"Invoice-{bill.InvoiceNumber}.pdf");
    }

    private static async Task<IResult> Create(CreateBillRequest request, IMediator mediator, CancellationToken cancellationToken)
    {
        // Validate required fields
        if (request.PropertyId == Guid.Empty)
        {
            return Results.BadRequest(new { error = "PropertyId is required" });
        }

        if (request.PeriodStart > request.PeriodEnd)
        {
            return Results.BadRequest(new { error = "Period end must be greater than or equal to period start" });
        }

        if (request.ElectricityReadingClosing < request.ElectricityReadingOpening)
        {
            return Results.BadRequest(new { error = "Electricity closing reading must be greater than or equal to opening reading" });
        }

        if (request.WaterReadingClosing < request.WaterReadingOpening)
        {
            return Results.BadRequest(new { error = "Water closing reading must be greater than or equal to opening reading" });
        }

        if (request.SanitationReadingClosing < request.SanitationReadingOpening)
        {
            return Results.BadRequest(new { error = "Sanitation closing reading must be greater than or equal to opening reading" });
        }

        try
        {
            var command = new CreateBillCommand(
                request.PropertyId,
                request.PeriodStart,
                request.PeriodEnd,
                request.ElectricityReadingOpening,
                request.ElectricityReadingClosing,
                request.WaterReadingOpening,
                request.WaterReadingClosing,
                request.SanitationReadingOpening,
                request.SanitationReadingClosing,
                request.ElectricityRate,
                request.WaterRateTier1,
                request.WaterRateTier2,
                request.WaterRateTier3,
                request.SanitationRateTier1,
                request.SanitationRateTier2,
                request.SanitationRateTier3);

            var id = await mediator.Send(command, cancellationToken);
            return Results.Created($"/api/bills/{id}", new { id });
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> Delete(Guid id, IMediator mediator, CancellationToken cancellationToken)
    {
        var success = await mediator.Send(new DeleteBillCommand(id), cancellationToken);

        return success
            ? Results.NoContent()
            : Results.NotFound(new { error = "Bill not found" });
    }
}

public record CreateBillRequest(
    Guid PropertyId,
    DateTime PeriodStart,
    DateTime PeriodEnd,
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
    decimal SanitationRateTier3);
