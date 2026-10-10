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
    }

    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        var tenantId = multiTenantContextAccessor.MultiTenantContext.TenantInfo?.Identifier ?? "root";
        var name = $"{tenantId}-elsa-studio";

        var client = await manager.FindByClientIdAsync(name, cancellationToken).ConfigureAwait(false);
        if (client is null)
        {
#pragma warning disable S1075
            var brunoUri = new Uri("https://oauth.usebruno.com/callback");
            var elsaLoginUri = new Uri("http://localhost:5000/authentication/login-callback");
            var elsaSignoutUri = new Uri("http://localhost:5000/signout-callback-oidc");
#pragma warning restore S1075

            await manager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = name,
                DisplayName = "Elsa Workflows Studio",
                RedirectUris =
                {
                    brunoUri,
                    elsaLoginUri
                },
                PostLogoutRedirectUris =
                {
                    elsaSignoutUri
                },
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
                }
            }, cancellationToken).ConfigureAwait(false);

            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("[Oidc] seeded default Elsa Studio application: {ClientId}", name);
            }
        }
    }
}
