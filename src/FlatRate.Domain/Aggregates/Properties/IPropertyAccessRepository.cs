namespace FlatRate.Domain.Aggregates.Properties;

/// <summary>
/// Repository interface for PropertyAccess entities.
/// </summary>
public interface IPropertyAccessRepository
{
    Task<PropertyAccess?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PropertyAccess?> GetByPropertyAndUserAsync(Guid propertyId, Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PropertyAccess>> GetByPropertyIdAsync(Guid propertyId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PropertyAccess>> GetPendingByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task AddAsync(PropertyAccess access, CancellationToken cancellationToken = default);
    void Update(PropertyAccess access);
    void Delete(PropertyAccess access);
}
