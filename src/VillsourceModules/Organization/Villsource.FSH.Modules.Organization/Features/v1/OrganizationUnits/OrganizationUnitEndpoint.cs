using FSH.Framework.Shared.Identity.Authorization;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Villsource.FSH.Modules.Organization.Contracts.Authorization;
using Villsource.FSH.Modules.Organization.Contracts.Dtos;
using Villsource.FSH.Modules.Organization.Contracts.v1.Structures;

namespace Villsource.FSH.Modules.Organization.Features.v1.OrganizationUnits;

public static class OrganizationUnitEndpoint
{
    extension(IEndpointRouteBuilder endpoints)
    {
        internal void MapUnitEndpoints()
        {
            var group = endpoints.MapGroup("/units")
                .WithTags("Organization Units");

            group.MapGetUnitsEndpoint();
            group.MapCreateUnitEndpoint();
            group.MapDeleteUnitEndpoint();
            group.MapUpdateUnitEndpoint();
            group.MapMoveUnitEndpoint();
        }

        private void MapGetUnitsEndpoint()
            => endpoints.MapGet("/",
                    async (IMediator mediator, CancellationToken cancellationToken) =>
                    {
                        var result = await mediator.Send(new GetAllOrganizationUnitsQuery(), cancellationToken);
                        return Results.Ok(result);
                    })
                .WithName("GetUnits")
                .WithSummary("Get all organization units.")
                .WithDescription("Retrieves a list of all organization units in the hierarchy.")
                .Produces<ICollection<OrganizationUnitDto>>()
                .RequirePermission(OrganizationPermissions.Units.View);

        private void MapCreateUnitEndpoint()
            => endpoints.MapPost("/",
                    async ([FromBody] CreateOrganizationUnitCommand command, IMediator mediator,
                        CancellationToken cancellationToken) =>
                    {
                        var result = await mediator.Send(command, cancellationToken);
                        return Results.Ok(result);
                    })
                .WithName("CreateUnit")
                .WithSummary("Create organization unit")
                .WithDescription("Creates a new organization unit under a specified parent or as a root unit.")
                .ProducesValidationProblem()
                .Produces<OrganizationUnitDto>()
                .RequirePermission(OrganizationPermissions.Units.Create);

        private void MapDeleteUnitEndpoint()
            => endpoints.MapDelete("/{unitRef:length(11)}",
                    async (string unitRef, IMediator mediator, CancellationToken cancellationToken) =>
                    {
                        var result = await mediator.Send(new DeleteOrganizationUnitCommand(UnitRef: unitRef),
                            cancellationToken);
                        return Results.Ok(result);
                    })
                .WithName("DeleteUnit")
                .WithSummary("Delete organization unit")
                .WithDescription("Deletes an existing organization unit by its 11-character ID.")
                .ProducesValidationProblem()
                .Produces<OrganizationUnitDto>()
                .ProducesProblem(StatusCodes.Status404NotFound)
                .RequirePermission(OrganizationPermissions.Units.Delete);

        private void MapUpdateUnitEndpoint()
            => endpoints.MapPut("/{unitRef:length(11)}",
                    async (string unitRef, [FromBody] UpdateOrganizationUnitBody body, IMediator mediator,
                        CancellationToken cancellationToken) =>
                    {
                        var result = await mediator.Send(new UpdateOrganizationUnitCommand(
                            UnitRef: unitRef,
                            Code: body.Code,
                            Name: body.Name,
                            Description: body.Description), cancellationToken);
                        return Results.Ok(result);
                    })
                .WithName("UpdateUnit")
                .WithSummary("Update organization unit")
                .WithDescription("Updates the code, name, and description of an existing organization unit.")
                .ProducesValidationProblem()
                .Produces<OrganizationUnitDto>()
                .ProducesProblem(StatusCodes.Status404NotFound)
                .RequirePermission(OrganizationPermissions.Units.Update);

        private void MapMoveUnitEndpoint()
            => endpoints.MapPatch("/{unitRef:length(11)}/move",
                    async (string organizationUnitId, [FromBody] MoveOrganizationUnitBody body, IMediator mediator,
                        CancellationToken cancellationToken) =>
                    {
                        var result = await mediator.Send(new MoveOrganizationUnitCommand(
                            UnitRef: organizationUnitId,
                            NewParentRef: body.NewParentRef), cancellationToken);
                        return Results.Ok(result);
                    })
                .WithName("MoveUnit")
                .WithSummary("Move organization unit")
                .WithDescription("Moves an organization unit to a new parent unit, or sets it as root if NewParentId is null.")
                .ProducesValidationProblem()
                .Produces<OrganizationUnitDto>()
                .ProducesProblem(StatusCodes.Status404NotFound)
                .RequirePermission(OrganizationPermissions.Units.Move);
    }


    public sealed record UpdateOrganizationUnitBody(string Code, string Name, string? Description);

    public sealed record MoveOrganizationUnitBody(string? NewParentRef);
}