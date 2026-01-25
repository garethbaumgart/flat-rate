namespace FlatRate.Domain.Aggregates.Properties;

/// <summary>
/// Repository interface for Property aggregate.
/// </summary>
public interface IPropertyRepository
{
    Task<Property?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Property>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Property property, CancellationToken cancellationToken = default);
    void Update(Property property);
    void Delete(Property property);

    // User-filtered queries
    Task<IReadOnlyList<Property>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> UserHasAccessAsync(Guid propertyId, Guid userId, CancellationToken cancellationToken = default);
    Task<PropertyRole?> GetUserRoleAsync(Guid propertyId, Guid userId, CancellationToken cancellationToken = default);
}
