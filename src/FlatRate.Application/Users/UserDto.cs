namespace FlatRate.Application.Users;

/// <summary>
/// Data transfer object for user information.
/// </summary>
public sealed record UserDto(
    Guid Id,
    string Email,
    string Name);
