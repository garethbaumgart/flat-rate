using FlatRate.Domain.Aggregates.Properties;

namespace FlatRate.Application.Properties;

/// <summary>
/// Data transfer object for property collaborators.
/// </summary>
public sealed record CollaboratorDto(
    Guid? UserId,
    string? Email,
    string? Name,
    PropertyRole Role,
    bool IsPending,
    DateTime CreatedAt,
    DateTime? AcceptedAt);
