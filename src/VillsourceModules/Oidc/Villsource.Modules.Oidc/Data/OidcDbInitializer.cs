using Finbuckle.MultiTenant.Abstractions;
using FSH.Framework.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenIddict.Abstractions;

namespace Villsource.Modules.Oidc.Data;

public sealed partial class OidcDbInitializer(
    OidcDbContext dbContext,
    IOpenIddictApplicationManager manager,
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
        string[] defaultPermissions =
        [
            OpenIddictConstants.Permissions.Endpoints.Authorization,
            OpenIddictConstants.Permissions.Endpoints.Token,
            OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
            OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
            OpenIddictConstants.Permissions.ResponseTypes.Code,
            OpenIddictConstants.Permissions.Scopes.Email,
            OpenIddictConstants.Permissions.Scopes.Profile,
            OpenIddictConstants.Permissions.Scopes.Roles
        ];

        var applications = new[]
        {
            new OpenIddictApplicationDescriptor
            {
                ClientId = "buzmag",
                DisplayName = "BuzMag ERP",
                ApplicationType = OpenIddictConstants.ApplicationTypes.Web,
                ClientType = OpenIddictConstants.ClientTypes.Public,
                RedirectUris =
                {
                    new Uri($"http://127.0.0.1:5173/"),
                    new Uri($"http://localhost:5173/"),
                    
                    new Uri($"http://127.0.0.1:5174/"),
                    new Uri($"http://localhost:5174/"),
                    
                    new Uri($"https://127.0.0.1:5173/"),
                    new Uri($"https://localhost:5173/"),
                    
                    new Uri($"https://127.0.0.1:5174/"),
                    new Uri($"https://localhost:5174/"),
                },
                PostLogoutRedirectUris = {
                    new Uri($"http://127.0.0.1:5173"),
                    new Uri($"http://localhost:5173"),
                    
                    new Uri($"http://127.0.0.1:5174"),
                    new Uri($"http://localhost:5174"),
                },
            },
            new OpenIddictApplicationDescriptor
            {
                ClientId = "scalar",
                DisplayName = "Scalar Dev tool",
                ApplicationType = OpenIddictConstants.ApplicationTypes.Web,
                ClientType = OpenIddictConstants.ClientTypes.Public,
                RedirectUris = { new Uri($"https://localhost:7030/scalar/") },
                PostLogoutRedirectUris = { new Uri($"https://localhost:7030/scalar/") }
            },
            new OpenIddictApplicationDescriptor
            {
                ClientId = "bruno",
                DisplayName = "Bruno Dev tool",
                ApplicationType = OpenIddictConstants.ApplicationTypes.Web,
                ClientType = OpenIddictConstants.ClientTypes.Public,
                RedirectUris = { new Uri($"https://oauth.usebruno.com/callback") },
                PostLogoutRedirectUris = { new Uri($"http://localhost:5000") }
            },
            new OpenIddictApplicationDescriptor
            {
                ClientId = "elsastudio",
                DisplayName = "Elsa Studio Dev tool",
                ApplicationType = OpenIddictConstants.ApplicationTypes.Web,
                ClientType = OpenIddictConstants.ClientTypes.Public,
                RedirectUris =
                {
                    new Uri($"http://localhost:5000/authentication/login-callback"),
                    new Uri($"http://localhost:5100/signin-oidc"),
                    new Uri($"https://localhost:5101/signin-oidc"),
                },
                PostLogoutRedirectUris = 
                {
                    new Uri($"http://localhost:5000"),
                    new Uri($"http://localhost:5100"),
                    new Uri($"https://localhost:5101"),
                },
            }
        };

        foreach (var descriptor in applications)
        {
            foreach (var permission in defaultPermissions)
            {
                descriptor.Permissions.Add(permission);
            }

            var existingClient = await manager.FindByClientIdAsync(descriptor.ClientId!, cancellationToken)
                .ConfigureAwait(false);

            if (existingClient is null)
            {
                await manager.CreateAsync(descriptor, cancellationToken).ConfigureAwait(false);
                LogOidcSeededClientId(descriptor.ClientId!);
            }
            else
            {
                await manager.UpdateAsync(existingClient, descriptor, cancellationToken).ConfigureAwait(false);
                LogOidcUpdatedClientId(descriptor.ClientId!);
            }
        }
    }

    [LoggerMessage(LogLevel.Information, "[Oidc] seeded default application: {ClientId}")]
    partial void LogOidcSeededClientId(string clientId);

    [LoggerMessage(LogLevel.Information, "[Oidc] updated existing application: {ClientId}")]
    partial void LogOidcUpdatedClientId(string clientId);
}