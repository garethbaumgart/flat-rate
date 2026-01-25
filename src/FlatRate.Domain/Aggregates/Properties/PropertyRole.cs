namespace FlatRate.Domain.Aggregates.Properties;

/// <summary>
/// Defines the role a user has in relation to a property.
/// </summary>
public enum PropertyRole
{
    /// <summary>
    /// Full control - can edit property, manage bills, and share with others.
    /// </summary>
    Owner = 0,

    /// <summary>
    /// Can view and edit property/bills, but cannot share or delete the property.
    /// </summary>
    Editor = 1
}
