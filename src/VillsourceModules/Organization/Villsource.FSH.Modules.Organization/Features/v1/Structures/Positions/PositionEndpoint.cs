using FSH.Framework.Shared.Identity.Authorization;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Villsource.FSH.Modules.Organization.Constants;
using Villsource.FSH.Modules.Organization.Contracts.Authorization;
using Villsource.FSH.Modules.Organization.Contracts.Constants;
using Villsource.FSH.Modules.Organization.Contracts.Dtos;
using Villsource.FSH.Modules.Organization.Contracts.v1.Structures;

namespace Villsource.FSH.Modules.Organization.Features.v1.Structures.Positions;

public static class PositionEndpoint
{
    internal static RouteHandlerBuilder MapGetAllPositionsEndpoint(this IEndpointRouteBuilder endpoints)
        => endpoints.MapGet("/positions",
                async (IMediator mediator, CancellationToken cancellationToken) =>
                {
                    var result = await mediator.Send(new GetAllPositionsQuery(), cancellationToken);
                    return Results.Ok(result);
                })
            .Produces<ICollection<PositionDto>>()
            .WithName("GetAllPositions")
            .WithSummary("Get all positions.")
            .RequirePermission(OrganizationPermissions.Structures.View);

    internal static RouteHandlerBuilder MapGetPositionByIdEndpoint(this IEndpointRouteBuilder endpoints)
        => endpoints.MapGet("/positions/{positionId:length(11)}",
                async (string positionId, IMediator mediator, CancellationToken cancellationToken) =>
                {
                    var result = await mediator.Send(new GetPositionByIdQuery(positionId), cancellationToken);
                    return Results.Ok(result);
                })
            .Produces<PositionDto>()
            .WithName("GetPositionById")
            .WithSummary("Get a position by reference id.")
            .RequirePermission(OrganizationPermissions.Structures.View);

    internal static RouteHandlerBuilder MapCreatePositionEndpoint(this IEndpointRouteBuilder endpoints)
        => endpoints.MapPost("/positions",
                async ([FromBody] CreatePositionCommand command, IMediator mediator,
                    CancellationToken cancellationToken) =>
                {
                    var result = await mediator.Send(command, cancellationToken);
                    return Results.Ok(result);
                })
            .Produces<PositionDto>()
            .WithName("CreatePosition")
            .WithSummary("Creates a new position.")
            .RequirePermission(OrganizationPermissions.Structures.Create);

    public sealed record UpdatePositionBody(string Code, string Name, string? Description);
    internal static RouteHandlerBuilder MapUpdatePositionEndpoint(this IEndpointRouteBuilder endpoints)
        => endpoints.MapPut("/positions/{positionId:length(11)}",
                async (string positionId, [FromBody] UpdatePositionBody body, IMediator mediator,
                    CancellationToken cancellationToken) =>
                {
                    var result = await mediator.Send(new UpdatePositionCommand(
                        PositionId: positionId,
                        Code: body.Code,
                        Name: body.Name,
                        Description: body.Description), cancellationToken);
                    return Results.Ok(result);
                })
            .Produces<PositionDto>()
            .WithName("UpdatePosition")
            .WithSummary("Update a position.")
            .RequirePermission(OrganizationPermissions.Structures.Update);

    internal static RouteHandlerBuilder MapDeletePositionEndpoint(this IEndpointRouteBuilder endpoints)
        => endpoints.MapDelete("/positions/{positionId:length(11)}",
                async (string positionId, IMediator mediator, CancellationToken cancellationToken) =>
                {
                    var result = await mediator.Send(new DeletePositionCommand(Id: positionId), cancellationToken);
                    return Results.Ok(result);
                })
            .Produces<PositionDto>()
            .WithName("DeletePosition")
            .WithSummary("Delete a position.")
            .RequirePermission(OrganizationPermissions.Structures.Delete);

    internal static RouteHandlerBuilder MapGetPositionTiers(this IEndpointRouteBuilder endpoints)
        => endpoints.MapGet("/positions/tiers", () => PositionTier.Items)
            .Produces<IReadOnlyCollection<PositionTier>>()
            .WithName("GetPositionTiers")
            .WithSummary("Get all position tiers.")
            .WithTags(ApiTags.Organization, "lookups")
            .AllowAnonymous();
}
