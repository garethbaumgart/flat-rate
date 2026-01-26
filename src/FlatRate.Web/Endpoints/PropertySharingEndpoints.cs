using FlatRate.Application.Properties;
using FlatRate.Application.Properties.Commands.InviteToProperty;
using FlatRate.Application.Properties.Commands.RevokePropertyAccess;
using FlatRate.Application.Properties.Queries.GetPropertyCollaborators;
using FlatRate.Domain.Aggregates.Properties;
using MediatR;

namespace FlatRate.Web.Endpoints;

public static class PropertySharingEndpoints
{
    public static void MapPropertySharingEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/properties/{propertyId:guid}/collaborators")
            .RequireAuthorization();

        group.MapGet("/", async (Guid propertyId, IMediator mediator) =>
        {
            var collaborators = await mediator.Send(new GetPropertyCollaboratorsQuery(propertyId));
            return Results.Ok(collaborators);
        });

        group.MapPost("/", async (Guid propertyId, InviteRequest request, IMediator mediator) =>
        {
            var role = request.Role?.ToLowerInvariant() switch
            {
                "owner" => PropertyRole.Owner,
                _ => PropertyRole.Editor
            };

            var result = await mediator.Send(new InviteToPropertyCommand(propertyId, request.Email, role));

            if (!result.Success)
            {
                return Results.BadRequest(new { error = result.ErrorMessage });
            }

            return Results.Ok();
        });

        group.MapDelete("/{userId:guid}", async (Guid propertyId, Guid userId, IMediator mediator) =>
        {
            var result = await mediator.Send(new RevokePropertyAccessCommand(propertyId, userId));

            if (!result.Success)
            {
                return Results.BadRequest(new { error = result.ErrorMessage });
            }

            return Results.Ok();
        });
    }
}

public record InviteRequest(string Email, string? Role = null);
