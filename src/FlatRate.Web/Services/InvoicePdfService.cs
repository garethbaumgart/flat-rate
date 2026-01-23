using FlatRate.Application.Bills;
using FlatRate.Application.Properties.Queries.GetPropertyById;
using MediatR;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace FlatRate.Web.Services;

/// <summary>
/// Service for generating PDF invoices from bills.
/// </summary>
public sealed class InvoicePdfService
{
    private readonly IMediator _mediator;

    public InvoicePdfService(IMediator mediator)
    {
        _mediator = mediator;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<byte[]> GenerateInvoicePdfAsync(BillDto bill, CancellationToken cancellationToken)
    {
        // Get property details
        var property = await _mediator.Send(new GetPropertyByIdQuery(bill.PropertyId), cancellationToken);
        var propertyName = property?.Name ?? "Unknown Property";
        var propertyAddress = property?.Address ?? "";

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Element(c => ComposeHeader(c, bill, propertyName, propertyAddress));
                page.Content().Element(c => ComposeContent(c, bill));
                page.Footer().Element(ComposeFooter);
            });
        });

        return document.GeneratePdf();
    }

    private void ComposeHeader(IContainer container, BillDto bill, string propertyName, string propertyAddress)
    {
        container.Column(column =>
        {
            column.Spacing(10);

            column.Item().Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("INVOICE").FontSize(24).Bold().FontColor(Colors.Blue.Darken3);
                    col.Item().Text($"Invoice #: {bill.InvoiceNumber}").FontSize(12).Bold();
                });

                row.RelativeItem().AlignRight().Column(col =>
                {
                    col.Item().Text("FlatRate").FontSize(16).Bold().FontColor(Colors.Blue.Darken3);
                    col.Item().Text("Utility Billing Services").FontSize(10).FontColor(Colors.Grey.Darken1);
                });
            });

            column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

            column.Item().Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("BILLED TO:").FontSize(9).FontColor(Colors.Grey.Darken1);
                    col.Item().Text(propertyName).FontSize(12).Bold();
                    col.Item().Text(propertyAddress).FontSize(10);
                });

                row.RelativeItem().AlignRight().Column(col =>
                {
                    col.Item().Text($"Billing Period").FontSize(9).FontColor(Colors.Grey.Darken1);
                    col.Item().Text($"{bill.PeriodStart:yyyy-MM-dd} to {bill.PeriodEnd:yyyy-MM-dd}").FontSize(10).Bold();
                    col.Item().PaddingTop(10).Text($"Date Issued").FontSize(9).FontColor(Colors.Grey.Darken1);
                    col.Item().Text($"{bill.CreatedAt:yyyy-MM-dd}").FontSize(10);
                });
            });

            column.Item().PaddingTop(10);
        });
    }

    private void ComposeContent(IContainer container, BillDto bill)
    {
        container.Column(column =>
        {
            column.Spacing(15);

            // Meter Readings Section
            column.Item().Element(c => ComposeMeterReadings(c, bill));

            // Cost Breakdown Section
            column.Item().Element(c => ComposeCostBreakdown(c, bill));
        });
    }

    private void ComposeMeterReadings(IContainer container, BillDto bill)
    {
        container.Column(column =>
        {
            column.Item().Text("METER READINGS").FontSize(12).Bold().FontColor(Colors.Blue.Darken3);
            column.Item().PaddingTop(5);

            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(2);
                });

                // Header
                table.Header(header =>
                {
                    header.Cell().Background(Colors.Grey.Lighten3).Padding(8).Text("Utility").Bold();
                    header.Cell().Background(Colors.Grey.Lighten3).Padding(8).AlignRight().Text("Opening").Bold();
                    header.Cell().Background(Colors.Grey.Lighten3).Padding(8).AlignRight().Text("Closing").Bold();
                    header.Cell().Background(Colors.Grey.Lighten3).Padding(8).AlignRight().Text("Units Used").Bold();
                });

                // Electricity
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Text("Electricity (kWh)");
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(8).AlignRight().Text($"{bill.ElectricityReading.Opening:N2}");
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(8).AlignRight().Text($"{bill.ElectricityReading.Closing:N2}");
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(8).AlignRight().Text($"{bill.ElectricityReading.UnitsUsed:N2}").Bold();

                // Water
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Text("Water (kL)");
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(8).AlignRight().Text($"{bill.WaterReading.Opening:N2}");
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(8).AlignRight().Text($"{bill.WaterReading.Closing:N2}");
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(8).AlignRight().Text($"{bill.WaterReading.UnitsUsed:N2}").Bold();

                // Sanitation
                table.Cell().Padding(8).Text("Sanitation (kL)");
                table.Cell().Padding(8).AlignRight().Text($"{bill.SanitationReading.Opening:N2}");
                table.Cell().Padding(8).AlignRight().Text($"{bill.SanitationReading.Closing:N2}");
                table.Cell().Padding(8).AlignRight().Text($"{bill.SanitationReading.UnitsUsed:N2}").Bold();
            });
        });
    }

    private void ComposeCostBreakdown(IContainer container, BillDto bill)
    {
        container.Column(column =>
        {
            column.Item().Text("COST BREAKDOWN").FontSize(12).Bold().FontColor(Colors.Blue.Darken3);
            column.Item().PaddingTop(5);

            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(4);
                    columns.RelativeColumn(2);
                });

                // Header
                table.Header(header =>
                {
                    header.Cell().Background(Colors.Grey.Lighten3).Padding(8).Text("Description").Bold();
                    header.Cell().Background(Colors.Grey.Lighten3).Padding(8).AlignRight().Text("Amount (R)").Bold();
                });

                // Line items
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Text("Electricity");
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(8).AlignRight().Text($"{bill.ElectricityCost:N2}");

                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Text("Water");
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(8).AlignRight().Text($"{bill.WaterCost:N2}");

                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Text("Sanitation");
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(8).AlignRight().Text($"{bill.SanitationCost:N2}");

                // Subtotal
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Text("Subtotal").Bold();
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(8).AlignRight().Text($"{bill.Subtotal:N2}").Bold();

                // VAT
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Text("VAT (15%)");
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(8).AlignRight().Text($"{bill.VatAmount:N2}");

                // Total
                table.Cell().Background(Colors.Blue.Lighten5).Padding(8).Text("TOTAL").FontSize(12).Bold();
                table.Cell().Background(Colors.Blue.Lighten5).Padding(8).AlignRight().Text($"R {bill.Total:N2}").FontSize(12).Bold().FontColor(Colors.Blue.Darken3);
            });
        });
    }

    private void ComposeFooter(IContainer container)
    {
        container.Column(column =>
        {
            column.Item().AlignCenter().Text(text =>
            {
                text.Span("Generated by FlatRate - ").FontSize(8).FontColor(Colors.Grey.Darken1);
                text.Span(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm")).FontSize(8).FontColor(Colors.Grey.Darken1);
            });
        });
    }
}
