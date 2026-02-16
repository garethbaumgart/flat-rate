using MediatR;

namespace FlatRate.Application.Users.Commands.EnsureUserExists;

/// <summary>
/// Command to ensure a user exists in the database.
/// Creates the user if they don't exist, updates last login if they do.
/// Returns the user's internal ID.
/// </summary>
public sealed record EnsureUserExistsCommand(
    string GoogleId,
    string Email,
    string Name,
    string? AvatarUrl = null) : IRequest<Guid>;
