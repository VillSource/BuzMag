using FSH.Framework.Shared.Identity.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using OpenIddict.Abstractions;
using Villsource.Modules.Oidc.Contracts.Authorization;

namespace Villsource.Modules.Oidc.Features.v1.Applications;

public static class GetApplicationByIdEndpoint
{
    public static IEndpointRouteBuilder MapGetApplicationByIdEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/{id}", async (
            string id,
            IOpenIddictApplicationManager manager,
            CancellationToken ct) =>
        {
            var app = await manager.FindByIdAsync(id, ct).ConfigureAwait(false);
            if (app is null)
            {
                return Results.NotFound();
            }

            var dto = await GetApplicationsEndpoint.ToDto(manager, app, ct).ConfigureAwait(false);
            return Results.Ok(dto);
        })
        .WithSummary("Get a single OIDC application by its ID")
        .RequirePermission(OidcPermissions.Applications.View);

        return endpoints;
    }
}
