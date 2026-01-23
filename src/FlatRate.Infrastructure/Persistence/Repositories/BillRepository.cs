using FlatRate.Domain.Aggregates.Bills;
using Microsoft.EntityFrameworkCore;

namespace FlatRate.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of IBillRepository.
/// </summary>
public sealed class BillRepository : IBillRepository
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
            .Where(b => b.PropertyId == propertyId)
            .OrderByDescending(b => b.PeriodStart)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Bill>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Bills
            .OrderByDescending(b => b.PeriodStart)
            .ToListAsync(cancellationToken);
    }

    public async Task<string> GetNextInvoiceNumberAsync(CancellationToken cancellationToken = default)
    {
        var lastBill = await _context.Bills
            .OrderByDescending(b => b.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (lastBill is null)
        {
            return "UTIL-0001";
        }

        // Extract number from existing format (UTIL-XXXX)
        var lastNumber = int.Parse(lastBill.InvoiceNumber.Replace("UTIL-", ""));
        return $"UTIL-{(lastNumber + 1):D4}";
    }

    public async Task AddAsync(Bill bill, CancellationToken cancellationToken = default)
    {
        await _context.Bills.AddAsync(bill, cancellationToken);
    }

    public void Delete(Bill bill)
    {
        _context.Bills.Remove(bill);
    }
}
