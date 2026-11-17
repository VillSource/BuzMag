using FSH.Framework.Shared.Persistence;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System.ComponentModel;
using Villsource.FSH.Modules.Organization.Contracts.v1.Positions.Commands;
using Villsource.FSH.Modules.Organization.Contracts.v1.Positions.Dtos;
using Villsource.FSH.Modules.Organization.Contracts.v1.Positions.Queries;
using PositionRequestMapper = Villsource.FSH.Modules.Organization.Features.v1.Positions.Mappers.PositionRequestMapper;

namespace Villsource.FSH.Modules.Organization.Features.v1.Positions.Endpoinsts;

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
                [AsParameters] SearchPositionsQuery query,
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                return Results.Ok(await mediator.Send(query, cancellationToken));
            })
            .WithName("SearchPositions")
            .WithSummary("Search positions")
            .WithDescription(
                "Searches and paginates organizational positions with reporting hierarchy and job grade metadata.")
            .Produces<PagedResponse<PositionSummaryDto>>(StatusCodes.Status200OK, "application/json")
            .ProducesValidationProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/lookups", async (
                [FromQuery, Description("Optional legal entity reference ID filter.")]
                string? legalEntityRefId = null,
                [FromQuery, Description("Optional organization unit ID filter.")]
                Guid? orgUnitId = null,
                [FromQuery, Description("Filter to active positions only (default: true).")]
                bool activeOnly = true,
                [FromQuery, Description("Filter to primary leadership positions only (default: false).")]
                bool leadershipOnly = false,
                ISender mediator = default!,
                CancellationToken cancellationToken = default) =>
            {
                var result = await mediator.Send(
                    new GetPositionLookupsQuery(legalEntityRefId, orgUnitId, activeOnly, leadershipOnly),
                    cancellationToken);
                return Results.Ok(result);
            })
            .WithName("GetPositionLookups")
            .WithSummary("List position lookups")
            .WithDescription(
                "Retrieves a lightweight list of positions ordered by authority rank for reporting and leadership selectors.")
            .Produces<IReadOnlyList<PositionLookupDto>>(StatusCodes.Status200OK, "application/json")
            .ProducesValidationProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/{refId}", async (
                [Description("Unique reference ID of the position.")]
                string refId,
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                return Results.Ok(await mediator.Send(new GetPositionByRefIdQuery(refId), cancellationToken));
            })
            .WithName("GetPosition")
            .WithSummary("Get position by reference ID")
            .Produces<PositionDetailDto>(StatusCodes.Status200OK, "application/json")
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/{refId}/reporting-chain", async (
                [Description("Starting position reference ID.")]
                string refId,
                [FromQuery, Description("Maximum upward reporting levels to traverse (1-50).")]
                int maxDepth = 15,
                [FromQuery, Description("Include inactive positions in the traversed chain.")]
                bool includeInactive = false,
                ISender mediator = default!,
                CancellationToken cancellationToken = default) =>
            {
                var result = await mediator.Send(
                    new GetPositionReportingChainQuery(refId, maxDepth, includeInactive),
                    cancellationToken);
                return Results.Ok(result);
            })
            .WithName("GetPositionReportingChain")
            .WithSummary("Get upward reporting chain")
            .WithDescription(
                "Resolves the ordered chain of command upward from a position for document approval routing.")
            .Produces<IReadOnlyList<PositionNodeDto>>(StatusCodes.Status200OK, "application/json")
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/{refId}/subtree", async (
                [Description("Root manager position reference ID.")]
                string refId,
                [FromQuery, Description("Maximum downward reporting levels to traverse (1-50).")]
                int maxDepth = 10,
                [FromQuery, Description("Include inactive positions in the subtree.")]
                bool includeInactive = false,
                ISender mediator = default!,
                CancellationToken cancellationToken = default) =>
            {
                var result = await mediator.Send(
                    new GetPositionSubtreeQuery(refId, maxDepth, includeInactive),
                    cancellationToken);
                return Results.Ok(result);
            })
            .WithName("GetPositionSubtree")
            .WithSummary("Get subordinate reporting subtree")
            .WithDescription(
                "Resolves all subordinate positions reporting beneath a manager position in breadth-first order.")
            .Produces<IReadOnlyList<PositionNodeDto>>(StatusCodes.Status200OK, "application/json")
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", async (
                [FromBody] CreatePositionCommand command,
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                var refId = await mediator.Send(command, cancellationToken);
                return Results.CreatedAtRoute("GetPosition", new { refId }, refId);
            })
            .WithName("CreatePosition")
            .WithSummary("Create a position")
            .Accepts<CreatePositionCommand>("application/json")
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
                return Results.Ok(await mediator.Send(PositionRequestMapper.ToCommand(request, refId), cancellationToken));
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