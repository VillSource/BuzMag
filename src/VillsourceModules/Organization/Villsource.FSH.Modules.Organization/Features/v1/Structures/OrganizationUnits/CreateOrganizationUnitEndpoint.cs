using FSH.Framework.Shared.Identity.Authorization;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Villsource.FSH.Modules.Organization.Contracts.Authorization;
using Villsource.FSH.Modules.Organization.Contracts.Dtos;
using Villsource.FSH.Modules.Organization.Contracts.v1.Structures;

namespace Villsource.FSH.Modules.Organization.Features.v1.Structures.OrganizationUnits;

public static class CreateOrganizationUnitEndpoint
{
    internal static RouteHandlerBuilder MapCreateOrganizationUnitEndpoint(this IEndpointRouteBuilder endpoints)
        => endpoints.MapPost("/units",
                async ([FromBody] CreateOrganizationUnitCommand command, IMediator mediator, CancellationToken cancellationToken) =>
                {
                    var result = await mediator.Send(command, cancellationToken);
                    return Results.Ok(result);
                })
            .Produces<OrganizationUnitDto>()
            .WithName("CreateOrganizationUnit")
            .WithSummary("Creates a new organization unit.")
            .RequirePermission(OrganizationPermissions.Structures.Create);
}