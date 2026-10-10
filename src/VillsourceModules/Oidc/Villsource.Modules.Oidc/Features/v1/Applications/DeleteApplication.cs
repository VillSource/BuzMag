using FSH.Framework.Shared.Identity.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using OpenIddict.Abstractions;
using Villsource.Modules.Oidc.Contracts.Authorization;

namespace Villsource.Modules.Oidc.Features.v1.Applications;

public static class DeleteApplicationEndpoint
{
    public static IEndpointRouteBuilder MapDeleteApplicationEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapDelete("/{id}", async (
            string id,
            IOpenIddictApplicationManager manager,
            CancellationToken ct) =>
        {
            var app = await manager.FindByIdAsync(id, ct).ConfigureAwait(false);
            if (app is null)
            {
                return Results.NotFound();
            }

            await manager.DeleteAsync(app, ct).ConfigureAwait(false);
            return Results.NoContent();
        })
        .WithSummary("Delete an OIDC application")
        .RequirePermission(OidcPermissions.Applications.Delete);

        return endpoints;
    }
}
