using FlatRate.Domain.Aggregates.Properties;
using Microsoft.EntityFrameworkCore;

namespace FlatRate.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of IPropertyRepository.
/// </summary>
public sealed class PropertyRepository : IPropertyRepository
{
    private readonly FlatRateDbContext _context;

    public PropertyRepository(FlatRateDbContext context)
    {
        _context = context;
    }

    public async Task<Property?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Properties
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Property>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Properties
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Property property, CancellationToken cancellationToken = default)
    {
        await _context.Properties.AddAsync(property, cancellationToken);
    }

    public void Update(Property property)
    {
        // EF Core tracks changes automatically for tracked entities
        // This method exists for detached entity scenarios
        _context.Properties.Update(property);
    }

    public void Delete(Property property)
    {
        _context.Properties.Remove(property);
    }

    public async Task<IReadOnlyList<Property>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // Get all properties that the user has access to
        var propertyIds = await _context.PropertyAccess
            .Where(pa => pa.UserId == userId)
            .Select(pa => pa.PropertyId)
            .ToListAsync(cancellationToken);

        return await _context.Properties
            .AsNoTracking()
            .Where(p => propertyIds.Contains(p.Id))
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> UserHasAccessAsync(Guid propertyId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.PropertyAccess
            .AnyAsync(pa => pa.PropertyId == propertyId && pa.UserId == userId, cancellationToken);
    }

    public async Task<PropertyRole?> GetUserRoleAsync(Guid propertyId, Guid userId, CancellationToken cancellationToken = default)
    {
        var access = await _context.PropertyAccess
            .FirstOrDefaultAsync(pa => pa.PropertyId == propertyId && pa.UserId == userId, cancellationToken);

        return access?.Role;
    }
}
