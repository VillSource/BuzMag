using FSH.Framework.Shared.Identity.Authorization;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Villsource.FSH.Modules.Organization.Contracts.Authorization;
using Villsource.FSH.Modules.Organization.Contracts.Dtos;
using Villsource.FSH.Modules.Organization.Contracts.v1.Structures;

namespace Villsource.FSH.Modules.Organization.Features.v1.Structures.OrganizationUnits;

public static class OrganizationEndpoint
{
    internal static RouteHandlerBuilder MapGetAllOrganizationEndpoint(this IEndpointRouteBuilder endpoints)
        => endpoints.MapGet("/",
                async (IMediator mediator, CancellationToken cancellationToken) =>
                {
                    var result = await mediator.Send(new GetAllOrganizationsQuery(), cancellationToken);
                    return Results.Ok(result);
                })
            .Produces<ICollection<OrganizationUnitDto>>()
            .WithName("GetAllOrganizations")
            .WithSummary("Get all organizations.")
            .RequirePermission(OrganizationPermissions.Structures.View);

}