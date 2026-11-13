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

namespace Villsource.FSH.Modules.Organization.Features.v1.Positions;

public static class PositionEndpoint
{
    extension(IEndpointRouteBuilder endpoints)
    {
        internal void MapPositionsEndpoints()
        {
            var group = endpoints.MapGroup("positions")
                .WithTags("Organization Positions");

            group.MapGetPositionsEndpoint();
            group.MapGetPositionEndpoint();
            group.MapCreatePositionEndpoint();
            group.MapUpdatePositionEndpoint();
            group.MapDeletePositionEndpoint();
            group.MapGetPositionTiers();
        }

        private void MapGetPositionsEndpoint()
            => endpoints.MapGet("/",
                    async (IMediator mediator, CancellationToken cancellationToken) =>
                    {
                        var result = await mediator.Send(new GetAllPositionsQuery(), cancellationToken);
                        return Results.Ok(result);
                    })
                .WithName("GetPositions")
                .WithSummary("Get all positions")
                .WithDescription("Retrieves a list of all available positions in the organization.")
                .Produces<ICollection<PositionDto>>()
                .RequirePermission(OrganizationPermissions.Units.View);

        private void MapGetPositionEndpoint()
            => endpoints.MapGet("/{positionId:length(11)}",
                    async (string positionId, IMediator mediator, CancellationToken cancellationToken) =>
                    {
                        var result = await mediator.Send(new GetPositionByIdQuery(positionId), cancellationToken);
                        return Results.Ok(result);
                    })
                .WithName("GetPosition")
                .WithSummary("Get position by ID")
                .WithDescription("Retrieves detailed information of a specific position by its 11-character ID.")
                .Produces<PositionDto>()
                .ProducesProblem(StatusCodes.Status404NotFound)
                .RequirePermission(OrganizationPermissions.Units.View);

        private void MapCreatePositionEndpoint()
            => endpoints.MapPost("/",
                    async ([FromBody] CreatePositionCommand command, IMediator mediator,
                        CancellationToken cancellationToken) =>
                    {
                        var result = await mediator.Send(command, cancellationToken);
                        return Results.Ok(result);
                    })
                .WithName("CreatePosition")
                .WithSummary("Create position")
                .WithDescription("Creates a new position in the organization.")
                .Produces<PositionDto>()
                .ProducesValidationProblem()
                .RequirePermission(OrganizationPermissions.Units.Create);

        private void MapUpdatePositionEndpoint()
            => endpoints.MapPut("/{positionId:length(11)}",
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
                .WithName("UpdatePosition")
                .WithSummary("Update position")
                .WithDescription("Updates the code, name, and description of an existing position.")
                .Produces<PositionDto>()
                .ProducesValidationProblem()
                .ProducesProblem(StatusCodes.Status404NotFound)
                .RequirePermission(OrganizationPermissions.Units.Update);

        private void MapDeletePositionEndpoint()
            => endpoints.MapDelete("/{positionId:length(11)}",
                    async (string positionId, IMediator mediator, CancellationToken cancellationToken) =>
                    {
                        var result = await mediator.Send(new DeletePositionCommand(Id: positionId), cancellationToken);
                        return Results.Ok(result);
                    })
                .WithName("DeletePosition")
                .WithSummary("Delete position")
                .WithDescription("Deletes an existing position by its 11-character ID.")
                .Produces<PositionDto>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .RequirePermission(OrganizationPermissions.Units.Delete);

        public void MapGetPositionTiers()
            => endpoints.MapGet("/tiers", () => PositionTier.Items)
                .WithName("GetPositionTiers")
                .WithSummary("Get all position tiers")
                .WithDescription("Retrieves a lookup list of all predefined position tiers.")
                .WithTags("Lookups")
                .Produces<IReadOnlyCollection<PositionTier>>(StatusCodes.Status200OK)
                .AllowAnonymous();
    }

    public sealed record UpdatePositionBody(string Code, string Name, string? Description);
}