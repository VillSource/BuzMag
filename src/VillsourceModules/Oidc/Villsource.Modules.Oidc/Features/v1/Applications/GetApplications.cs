using FSH.Framework.Shared.Identity.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using OpenIddict.Abstractions;
using Villsource.Modules.Oidc.Contracts.Authorization;
using Villsource.Modules.Oidc.Contracts.v1.Applications;

namespace Villsource.Modules.Oidc.Features.v1.Applications;

public static class GetApplicationsEndpoint
{
    public static IEndpointRouteBuilder MapGetApplicationsEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/", async (
            IOpenIddictApplicationManager manager,
            CancellationToken ct) =>
        {
            var apps = new List<ApplicationDto>();

            await foreach (var app in manager.ListAsync(cancellationToken: ct).ConfigureAwait(false))
            {
                apps.Add(await ToDto(manager, app, ct).ConfigureAwait(false));
            }

            return Results.Ok(apps);
        })
        .WithSummary("List all registered OIDC applications")
        .RequirePermission(OidcPermissions.Applications.View);

        return endpoints;
    }

    internal static async Task<ApplicationDto> ToDto(IOpenIddictApplicationManager manager, object app, CancellationToken ct)
    {
        var id = await manager.GetIdAsync(app, ct).ConfigureAwait(false) ?? string.Empty;
        var clientId = await manager.GetClientIdAsync(app, ct).ConfigureAwait(false) ?? string.Empty;
        var displayName = await manager.GetDisplayNameAsync(app, ct).ConfigureAwait(false);
        var clientType = await manager.GetClientTypeAsync(app, ct).ConfigureAwait(false) ?? string.Empty;
        var redirectUris = (await manager.GetRedirectUrisAsync(app, ct).ConfigureAwait(false)).ToList();
        var postLogoutUris = (await manager.GetPostLogoutRedirectUrisAsync(app, ct).ConfigureAwait(false)).ToList();
        var permissions = (await manager.GetPermissionsAsync(app, ct).ConfigureAwait(false)).ToList();

        return new ApplicationDto(id, clientId, displayName, clientType, redirectUris, postLogoutUris, permissions);
    }
}
