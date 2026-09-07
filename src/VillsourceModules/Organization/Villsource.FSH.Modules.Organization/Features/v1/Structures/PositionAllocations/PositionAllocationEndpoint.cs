using FSH.Framework.Shared.Identity.Authorization;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Villsource.FSH.Modules.Organization.Contracts.Authorization;
using Villsource.FSH.Modules.Organization.Contracts.Dtos;
using Villsource.FSH.Modules.Organization.Contracts.v1.Structures;

namespace Villsource.FSH.Modules.Organization.Features.v1.Structures.PositionAllocations;

public static class PositionAllocationEndpoint
{
    public sealed record AllocatePositionBody(string PositionId, int? HeadCount);

    public sealed record ChangePositionAllocationBody(int? HeadCount);

    extension(IEndpointRouteBuilder endpoints)
    {
        internal RouteHandlerBuilder
            MapGetCurrentPositionAllocationsEndpoint() => endpoints
            .MapGet("/units/{organizationUnitId:length(11)}/positions",
                async (string organizationUnitId, IMediator mediator, CancellationToken ct) =>
                    Results.Ok(await mediator.Send(new GetCurrentPositionAllocationsQuery(organizationUnitId), ct)))
            .Produces<ICollection<PositionAllocationDto>>()
            .RequirePermission(OrganizationPermissions.Structures.View)
            .WithName("GetCurrentPositionAllocations")
            .WithSummary("Gets the current position allocations for a given organization unit");

        internal RouteHandlerBuilder MapGetPositionAllocationHistoryEndpoint() =>
            endpoints.MapGet("/units/{organizationUnitId:length(11)}/positions/{positionId:length(11)}/history",
                    async (string organizationUnitId, string positionId, IMediator mediator, CancellationToken ct) =>
                        Results.Ok(await mediator.Send(
                            new GetPositionAllocationHistoryQuery(organizationUnitId, positionId),
                            ct)))
                .Produces<ICollection<PositionAllocationDto>>()
                .RequirePermission(OrganizationPermissions.Structures.View)
                .WithName("GetPositionAllocationHistory")
                .WithSummary("Gets the current position allocation history for a given organization unit");

        internal RouteHandlerBuilder MapAllocatePositionEndpoint() => endpoints
            .MapPost("/units/{organizationUnitId:length(11)}/positions",
                async (string organizationUnitId, [FromBody] AllocatePositionBody body, IMediator mediator,
                        CancellationToken ct) =>
                    Results.Ok(await mediator.Send(
                        new AllocatePositionCommand(organizationUnitId, body.PositionId, body.HeadCount), ct)))
            .Produces<PositionAllocationDto>().RequirePermission(OrganizationPermissions.Structures.Create)
            .WithName("AllocatePosition")
            .WithSummary("Allocates the current position allocation for a given organization unit");

        internal RouteHandlerBuilder MapChangePositionAllocationEndpoint() =>
            endpoints.MapPost("/position-allocations/{allocationId:guid}/change",
                    async (Guid allocationId, [FromBody] ChangePositionAllocationBody body, IMediator mediator,
                            CancellationToken ct) =>
                        Results.Ok(await mediator.Send(new ChangePositionAllocationCommand(allocationId, body.HeadCount),
                            ct)))
                .Produces<PositionAllocationDto>()
                .RequirePermission(OrganizationPermissions.Structures.Update)
                .WithName("ChangePositionAllocation")
                .WithSummary("Changes the current position allocation for a given organization unit");

        internal RouteHandlerBuilder MapEndPositionAllocationEndpoint() =>
            endpoints.MapPost("/position-allocations/{allocationId:guid}/end",
                    async (Guid allocationId, IMediator mediator, CancellationToken ct) =>
                        Results.Ok(await mediator.Send(new EndPositionAllocationCommand(allocationId), ct)))
                .Produces<PositionAllocationDto>()
                .RequirePermission(OrganizationPermissions.Structures.Update)
                .WithName("EndPositionAllocation")
                .WithSummary("Ends the current position allocation for a given organization unit");
    }
}