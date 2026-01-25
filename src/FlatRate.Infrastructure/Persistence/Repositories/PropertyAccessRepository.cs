using FlatRate.Domain.Aggregates.Properties;
using Microsoft.EntityFrameworkCore;

namespace FlatRate.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of IPropertyAccessRepository.
/// </summary>
public sealed class PropertyAccessRepository : IPropertyAccessRepository
{
    private readonly FlatRateDbContext _context;

    public PropertyAccessRepository(FlatRateDbContext context)
    {
        _context = context;
    }

    public async Task<PropertyAccess?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.PropertyAccess
            .FirstOrDefaultAsync(pa => pa.Id == id, cancellationToken);
    }

    public async Task<PropertyAccess?> GetByPropertyAndUserAsync(Guid propertyId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.PropertyAccess
            .FirstOrDefaultAsync(pa => pa.PropertyId == propertyId && pa.UserId == userId, cancellationToken);
    }

    public async Task<IReadOnlyList<PropertyAccess>> GetByPropertyIdAsync(Guid propertyId, CancellationToken cancellationToken = default)
    {
        return await _context.PropertyAccess
            .AsNoTracking()
            .Where(pa => pa.PropertyId == propertyId)
            .OrderBy(pa => pa.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PropertyAccess>> GetPendingByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        return await _context.PropertyAccess
            .Where(pa => pa.InvitedEmail == normalizedEmail && pa.UserId == null)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(PropertyAccess access, CancellationToken cancellationToken = default)
    {
        await _context.PropertyAccess.AddAsync(access, cancellationToken);
    }

    public void Update(PropertyAccess access)
    {
        _context.PropertyAccess.Update(access);
    }

    public void Delete(PropertyAccess access)
    {
        _context.PropertyAccess.Remove(access);
    }
}
