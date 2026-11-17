using System.ComponentModel;
using FSH.Framework.Shared.Persistence;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Villsource.FSH.Modules.Organization.Contracts.v1.OrganizationUnitTypes.Commands;
using Villsource.FSH.Modules.Organization.Contracts.v1.OrganizationUnitTypes.Queries;

namespace Villsource.FSH.Modules.Organization.Features.v1.OrganizationUnitTypes.Endpoints;

public static class OrganizationUnitTypeEndpoints
{
    public static IEndpointRouteBuilder MapOrganizationUnitTypeEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/unit-types")
            .WithTags("Organization Unit Types")
            .RequireAuthorization()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapGet("/", async (
                [AsParameters] SearchOrganizationUnitTypesRequest request,
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                var result = await mediator.Send(request.ToQuery(), cancellationToken);
                return Results.Ok(result.ToResponse());
            })
            .WithName("SearchOrganizationUnitTypes")
            .WithSummary("Search organization unit types")
            .Produces<PagedResponse<OrganizationUnitTypeSummaryResponse>>(StatusCodes.Status200OK, "application/json")
            .ProducesValidationProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/lookups", async (
                [FromQuery, Description("Filter to active unit types only (default: true).")] bool activeOnly = true,
                ISender mediator = default!,
                CancellationToken cancellationToken = default) =>
            {
                var result = await mediator.Send(new GetOrganizationUnitTypeLookupsQuery(activeOnly), cancellationToken);
                return Results.Ok(result.ToResponse());
            })
            .WithName("GetOrganizationUnitTypeLookups")
            .WithSummary("List organization unit type lookups")
            .Produces<IReadOnlyList<OrganizationUnitTypeLookupResponse>>(StatusCodes.Status200OK, "application/json");

        group.MapGet("/{refId}", async (
                [Description("Unique reference ID of the organization unit type.")] string refId,
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                var result = await mediator.Send(new GetOrganizationUnitTypeByRefIdQuery(refId), cancellationToken);
                return Results.Ok(result.ToResponse());
            })
            .WithName("GetOrganizationUnitType")
            .WithSummary("Get organization unit type by reference ID")
            .Produces<OrganizationUnitTypeDetailResponse>(StatusCodes.Status200OK, "application/json")
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", async (
                [FromBody] CreateOrganizationUnitTypeRequest request,
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                var refId = await mediator.Send(request.ToCommand(), cancellationToken);
                return Results.CreatedAtRoute("GetOrganizationUnitType", new { refId }, refId);
            })
            .WithName("CreateOrganizationUnitType")
            .WithSummary("Create an organization unit type")
            .Accepts<CreateOrganizationUnitTypeRequest>("application/json")
            .Produces<string>(StatusCodes.Status201Created, "application/json")
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPut("/{refId}", async (
                [Description("Unique reference ID of the organization unit type.")] string refId,
                [FromBody] UpdateOrganizationUnitTypeRequest request,
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                var updatedRefId = await mediator.Send(request.ToCommand(refId), cancellationToken);
                return Results.Ok(updatedRefId);
            })
            .WithName("UpdateOrganizationUnitType")
            .WithSummary("Update an organization unit type")
            .Accepts<UpdateOrganizationUnitTypeRequest>("application/json")
            .Produces<string>(StatusCodes.Status200OK, "application/json")
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/{refId}/activate", async (
                [Description("Unique reference ID of the organization unit type.")] string refId,
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                await mediator.Send(new ActivateOrganizationUnitTypeCommand(refId), cancellationToken);
                return Results.NoContent();
            })
            .WithName("ActivateOrganizationUnitType")
            .WithSummary("Activate an organization unit type")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/{refId}/deactivate", async (
                [Description("Unique reference ID of the organization unit type.")] string refId,
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                await mediator.Send(new DeactivateOrganizationUnitTypeCommand(refId), cancellationToken);
                return Results.NoContent();
            })
            .WithName("DeactivateOrganizationUnitType")
            .WithSummary("Deactivate an organization unit type")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{refId}", async (
                [Description("Unique reference ID of the organization unit type.")] string refId,
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                await mediator.Send(new DeleteOrganizationUnitTypeCommand(refId), cancellationToken);
                return Results.NoContent();
            })
            .WithName("DeleteOrganizationUnitType")
            .WithSummary("Delete an organization unit type")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        return app;
    }
}
