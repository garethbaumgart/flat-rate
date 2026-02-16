using FlatRate.Domain.Common;

namespace FlatRate.Domain.Aggregates.Users;

/// <summary>
/// Represents an application user authenticated via Google OAuth.
/// </summary>
public sealed class User : AggregateRoot
{
    public string GoogleId { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? AvatarUrl { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset LastLoginAt { get; private set; }

    private User() : base()
    {
    }

    public static User Create(string googleId, string email, string name, string? avatarUrl = null)
    {
        if (string.IsNullOrWhiteSpace(googleId))
            throw new ArgumentException("Google ID cannot be empty.", nameof(googleId));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty.", nameof(email));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));

        var now = DateTimeOffset.UtcNow;
        return new User
        {
            GoogleId = googleId.Trim(),
            Email = email.Trim().ToLowerInvariant(),
            Name = name.Trim(),
            AvatarUrl = avatarUrl?.Trim(),
            CreatedAt = now,
            LastLoginAt = now
        };
    }

    public void UpdateLastLogin()
    {
        LastLoginAt = DateTimeOffset.UtcNow;
    }

    public void UpdateProfile(string name, string email, string? avatarUrl = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty.", nameof(email));

        Name = name.Trim();
        Email = email.Trim().ToLowerInvariant();

        if (!string.IsNullOrWhiteSpace(avatarUrl))
            AvatarUrl = avatarUrl.Trim();
    }
}
