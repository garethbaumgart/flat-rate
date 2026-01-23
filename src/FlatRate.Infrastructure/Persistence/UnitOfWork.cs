using FlatRate.Domain.Repositories;

namespace FlatRate.Infrastructure.Persistence;

/// <summary>
/// EF Core implementation of IUnitOfWork.
/// </summary>
public sealed class UnitOfWork : IUnitOfWork
{
    private readonly FlatRateDbContext _context;

    public UnitOfWork(FlatRateDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
