using MediatR;

namespace FlatRate.Application.Properties.Queries.GetAllProperties;

/// <summary>
/// Query to get all properties.
/// </summary>
public sealed record GetAllPropertiesQuery : IRequest<IReadOnlyList<PropertyDto>>;
