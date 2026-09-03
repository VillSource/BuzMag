using FSH.Framework.Shared.Identity.Authorization;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Villsource.FSH.Modules.Organization.Contracts.Authorization;
using Villsource.FSH.Modules.Organization.Contracts.Dtos;
using Villsource.FSH.Modules.Organization.Contracts.v1.Structures.PositionAllocations;

namespace Villsource.FSH.Modules.Organization.Features.v1.Structures.PositionAllocations;

public static class PositionAllocationEndpoint
{
    public sealed record AllocatePositionBody(string PositionId, int? HeadCount);

    public sealed record ChangePositionAllocationBody(int? HeadCount);

    internal static RouteHandlerBuilder
        MapGetCurrentPositionAllocationsEndpoint(this IEndpointRouteBuilder endpoints) => endpoints
        .MapGet("/units/{organizationUnitId:length(11)}/position-allocations",
            async (string organizationUnitId, IMediator mediator, CancellationToken ct) =>
                Results.Ok(await mediator.Send(new GetCurrentPositionAllocationsQuery(organizationUnitId), ct)))
        .Produces<ICollection<OrganizationUnitPositionAllocationDto>>()
        .RequirePermission(OrganizationPermissions.Structures.View)
        .WithName("GetCurrentPositionAllocations")
        .WithSummary("Gets the current position allocations for a given organization unit");

    internal static RouteHandlerBuilder MapGetPositionAllocationHistoryEndpoint(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapGet("/units/{organizationUnitId:length(11)}/positions/{positionId:length(11)}/allocation-history",
                async (string organizationUnitId, string positionId, IMediator mediator, CancellationToken ct) =>
                    Results.Ok(await mediator.Send(
                        new GetPositionAllocationHistoryQuery(organizationUnitId, positionId),
                        ct))).Produces<ICollection<OrganizationUnitPositionAllocationDto>>()
            .RequirePermission(OrganizationPermissions.Structures.View)
            .WithName("GetPositionAllocationHistory")
            .WithSummary("Gets the current position allocation history for a given organization unit");

    internal static RouteHandlerBuilder MapAllocatePositionEndpoint(this IEndpointRouteBuilder endpoints) => endpoints
        .MapPost("/units/{organizationUnitId:length(11)}/position-allocations",
            async (string organizationUnitId, [FromBody] AllocatePositionBody body, IMediator mediator,
                    CancellationToken ct) =>
                Results.Ok(await mediator.Send(
                    new AllocatePositionCommand(organizationUnitId, body.PositionId, body.HeadCount), ct)))
        .Produces<OrganizationUnitPositionAllocationDto>().RequirePermission(OrganizationPermissions.Structures.Create)
        .WithName("AllocatePosition")
        .WithSummary("Allocates the current position allocation for a given organization unit");

    internal static RouteHandlerBuilder MapChangePositionAllocationEndpoint(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapPost("/position-allocations/{allocationId:guid}/change",
                async (Guid allocationId, [FromBody] ChangePositionAllocationBody body, IMediator mediator,
                        CancellationToken ct) =>
                    Results.Ok(await mediator.Send(new ChangePositionAllocationCommand(allocationId, body.HeadCount),
                        ct)))
            .Produces<OrganizationUnitPositionAllocationDto>()
            .RequirePermission(OrganizationPermissions.Structures.Update)
            .WithName("ChangePositionAllocation")
            .WithSummary("Changes the current position allocation for a given organization unit");

    internal static RouteHandlerBuilder MapEndPositionAllocationEndpoint(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapPost("/position-allocations/{allocationId:guid}/end",
                async (Guid allocationId, IMediator mediator, CancellationToken ct) =>
                    Results.Ok(await mediator.Send(new EndPositionAllocationCommand(allocationId), ct)))
            .Produces<OrganizationUnitPositionAllocationDto>()
            .RequirePermission(OrganizationPermissions.Structures.Update)
            .WithName("EndPositionAllocation")
            .WithSummary("Ends the current position allocation for a given organization unit");
}