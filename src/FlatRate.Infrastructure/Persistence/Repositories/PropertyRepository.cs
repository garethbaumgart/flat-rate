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
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Property property, CancellationToken cancellationToken = default)
    {
        await _context.Properties.AddAsync(property, cancellationToken);
    }

    public void Update(Property property)
    {
        _context.Properties.Update(property);
    }

    public void Delete(Property property)
    {
        _context.Properties.Remove(property);
    }
}
