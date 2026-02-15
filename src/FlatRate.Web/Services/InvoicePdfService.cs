using System.Globalization;
using FlatRate.Application.Bills;
using FlatRate.Application.Properties.Queries.GetPropertyById;
using MediatR;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace FlatRate.Web.Services;

/// <summary>
/// Service for generating Nord-themed PDF invoices from bills.
/// Uses the "Split Header + Grid Statement" (Variant D) layout.
/// </summary>
public sealed class InvoicePdfService
{
    // Nord palette
    private static readonly Color Nord0 = Color.FromHex("#2e3440");
    private static readonly Color Nord1 = Color.FromHex("#3b4252");
    private static readonly Color Nord2 = Color.FromHex("#434c5e");
    private static readonly Color Nord3 = Color.FromHex("#4c566a");
    private static readonly Color Nord4 = Color.FromHex("#d8dee9");
    private static readonly Color Nord5 = Color.FromHex("#e5e9f0");
    private static readonly Color Nord6 = Color.FromHex("#eceff4");
    private static readonly Color Nord13 = Color.FromHex("#ebcb8b");

    private readonly IMediator _mediator;

    static InvoicePdfService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public InvoicePdfService(IMediator mediator)
    {
        _mediator = mediator;
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
                page.Margin(0);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Element(c => ComposeHeader(c, bill));
                page.Content().Element(c => ComposeContent(c, bill, propertyName, propertyAddress));
                page.Footer().Element(ComposeFooter);
            });
        });

        return document.GeneratePdf();
    }

    private void ComposeHeader(IContainer container, BillDto bill)
    {
        container.Background(Nord0).Padding(20).PaddingHorizontal(40).Row(row =>
        {
            // Left side: logo mark + brand text
            row.RelativeItem().Row(innerRow =>
            {
                // Aurora Yellow logo square with Voltage Arrow SVG
                const string logoSvg = """
                    <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 80 80">
                      <rect width="80" height="80" rx="16" fill="#ebcb8b"/>
                      <path d="M12,40 L22,40 L26,32 L30,48 L34,32 L38,48 L42,40 L52,40"
                        fill="none" stroke="#2e3440" stroke-width="4"
                        stroke-linecap="round" stroke-linejoin="round"/>
                      <polygon points="52,30 68,40 52,50" fill="#2e3440"/>
                    </svg>
                    """;
                innerRow.ConstantItem(34).Height(34).Svg(logoSvg);

                innerRow.ConstantItem(12); // spacer

                innerRow.RelativeItem().Column(col =>
                {
                    col.Item().Text("FlatRate").FontSize(16).Bold().FontColor(Nord6);
                    col.Item().Text("UTILITY BILLING SERVICES").FontSize(6.5f).FontColor(Nord3).LetterSpacing(0.1f);
                });
            });

            // Right side: INVOICE tag + invoice number/date
            row.ConstantItem(180).AlignRight().Column(col =>
            {
                col.Item().AlignRight().Container()
                    .Background(Nord1).CornerRadius(3).PaddingVertical(4).PaddingHorizontal(12)
                    .Text("INVOICE").FontSize(8).Bold().FontColor(Nord13).LetterSpacing(0.12f);

                col.Item().PaddingTop(4).AlignRight()
                    .Text(string.Format(CultureInfo.InvariantCulture, "#{0}  \u2022  {1:dd MMM yyyy}", bill.InvoiceNumber, bill.CreatedAt))
                    .FontSize(8).FontColor(Nord3);
            });
        });
    }

    private void ComposeContent(IContainer container, BillDto bill, string propertyName, string propertyAddress)
    {
        container.PaddingHorizontal(40).PaddingTop(20).Column(column =>
        {
            column.Spacing(16);

            // Info grid
            column.Item().Element(c => ComposeInfoGrid(c, bill, propertyName, propertyAddress));

            // Meter Readings section
            column.Item().Element(c => ComposeSectionTitle(c, "Meter Readings"));
            column.Item().Element(c => ComposeMeterReadings(c, bill));

            // Cost Breakdown section
            column.Item().Element(c => ComposeSectionTitle(c, "Cost Breakdown"));
            column.Item().Element(c => ComposeCostBreakdown(c, bill));

            // Summary cards
            column.Item().Element(c => ComposeSummaryCards(c, bill));
        });
    }

    private static void ComposeInfoGrid(IContainer container, BillDto bill, string propertyName, string propertyAddress)
    {
        container.Border(1).BorderColor(Nord5).CornerRadius(6).Padding(14).Row(row =>
        {
            InfoGridCell(row, "BILLED TO", propertyName);
            InfoGridCell(row, "LOCATION", propertyAddress);
            InfoGridCell(row, "PERIOD", $"{bill.PeriodStart:MMM d} \u2014 {bill.PeriodEnd:MMM d, yyyy}");
            InfoGridCell(row, "ISSUED", $"{bill.CreatedAt:MMMM d, yyyy}");
        });
    }

    private static void InfoGridCell(RowDescriptor row, string label, string value)
    {
        row.RelativeItem().Column(col =>
        {
            col.Item().Text(label).FontSize(7).FontColor(Nord3).LetterSpacing(0.08f);
            col.Item().PaddingTop(3).Text(value).FontSize(9).Bold().FontColor(Nord0);
        });
    }

    private static void ComposeSectionTitle(IContainer container, string title)
    {
        container.Text(title).FontSize(8).Bold().FontColor(Nord3).LetterSpacing(0.1f);
    }

    private void ComposeMeterReadings(IContainer container, BillDto bill)
    {
        container.Table(table =>
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
                HeaderCell(header.Cell(), "Utility", false);
                HeaderCell(header.Cell(), "Opening", true);
                HeaderCell(header.Cell(), "Closing", true);
                HeaderCell(header.Cell(), "Units", true);
            });

            // Electricity
            BodyCell(table, "Electricity (kWh)", false, false);
            BodyCell(table, $"{bill.ElectricityReading.Opening:N2}", true, false);
            BodyCell(table, $"{bill.ElectricityReading.Closing:N2}", true, false);
            BodyCellBold(table, $"{bill.ElectricityReading.UnitsUsed:N2}", false);

            // Water
            BodyCell(table, "Water (kL)", false, false);
            BodyCell(table, $"{bill.WaterReading.Opening:N2}", true, false);
            BodyCell(table, $"{bill.WaterReading.Closing:N2}", true, false);
            BodyCellBold(table, $"{bill.WaterReading.UnitsUsed:N2}", false);

            // Sanitation (last row)
            BodyCell(table, "Sanitation (kL)", false, true);
            BodyCell(table, $"{bill.SanitationReading.Opening:N2}", true, true);
            BodyCell(table, $"{bill.SanitationReading.Closing:N2}", true, true);
            BodyCellBold(table, $"{bill.SanitationReading.UnitsUsed:N2}", true);
        });
    }

    private void ComposeCostBreakdown(IContainer container, BillDto bill)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(4);
                columns.RelativeColumn(2);
            });

            // Header
            table.Header(header =>
            {
                HeaderCell(header.Cell(), "Description", false);
                HeaderCell(header.Cell(), "Amount (R)", true);
            });

            // Electricity
            BodyCell(table, "Electricity", false, false);
            BodyCell(table, $"{bill.ElectricityCost:N2}", true, false);

            // Water
            BodyCell(table, "Water", false, false);
            BodyCell(table, $"{bill.WaterCost:N2}", true, false);

            // Sanitation (last row)
            BodyCell(table, "Sanitation", false, true);
            BodyCell(table, $"{bill.SanitationCost:N2}", true, true);
        });
    }

    private static void ComposeSummaryCards(IContainer container, BillDto bill)
    {
        container.Row(row =>
        {
            SummaryCard(row.RelativeItem(), "SUBTOTAL", $"R {bill.Subtotal:N2}", false);
            row.ConstantItem(10);
            SummaryCard(row.RelativeItem(), "VAT (15%)", $"R {bill.VatAmount:N2}", false);
            row.ConstantItem(10);
            SummaryCard(row.RelativeItem(), "ITEMS", "3", false);
            row.ConstantItem(10);
            SummaryCard(row.RelativeItem(), "TOTAL DUE", $"R {bill.Total:N2}", true);
        });
    }

    private static void SummaryCard(IContainer container, string label, string value, bool isDark)
    {
        var bgColor = isDark ? Nord0 : Nord6;
        var valueColor = isDark ? Nord13 : Nord0;
        var labelColor = isDark ? Nord4 : Nord3;

        container.Background(bgColor).CornerRadius(6).Padding(12).Column(col =>
        {
            col.Item().AlignCenter().Text(label).FontSize(7).FontColor(labelColor).LetterSpacing(0.08f);
            col.Item().PaddingTop(4).AlignCenter().Text(value).FontSize(13).Bold().FontColor(valueColor);
        });
    }

    private void ComposeFooter(IContainer container)
    {
        container.PaddingHorizontal(40).AlignCenter().Text(text =>
        {
            text.Span("Generated by FlatRate").FontSize(8).FontColor(Nord3);
            text.Span("  \u2022  ").FontSize(8).FontColor(Nord4);
            text.Span(DateTime.UtcNow.ToString("dd MMM yyyy, HH:mm", CultureInfo.InvariantCulture)).FontSize(8).FontColor(Nord3);
        });
    }

    private static void HeaderCell(IContainer container, string text, bool alignRight)
    {
        IContainer cell = container.Background(Nord6).Padding(8);
        if (alignRight)
            cell = cell.AlignRight();
        cell.Text(text).FontSize(8).Bold().FontColor(Nord2);
    }

    private static void BodyCell(TableDescriptor table, string text, bool alignRight, bool lastRow)
    {
        table.Cell().Column(col =>
        {
            IContainer cell = col.Item().Padding(8);
            if (alignRight)
                cell = cell.AlignRight();
            cell.Text(text).FontSize(9).FontColor(Nord0);

            if (!lastRow)
                col.Item().LineHorizontal(0.5f).LineColor(Nord5).LineDashPattern([4f, 3f]);
        });
    }

    private static void BodyCellBold(TableDescriptor table, string text, bool lastRow)
    {
        table.Cell().Column(col =>
        {
            col.Item().Padding(8).AlignRight().Text(text).FontSize(9).Bold().FontColor(Nord0);

            if (!lastRow)
                col.Item().LineHorizontal(0.5f).LineColor(Nord5).LineDashPattern([4f, 3f]);
        });
    }
}
