using FlatRate.Domain.Common;

namespace FlatRate.Domain.Aggregates.Properties;

/// <summary>
/// Represents a user's access to a property.
/// Can be either an active access (UserId is set) or a pending invite (InvitedEmail is set).
/// </summary>
public sealed class PropertyAccess : Entity
{
    public Guid PropertyId { get; private set; }
    public Guid? UserId { get; private set; }
    public string? InvitedEmail { get; private set; }
    public PropertyRole Role { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? AcceptedAt { get; private set; }

    private PropertyAccess() : base()
    {
    }

    /// <summary>
    /// Creates an active access grant for an existing user.
    /// </summary>
    public static PropertyAccess CreateForUser(Guid propertyId, Guid userId, PropertyRole role)
    {
        if (propertyId == Guid.Empty)
            throw new ArgumentException("Property ID cannot be empty.", nameof(propertyId));

        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));

        var now = DateTime.UtcNow;
        return new PropertyAccess
        {
            PropertyId = propertyId,
            UserId = userId,
            InvitedEmail = null,
            Role = role,
            CreatedAt = now,
            AcceptedAt = now // Immediately active
        };
    }

    /// <summary>
    /// Creates a pending invite for an email address.
    /// Will be converted to active access when the user signs up.
    /// </summary>
    public static PropertyAccess CreatePendingInvite(Guid propertyId, string email, PropertyRole role)
    {
        if (propertyId == Guid.Empty)
            throw new ArgumentException("Property ID cannot be empty.", nameof(propertyId));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty.", nameof(email));

        return new PropertyAccess
        {
            PropertyId = propertyId,
            UserId = null,
            InvitedEmail = email.Trim().ToLowerInvariant(),
            Role = role,
            CreatedAt = DateTime.UtcNow,
            AcceptedAt = null
        };
    }

    /// <summary>
    /// Accepts a pending invite by linking it to a user.
    /// </summary>
    public void AcceptInvite(Guid userId)
    {
        if (UserId is not null)
            throw new InvalidOperationException("This access is already active.");

        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));

        UserId = userId;
        InvitedEmail = null;
        AcceptedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Returns true if this is a pending invite (not yet accepted).
    /// </summary>
    public bool IsPending => UserId is null && InvitedEmail is not null;

    /// <summary>
    /// Returns true if this is an active access grant.
    /// </summary>
    public bool IsActive => UserId is not null;
}
