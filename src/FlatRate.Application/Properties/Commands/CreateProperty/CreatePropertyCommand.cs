using MediatR;

namespace FlatRate.Application.Properties.Commands.CreateProperty;

/// <summary>
/// Command to create a new property.
/// </summary>
public sealed record CreatePropertyCommand(
    string Name,
    string Address) : IRequest<Guid>;
