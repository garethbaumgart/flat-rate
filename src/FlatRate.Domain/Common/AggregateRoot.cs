namespace FlatRate.Domain.Common;

/// <summary>
/// Base class for aggregate roots.
/// </summary>
public abstract class AggregateRoot : Entity
{
    protected AggregateRoot() : base()
    {
    }

    protected AggregateRoot(Guid id) : base(id)
    {
    }
}
