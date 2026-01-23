using MediatR;

namespace FlatRate.Application.Properties.Commands.UpdateProperty;

/// <summary>
/// Command to update an existing property.
/// </summary>
public sealed record UpdatePropertyCommand(
    Guid Id,
    string Name,
    string Address) : IRequest<bool>;
