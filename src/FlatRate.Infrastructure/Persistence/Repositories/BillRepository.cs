using System.Text.RegularExpressions;
using FlatRate.Domain.Aggregates.Bills;
using Microsoft.EntityFrameworkCore;

namespace FlatRate.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of IBillRepository.
/// </summary>
public sealed partial class BillRepository : IBillRepository
{
    private readonly FlatRateDbContext _context;

    public BillRepository(FlatRateDbContext context)
    {
        _context = context;
    }

    public async Task<Bill?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Bills
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Bill>> GetByPropertyIdAsync(Guid propertyId, CancellationToken cancellationToken = default)
    {
        return await _context.Bills
            .AsNoTracking()
            .Where(b => b.PropertyId == propertyId)
            .OrderByDescending(b => b.PeriodStart)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Bill>> GetByPropertyIdsAsync(IEnumerable<Guid> propertyIds, CancellationToken cancellationToken = default)
    {
        var propertyIdList = propertyIds.ToList();
        return await _context.Bills
            .AsNoTracking()
            .Where(b => propertyIdList.Contains(b.PropertyId))
            .OrderByDescending(b => b.PeriodStart)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Bill>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Bills
            .AsNoTracking()
            .OrderByDescending(b => b.PeriodStart)
            .ToListAsync(cancellationToken);
    }

    public async Task<string> GetNextInvoiceNumberAsync(CancellationToken cancellationToken = default)
    {
        // Get all invoice numbers and find the max to be more concurrency-safe
        var invoiceNumbers = await _context.Bills
            .Select(b => b.InvoiceNumber)
            .ToListAsync(cancellationToken);

        if (invoiceNumbers.Count == 0)
        {
            return "UTIL-0001";
        }

        // Parse invoice numbers safely using regex and find the maximum
        var maxNumber = invoiceNumbers
            .Select(inv => InvoiceNumberRegex().Match(inv))
            .Where(m => m.Success)
            .Select(m => int.Parse(m.Groups[1].Value))
            .DefaultIfEmpty(0)
            .Max();

        return $"UTIL-{(maxNumber + 1):D4}";
    }

    public async Task AddAsync(Bill bill, CancellationToken cancellationToken = default)
    {
        await _context.Bills.AddAsync(bill, cancellationToken);
    }

    public void Delete(Bill bill)
    {
        _context.Bills.Remove(bill);
    }

    [GeneratedRegex(@"^UTIL-(\d+)$")]
    private static partial Regex InvoiceNumberRegex();
}
