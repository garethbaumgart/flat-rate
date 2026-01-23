namespace FlatRate.Domain.Aggregates.Bills;

/// <summary>
/// Repository interface for Bill aggregate.
/// </summary>
public interface IBillRepository
{
    Task<Bill?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Bill>> GetByPropertyIdAsync(Guid propertyId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Bill>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<string> GetNextInvoiceNumberAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Bill bill, CancellationToken cancellationToken = default);
    void Delete(Bill bill);
}
