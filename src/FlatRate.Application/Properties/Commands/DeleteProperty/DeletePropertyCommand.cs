using MediatR;

namespace FlatRate.Application.Properties.Commands.DeleteProperty;

/// <summary>
/// Command to delete a property.
/// </summary>
public sealed record DeletePropertyCommand(Guid Id) : IRequest<bool>;
