using MediatR;

namespace FlatRate.Application.Properties.Commands.RevokePropertyAccess;

/// <summary>
/// Command to revoke a user's access to a property.
/// </summary>
public sealed record RevokePropertyAccessCommand(
    Guid PropertyId,
    Guid UserId) : IRequest<RevokePropertyAccessResult>;

public sealed record RevokePropertyAccessResult(
    bool Success,
    string? ErrorMessage = null);
