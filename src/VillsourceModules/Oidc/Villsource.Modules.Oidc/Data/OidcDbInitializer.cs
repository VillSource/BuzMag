using Finbuckle.MultiTenant.Abstractions;
using FSH.Framework.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenIddict.Abstractions;

namespace Villsource.Modules.Oidc.Data;

public sealed class OidcDbInitializer(
    OidcDbContext dbContext,
    IOpenIddictApplicationManager manager,
    IMultiTenantContextAccessor multiTenantContextAccessor,
    ILogger<OidcDbInitializer> logger) : IDbInitializer
{
    public async Task MigrateAsync(CancellationToken cancellationToken)
    {
        if ((await dbContext.Database.GetPendingMigrationsAsync(cancellationToken).ConfigureAwait(false)).Any())
        {
            await dbContext.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);
            logger.LogInformation("[Oidc] applied migrations");
        }

        var name = $"{multiTenantContextAccessor.MultiTenantContext.TenantInfo?.Identifier?? "None"}-elsa-studio";
        var x = await manager.FindByClientIdAsync(name, cancellationToken).ConfigureAwait(false);
        if (x == null)
        {
            await manager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = name,
                DisplayName = "Elsa Workflows Studio",
                RedirectUris = { new Uri($"https://oauth.usebruno.com/callback"), new Uri($"http://localhost:5000/authentication/login-callback") }, // URL ของ Elsa Studio
                PostLogoutRedirectUris = { new Uri($"https://localhost:5000/signout-callback-oidc") },
                Permissions =
                {
                    OpenIddictConstants.Permissions.Endpoints.Authorization,
                    OpenIddictConstants.Permissions.Endpoints.Token,
                    OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                    OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
                    OpenIddictConstants.Permissions.ResponseTypes.Code,
                    OpenIddictConstants.Permissions.Scopes.Email,
                    OpenIddictConstants.Permissions.Scopes.Profile,
                    OpenIddictConstants.Permissions.Scopes.Roles,
                    "scp:permissions"
                }
            }, cancellationToken);
        }
    }

    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("[Oidc] seeded default organization");
    }
}