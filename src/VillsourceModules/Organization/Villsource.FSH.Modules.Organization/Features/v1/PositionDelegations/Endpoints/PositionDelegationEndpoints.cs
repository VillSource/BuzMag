using System.ComponentModel;
using FSH.Framework.Shared.Persistence;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Villsource.FSH.Modules.Organization.Contracts.v1.PositionDelegations.Commands;
using Villsource.FSH.Modules.Organization.Contracts.v1.PositionDelegations.Queries;

namespace Villsource.FSH.Modules.Organization.Features.v1.PositionDelegations.Endpoints;

public static class PositionDelegationEndpoints
{
    public static IEndpointRouteBuilder MapPositionDelegationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/position-delegations")
            .WithTags("Position Delegations")
            .RequireAuthorization()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapGet("/", async (
                [AsParameters] SearchPositionDelegationsRequest request,
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                var result = await mediator.Send(request.ToQuery(), cancellationToken);
                return Results.Ok(result.ToResponse());
            })
            .WithName("SearchPositionDelegations")
            .WithSummary("Search position delegations")
            .WithDescription("Searches and paginates temporary acting and approval authority delegations.")
            .Produces<PagedResponse<PositionDelegationSummaryResponse>>(StatusCodes.Status200OK, "application/json")
            .ProducesValidationProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/types", async (
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                var result = await mediator.Send(new GetDelegationTypesQuery(), cancellationToken);
                return Results.Ok(result.ToResponse());
            })
            .WithName("GetDelegationTypes")
            .WithSummary("List delegation types")
            .Produces<IReadOnlyList<DelegationTypeResponse>>(StatusCodes.Status200OK, "application/json");

        group.MapGet("/effective", async (
                [AsParameters] ResolveEffectiveDelegationsRequest request,
                ISender mediator = default!,
                CancellationToken cancellationToken = default) =>
            {
                var result = await mediator.Send(request.ToQuery(), cancellationToken);
                return Results.Ok(result.ToResponse());
            })
            .WithName("ResolveEffectiveDelegations")
            .WithSummary("Resolve effective delegations for a position")
            .WithDescription("Returns active acting/approval delegations effective at the given timestamp and document type.")
            .Produces<IReadOnlyList<EffectiveDelegationResponse>>(StatusCodes.Status200OK, "application/json")
            .ProducesValidationProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/{refId}", async (
                [Description("Unique reference ID of the position delegation.")] string refId,
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                var result = await mediator.Send(new GetPositionDelegationByRefIdQuery(refId), cancellationToken);
                return Results.Ok(result.ToResponse());
            })
            .WithName("GetPositionDelegation")
            .WithSummary("Get position delegation by reference ID")
            .Produces<PositionDelegationDetailResponse>(StatusCodes.Status200OK, "application/json")
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", async (
                [FromBody] CreatePositionDelegationRequest request,
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                var refId = await mediator.Send(request.ToCommand(), cancellationToken);
                return Results.CreatedAtRoute("GetPositionDelegation", new { refId }, refId);
            })
            .WithName("CreatePositionDelegation")
            .WithSummary("Create a position delegation")
            .Accepts<CreatePositionDelegationRequest>("application/json")
            .Produces<string>(StatusCodes.Status201Created, "application/json")
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPut("/{refId}", async (
                [Description("Unique reference ID of the position delegation.")] string refId,
                [FromBody] UpdatePositionDelegationRequest request,
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                return Results.Ok(await mediator.Send(request.ToCommand(refId), cancellationToken));
            })
            .WithName("UpdatePositionDelegation")
            .WithSummary("Update a position delegation")
            .Accepts<UpdatePositionDelegationRequest>("application/json")
            .Produces<string>(StatusCodes.Status200OK, "application/json")
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPatch("/{refId}/revoke", async (
                [Description("Unique reference ID of the position delegation.")] string refId,
                [FromBody] RevokePositionDelegationRequest? request,
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                var command = (request ?? new RevokePositionDelegationRequest()).ToCommand(refId);
                return Results.Ok(await mediator.Send(command, cancellationToken));
            })
            .WithName("RevokePositionDelegation")
            .WithSummary("Revoke a position delegation")
            .Produces<string>(StatusCodes.Status200OK, "application/json")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{refId}", async (
                [Description("Unique reference ID of the position delegation.")] string refId,
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                return Results.Ok(await mediator.Send(new DeletePositionDelegationCommand(refId), cancellationToken));
            })
            .WithName("DeletePositionDelegation")
            .WithSummary("Delete a position delegation")
            .Produces<string>(StatusCodes.Status200OK, "application/json")
            .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }
}
