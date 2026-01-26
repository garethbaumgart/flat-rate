namespace FlatRate.Application.Common;

/// <summary>
/// Service to access information about the currently authenticated user.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Gets the internal user ID (from Users table).
    /// Returns null if user is not authenticated or not found.
    /// </summary>
    Guid? UserId { get; }

    /// <summary>
    /// Gets the Google ID (OAuth subject identifier).
    /// </summary>
    string? GoogleId { get; }

    /// <summary>
    /// Gets the user's email address.
    /// </summary>
    string? Email { get; }

    /// <summary>
    /// Gets the user's display name.
    /// </summary>
    string? Name { get; }

    /// <summary>
    /// Returns true if the user is authenticated.
    /// </summary>
    bool IsAuthenticated { get; }
}
