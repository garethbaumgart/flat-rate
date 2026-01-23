using MediatR;

namespace FlatRate.Application.Properties.Queries.GetPropertyById;

/// <summary>
/// Query to get a property by ID.
/// </summary>
public sealed record GetPropertyByIdQuery(Guid Id) : IRequest<PropertyDto?>;
