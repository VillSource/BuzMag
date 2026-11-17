using System.ComponentModel;
using FSH.Framework.Shared.Persistence;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Villsource.FSH.Modules.Organization.Contracts.v1.OrganizationUnits.Commands;
using Villsource.FSH.Modules.Organization.Contracts.v1.OrganizationUnits.Queries;

namespace Villsource.FSH.Modules.Organization.Features.v1.OrganizationUnits.Endpoints;

public static class OrganizationUnitEndpoints
{
    public static IEndpointRouteBuilder MapOrganizationUnitEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/organization-units")
            .WithTags("Organization Units")
            .RequireAuthorization()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapGet("/", async (
                [AsParameters] SearchOrganizationUnitsRequest request,
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                var result = await mediator.Send(request: request.ToQuery(), cancellationToken: cancellationToken).ConfigureAwait(false);
                return Results.Ok(result.ToResponse());
            })
            .WithName("SearchOrganizationUnits")
            .WithSummary("Search organization units")
            .WithDescription("Searches and paginates organization units with optional hierarchy, legal entity, manager position, and vacancy filters.")
            .Produces<PagedResponse<OrganizationUnitSummaryResponse>>(StatusCodes.Status200OK, "application/json")
            .ProducesValidationProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/tree", async (
                [AsParameters] GetOrganizationTreeRequest request,
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                var result = await mediator.Send(
                    request: request.ToQuery(),
                    cancellationToken: cancellationToken).ConfigureAwait(false);
                return Results.Ok(result.ToResponse());
            })
            .WithName("GetOrganizationTree")
            .WithSummary("Get organization tree")
            .WithDescription("Retrieves the hierarchical tree of organization units for a legal entity with optional depth filtering.")
            .Produces<IReadOnlyList<OrganizationTreeNodeResponse>>(StatusCodes.Status200OK, "application/json")
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/{refId}", async (
                [Description("Unique reference ID of the organization unit.")] string refId,
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                var result = await mediator.Send(
                    request: new GetOrganizationUnitByRefIdQuery(RefId: refId),
                    cancellationToken: cancellationToken).ConfigureAwait(false);
                return Results.Ok(result.ToResponse());
            })
            .WithName("GetOrganizationUnit")
            .WithSummary("Get organization unit by reference ID")
            .WithDescription("Retrieves detailed organization unit information including its designated manager position, parent, and active child units.")
            .Produces<OrganizationUnitDetailResponse>(StatusCodes.Status200OK, "application/json")
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/{refId}/leadership-chain", async (
                [Description("Unique reference ID of the organization unit.")] string refId,
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                var result = await mediator.Send(
                    request: new GetOrganizationUnitLeadershipChainQuery(RefId: refId),
                    cancellationToken: cancellationToken).ConfigureAwait(false);
                return Results.Ok(result.ToResponse());
            })
            .WithName("GetOrganizationUnitLeadershipChain")
            .WithSummary("Get bottom-up leadership position chain")
            .WithDescription("Returns ordered manager positions from the target organization unit up to the root unit so downstream approval/HR resolvers can handle vacant manager positions and skip-level escalation.")
            .Produces<OrganizationUnitLeadershipChainResponse>(StatusCodes.Status200OK, "application/json")
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", async (
                [FromBody] CreateOrganizationUnitRequest request,
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                var refId = await mediator.Send(request: request.ToCommand(), cancellationToken: cancellationToken).ConfigureAwait(false);
                return Results.CreatedAtRoute(
                    routeName: "GetOrganizationUnit",
                    routeValues: new { refId },
                    value: refId);
            })
            .WithName("CreateOrganizationUnit")
            .WithSummary("Create an organization unit")
            .WithDescription("Creates a new organization unit (root or under an existing parent unit) and optionally assigns a manager position.")
            .Accepts<CreateOrganizationUnitRequest>("application/json")
            .Produces<string>(StatusCodes.Status201Created, "application/json")
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/{refId}", async (
                [Description("Unique reference ID of the organization unit.")] string refId,
                [FromBody] UpdateOrganizationUnitRequest request,
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                var command = request.ToCommand(refId: refId);
                var updatedRefId = await mediator.Send(request: command, cancellationToken: cancellationToken).ConfigureAwait(false);
                return Results.Ok(updatedRefId);
            })
            .WithName("UpdateOrganizationUnit")
            .WithSummary("Update an organization unit")
            .WithDescription("Updates core metadata of an existing organization unit.")
            .Accepts<UpdateOrganizationUnitRequest>("application/json")
            .Produces<string>(StatusCodes.Status200OK, "application/json")
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/{refId}/manager-position", async (
                [Description("Unique reference ID of the organization unit.")] string refId,
                [FromBody] AssignManagerPositionRequest request,
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                var command = request.ToCommand(refId: refId);
                var updatedRefId = await mediator.Send(request: command, cancellationToken: cancellationToken).ConfigureAwait(false);
                return Results.Ok(updatedRefId);
            })
            .WithName("AssignOrganizationUnitManagerPosition")
            .WithSummary("Assign or clear manager position of an organization unit")
            .WithDescription("Designates an active position as manager of the organization unit (or clears it when managerRefId is null).")
            .Accepts<AssignManagerPositionRequest>("application/json")
            .Produces<string>(StatusCodes.Status200OK, "application/json")
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPatch("/{refId}/move", async (
                [Description("Unique reference ID of the organization unit.")] string refId,
                [FromBody] MoveOrganizationUnitRequest request,
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                var command = request.ToCommand(refId: refId);
                var movedRefId = await mediator.Send(request: command, cancellationToken: cancellationToken).ConfigureAwait(false);
                return Results.Ok(movedRefId);
            })
            .WithName("MoveOrganizationUnit")
            .WithSummary("Move an organization unit")
            .WithDescription("Reparents an organization unit to a new parent (or promotes to root when parentRefId is null) and recalculates descendant hierarchy paths.")
            .Accepts<MoveOrganizationUnitRequest>("application/json")
            .Produces<string>(StatusCodes.Status200OK, "application/json")
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPatch("/{refId}/activate", async (
                [Description("Unique reference ID of the organization unit.")] string refId,
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                var activatedRefId = await mediator.Send(
                    request: new ActivateOrganizationUnitCommand(RefId: refId),
                    cancellationToken: cancellationToken).ConfigureAwait(false);
                return Results.Ok(activatedRefId);
            })
            .WithName("ActivateOrganizationUnit")
            .WithSummary("Activate an organization unit")
            .WithDescription("Marks an organization unit as active.")
            .Produces<string>(StatusCodes.Status200OK, "application/json")
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPatch("/{refId}/deactivate", async (
                [Description("Unique reference ID of the organization unit.")] string refId,
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                var deactivatedRefId = await mediator.Send(
                    request: new DeactivateOrganizationUnitCommand(RefId: refId),
                    cancellationToken: cancellationToken).ConfigureAwait(false);
                return Results.Ok(deactivatedRefId);
            })
            .WithName("DeactivateOrganizationUnit")
            .WithSummary("Deactivate an organization unit")
            .WithDescription("Marks an organization unit as inactive.")
            .Produces<string>(StatusCodes.Status200OK, "application/json")
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{refId}", async (
                [Description("Unique reference ID of the organization unit.")] string refId,
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                var deletedRefId = await mediator.Send(
                    request: new DeleteOrganizationUnitCommand(RefId: refId),
                    cancellationToken: cancellationToken).ConfigureAwait(false);
                return Results.Ok(deletedRefId);
            })
            .WithName("DeleteOrganizationUnit")
            .WithSummary("Delete an organization unit")
            .WithDescription("Soft-deletes an organization unit if it has no active child units.")
            .Produces<string>(StatusCodes.Status200OK, "application/json")
            .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }
}
