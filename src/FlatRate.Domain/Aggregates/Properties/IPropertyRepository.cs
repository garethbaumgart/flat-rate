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
}
