using System.ComponentModel;
using FSH.Framework.Shared.Persistence;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Villsource.FSH.Modules.Organization.Contracts.v1.Positions.Commands;
using Villsource.FSH.Modules.Organization.Contracts.v1.Positions.Queries;

namespace Villsource.FSH.Modules.Organization.Features.v1.Positions.Endpoints;

public static class PositionEndpoints
{
    public static IEndpointRouteBuilder MapPositionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/positions")
            .WithTags("Positions")
            .RequireAuthorization()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapGet("/", async (
                [AsParameters] SearchPositionsRequest request,
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                var result = await mediator.Send(request.ToQuery(), cancellationToken);
                return Results.Ok(result.ToResponse());
            })
            .WithName("SearchPositions")
            .WithSummary("Search positions")
            .WithDescription(
                "Searches and paginates organizational positions with reporting hierarchy and job grade metadata.")
            .Produces<PagedResponse<PositionSummaryResponse>>(StatusCodes.Status200OK, "application/json")
            .ProducesValidationProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/lookups", async (
                [AsParameters] GetPositionLookupsRequest request,
                ISender mediator = default!,
                CancellationToken cancellationToken = default) =>
            {
                var result = await mediator.Send(request.ToQuery(), cancellationToken);
                return Results.Ok(result.ToResponse());
            })
            .WithName("GetPositionLookups")
            .WithSummary("List position lookups")
            .WithDescription(
                "Retrieves a lightweight list of positions ordered by authority rank for reporting and leadership selectors.")
            .Produces<IReadOnlyList<PositionLookupResponse>>(StatusCodes.Status200OK, "application/json")
            .ProducesValidationProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/{refId}", async (
                [Description("Unique reference ID of the position.")]
                string refId,
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                var result = await mediator.Send(new GetPositionByRefIdQuery(refId), cancellationToken);
                return Results.Ok(result.ToResponse());
            })
            .WithName("GetPosition")
            .WithSummary("Get position by reference ID")
            .Produces<PositionDetailResponse>(StatusCodes.Status200OK, "application/json")
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/{refId}/reporting-chain", async (
                [Description("Starting position reference ID.")]
                string refId,
                [AsParameters] GetPositionReportingChainRequest request,
                ISender mediator = default!,
                CancellationToken cancellationToken = default) =>
            {
                var result = await mediator.Send(request.ToQuery(refId), cancellationToken);
                return Results.Ok(result.ToResponse());
            })
            .WithName("GetPositionReportingChain")
            .WithSummary("Get upward reporting chain")
            .WithDescription(
                "Resolves the ordered chain of command upward from a position for document approval routing.")
            .Produces<IReadOnlyList<PositionNodeResponse>>(StatusCodes.Status200OK, "application/json")
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/vacancy-status", async (
                [AsParameters] GetVacancyStatusRequest request,
                ISender mediator = default!,
                CancellationToken cancellationToken = default) =>
            {
                var result = await mediator.Send(request.ToQuery(), cancellationToken);
                return Results.Ok(result.ToResponse());
            })
            .WithName("GetPositionVacancyStatus")
            .WithSummary("Get position headcount and vacancy status")
            .WithDescription("Queries headcount capacity against vacancies across positions in a legal entity or organization unit.")
            .Produces<IReadOnlyList<PositionVacancyResponse>>(StatusCodes.Status200OK, "application/json")
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/{refId}/approval-chain", async (
                [Description("Position reference ID.")]
                string refId,
                [AsParameters] ResolveApprovalAuthorityRequest request,
                ISender mediator = default!,
                CancellationToken cancellationToken = default) =>
            {
                var result = await mediator.Send(request.ToQuery(refId), cancellationToken);
                return Results.Ok(result.ToResponse());
            })
            .WithName("ResolveApprovalAuthority")
            .WithSummary("Resolve approval authority chain with delegations")
            .WithDescription("Resolves the ordered approval chain from a position, resolving active delegations and skip-level flags.")
            .Produces<ApprovalChainResponse>(StatusCodes.Status200OK, "application/json")
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/{refId}/subtree", async (
                [Description("Root manager position reference ID.")]
                string refId,
                [AsParameters] GetPositionSubtreeRequest request,
                ISender mediator = default!,
                CancellationToken cancellationToken = default) =>
            {
                var result = await mediator.Send(request.ToQuery(refId), cancellationToken);
                return Results.Ok(result.ToResponse());
            })
            .WithName("GetPositionSubtree")
            .WithSummary("Get subordinate reporting subtree")
            .WithDescription(
                "Resolves all subordinate positions reporting beneath a manager position in breadth-first order.")
            .Produces<IReadOnlyList<PositionNodeResponse>>(StatusCodes.Status200OK, "application/json")
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", async (
                [FromBody] CreatePositionRequest request,
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                var refId = await mediator.Send(request.ToCommand(), cancellationToken);
                return Results.CreatedAtRoute("GetPosition", new { refId }, refId);
            })
            .WithName("CreatePosition")
            .WithSummary("Create a position")
            .Accepts<CreatePositionRequest>("application/json")
            .Produces<string>(StatusCodes.Status201Created, "application/json")
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPut("/{refId}", async (
                [Description("Unique reference ID of the position.")]
                string refId,
                [FromBody] UpdatePositionRequest request,
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                return Results.Ok(await mediator.Send(request.ToCommand(refId), cancellationToken));
            })
            .WithName("UpdatePosition")
            .WithSummary("Update a position")
            .Accepts<UpdatePositionRequest>("application/json")
            .Produces<string>(StatusCodes.Status200OK, "application/json")
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPatch("/{refId}/activate", async (
                [Description("Unique reference ID of the position.")]
                string refId,
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                return Results.Ok(await mediator.Send(new ActivatePositionCommand(refId), cancellationToken));
            })
            .WithName("ActivatePosition")
            .WithSummary("Activate a position")
            .Produces<string>(StatusCodes.Status200OK, "application/json")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPatch("/{refId}/deactivate", async (
                [Description("Unique reference ID of the position.")]
                string refId,
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                return Results.Ok(await mediator.Send(new DeactivatePositionCommand(refId), cancellationToken));
            })
            .WithName("DeactivatePosition")
            .WithSummary("Deactivate a position")
            .Produces<string>(StatusCodes.Status200OK, "application/json")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{refId}", async (
                [Description("Unique reference ID of the position.")]
                string refId,
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                return Results.Ok(await mediator.Send(new DeletePositionCommand(refId), cancellationToken));
            })
            .WithName("DeletePosition")
            .WithSummary("Delete a position")
            .Produces<string>(StatusCodes.Status200OK, "application/json")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }
}
