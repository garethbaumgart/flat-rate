using FlatRate.Application.Properties;
using FlatRate.Application.Properties.Commands.CreateProperty;
using FlatRate.Application.Properties.Commands.DeleteProperty;
using FlatRate.Application.Properties.Commands.SetPropertyRates;
using FlatRate.Application.Properties.Commands.UpdateProperty;
using FlatRate.Application.Properties.Queries.GetAllProperties;
using FlatRate.Application.Properties.Queries.GetPropertyById;
using MediatR;

namespace FlatRate.Web.Endpoints;

public static class PropertyEndpoints
{
    public static void MapPropertyEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/properties")
            .RequireAuthorization();

        group.MapGet("/", GetAll);
        group.MapGet("/{id:guid}", GetById);
        group.MapPost("/", Create);
        group.MapPut("/{id:guid}", Update);
        group.MapPut("/{id:guid}/rates", SetRates);
        group.MapDelete("/{id:guid}", Delete);
    }

    private static async Task<IResult> GetAll(IMediator mediator, CancellationToken cancellationToken)
    {
        var properties = await mediator.Send(new GetAllPropertiesQuery(), cancellationToken);
        return Results.Ok(properties);
    }

    private static async Task<IResult> GetById(Guid id, IMediator mediator, CancellationToken cancellationToken)
    {
        var property = await mediator.Send(new GetPropertyByIdQuery(id), cancellationToken);

        return property is null
            ? Results.NotFound(new { error = "Property not found" })
            : Results.Ok(property);
    }

    private static async Task<IResult> Create(CreatePropertyRequest request, IMediator mediator, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Results.BadRequest(new { error = "Name is required" });
        }

        if (string.IsNullOrWhiteSpace(request.Address))
        {
            return Results.BadRequest(new { error = "Address is required" });
        }

        try
        {
            var id = await mediator.Send(new CreatePropertyCommand(request.Name, request.Address), cancellationToken);
            return Results.Created($"/api/properties/{id}", new { id });
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> Update(Guid id, UpdatePropertyRequest request, IMediator mediator, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Results.BadRequest(new { error = "Name is required" });
        }

        if (string.IsNullOrWhiteSpace(request.Address))
        {
            return Results.BadRequest(new { error = "Address is required" });
        }

        try
        {
            var success = await mediator.Send(new UpdatePropertyCommand(id, request.Name, request.Address), cancellationToken);

            return success
                ? Results.NoContent()
                : Results.NotFound(new { error = "Property not found" });
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> SetRates(Guid id, SetPropertyRatesRequest request, IMediator mediator, CancellationToken cancellationToken)
    {
        try
        {
            var command = new SetPropertyRatesCommand(
                id,
                request.ElectricityRate,
                request.WaterRateTier1,
                request.WaterRateTier2,
                request.WaterRateTier3,
                request.SanitationRateTier1,
                request.SanitationRateTier2,
                request.SanitationRateTier3);

            var success = await mediator.Send(command, cancellationToken);

            return success
                ? Results.NoContent()
                : Results.NotFound(new { error = "Property not found" });
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> Delete(Guid id, IMediator mediator, CancellationToken cancellationToken)
    {
        var success = await mediator.Send(new DeletePropertyCommand(id), cancellationToken);

        return success
            ? Results.NoContent()
            : Results.NotFound(new { error = "Property not found" });
    }
}

public record CreatePropertyRequest(string Name, string Address);
public record UpdatePropertyRequest(string Name, string Address);
public record SetPropertyRatesRequest(
    decimal? ElectricityRate,
    decimal? WaterRateTier1,
    decimal? WaterRateTier2,
    decimal? WaterRateTier3,
    decimal? SanitationRateTier1,
    decimal? SanitationRateTier2,
    decimal? SanitationRateTier3);
