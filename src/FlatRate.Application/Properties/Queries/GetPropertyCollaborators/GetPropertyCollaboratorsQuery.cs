using MediatR;

namespace FlatRate.Application.Properties.Queries.GetPropertyCollaborators;

/// <summary>
/// Query to get all collaborators for a property.
/// </summary>
public sealed record GetPropertyCollaboratorsQuery(Guid PropertyId) : IRequest<IReadOnlyList<CollaboratorDto>>;
