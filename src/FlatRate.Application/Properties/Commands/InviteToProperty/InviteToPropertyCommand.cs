using FlatRate.Domain.Aggregates.Properties;
using MediatR;

namespace FlatRate.Application.Properties.Commands.InviteToProperty;

/// <summary>
/// Command to invite a user to a property by email.
/// </summary>
public sealed record InviteToPropertyCommand(
    Guid PropertyId,
    string Email,
    PropertyRole Role = PropertyRole.Editor) : IRequest<InviteToPropertyResult>;

public sealed record InviteToPropertyResult(
    bool Success,
    string? ErrorMessage = null);
