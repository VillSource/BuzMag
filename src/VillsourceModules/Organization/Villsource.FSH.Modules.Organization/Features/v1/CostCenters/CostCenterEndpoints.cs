using System.ComponentModel;
using FluentValidation;
using FSH.Framework.Shared.Persistence;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Villsource.FSH.Modules.Organization.Contracts.v1.CostCenters;
using Villsource.FSH.Modules.Organization.Contracts.v1.CostCenters.Commands;
using Villsource.FSH.Modules.Organization.Contracts.v1.CostCenters.Dtos;
using Villsource.FSH.Modules.Organization.Contracts.v1.CostCenters.Queries;

namespace Villsource.FSH.Modules.Organization.Features.v1.CostCenters;
public static class CostCenterEndpoints
{
    public static IEndpointRouteBuilder MapCostCenterEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/cost-centers")
            .WithTags("Cost Centers")
            .RequireAuthorization()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        // Queries
        group.MapGet("/", async (
                [AsParameters] SearchCostCentersQuery query,
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                var result = await mediator.Send(request: query, cancellationToken: cancellationToken);
                return Results.Ok(result);
            })
            .WithName("SearchCostCenters")
            .WithSummary("Search cost centers")
            .WithDescription(
                "Searches and paginates cost centers with optional filters for legal entity, parent hierarchy, accounting function type, validity window, and status.")
            .Produces<PagedResponse<CostCenterSummaryDto>>(StatusCodes.Status200OK, "application/json")
            .ProducesValidationProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/function-types", async (
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                var result = await mediator.Send(
                    request: new GetCostCenterFunctionTypesQuery(),
                    cancellationToken: cancellationToken);
                return Results.Ok(result);
            })
            .WithName("GetCostCenterFunctionTypes")
            .WithSummary("List accounting function types")
            .WithDescription(
                "Retrieves standard IAS 1 / TFRS 1 functional cost classifications and their financial statement mapping metadata.")
            .Produces<IReadOnlyList<CostCenterFunctionTypeDto>>(StatusCodes.Status200OK, "application/json");

        group.MapGet("/{refId}", async (
                [Description("Unique reference ID of the cost center.")]
                string refId,
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                var result = await mediator.Send(
                    request: new GetCostCenterByRefIdQuery(RefId: refId),
                    cancellationToken: cancellationToken);
                return Results.Ok(result);
            })
            .WithName("GetCostCenter")
            .WithSummary("Get cost center by reference ID")
            .WithDescription(
                "Retrieves detailed cost center information including hierarchy links, accounting function metadata, default tax branch, and child cost centers.")
            .Produces<CostCenterDetailDto>(StatusCodes.Status200OK, "application/json")
            .ProducesProblem(StatusCodes.Status404NotFound);

        // Commands
        group.MapPost("/", async (
                [FromBody] CreateCostCenterCommand command,
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                var refId = await mediator.Send(request: command, cancellationToken: cancellationToken);
                return Results.CreatedAtRoute(
                    routeName: "GetCostCenter",
                    routeValues: new { refId },
                    value: refId);
            })
            .WithName("CreateCostCenter")
            .WithSummary("Create a cost center")
            .WithDescription("Creates a new cost center under a legal entity and returns its reference ID.")
            .Accepts<CreateCostCenterCommand>("application/json")
            .Produces<string>(StatusCodes.Status201Created, "application/json")
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPut("/{refId}", async (
                [Description("Unique reference ID of the cost center.")]
                string refId,
                [FromBody] UpdateCostCenterRequest request,
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                var command = request.ToCommand(refId: refId);
                var updatedRefId = await mediator.Send(request: command, cancellationToken: cancellationToken);
                return Results.Ok(updatedRefId);
            })
            .WithName("UpdateCostCenter")
            .WithSummary("Update a cost center")
            .WithDescription("Updates master attributes, hierarchy, accounting function type, and validity window of an existing cost center.")
            .Accepts<UpdateCostCenterRequest>("application/json")
            .Produces<string>(StatusCodes.Status200OK, "application/json")
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPatch("/{refId}/activate", async (
                [Description("Unique reference ID of the cost center.")]
                string refId,
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                var activatedRefId = await mediator.Send(
                    request: new ActivateCostCenterCommand(RefId: refId),
                    cancellationToken: cancellationToken);
                return Results.Ok(activatedRefId);
            })
            .WithName("ActivateCostCenter")
            .WithSummary("Activate a cost center")
            .WithDescription("Marks an inactive cost center as active if its parent cost center is active.")
            .Produces<string>(StatusCodes.Status200OK, "application/json")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPatch("/{refId}/deactivate", async (
                [Description("Unique reference ID of the cost center.")]
                string refId,
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                var deactivatedRefId = await mediator.Send(
                    request: new DeactivateCostCenterCommand(RefId: refId),
                    cancellationToken: cancellationToken);
                return Results.Ok(deactivatedRefId);
            })
            .WithName("DeactivateCostCenter")
            .WithSummary("Deactivate a cost center")
            .WithDescription("Marks an active cost center as inactive if no active child cost centers exist.")
            .Produces<string>(StatusCodes.Status200OK, "application/json")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPatch("/{refId}/block-posting", async (
                [Description("Unique reference ID of the cost center.")]
                string refId,
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                var blockedRefId = await mediator.Send(
                    request: new BlockCostCenterPostingCommand(RefId: refId),
                    cancellationToken: cancellationToken);
                return Results.Ok(blockedRefId);
            })
            .WithName("BlockCostCenterPosting")
            .WithSummary("Block cost center posting")
            .WithDescription("Locks new financial postings to the cost center while keeping it visible for reporting.")
            .Produces<string>(StatusCodes.Status200OK, "application/json")
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPatch("/{refId}/unblock-posting", async (
                [Description("Unique reference ID of the cost center.")]
                string refId,
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                var unblockedRefId = await mediator.Send(
                    request: new UnblockCostCenterPostingCommand(RefId: refId),
                    cancellationToken: cancellationToken);
                return Results.Ok(unblockedRefId);
            })
            .WithName("UnblockCostCenterPosting")
            .WithSummary("Unblock cost center posting")
            .WithDescription("Re-allows financial postings to a posting-blocked cost center.")
            .Produces<string>(StatusCodes.Status200OK, "application/json")
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{refId}", async (
                [Description("Unique reference ID of the cost center.")]
                string refId,
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                var deletedRefId = await mediator.Send(
                    request: new DeleteCostCenterCommand(RefId: refId),
                    cancellationToken: cancellationToken);
                return Results.Ok(deletedRefId);
            })
            .WithName("DeleteCostCenter")
            .WithSummary("Delete a cost center")
            .WithDescription("Soft-deletes a cost center if no active child cost centers exist.")
            .Produces<string>(StatusCodes.Status200OK, "application/json")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }
}
