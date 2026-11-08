using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Villsource.Modules.Oidc.Data;
using Villsource.Modules.Oidc.Data.Entities;

namespace Villsource.Modules.Oidc;

public static class OpenIddictConfig
{
    public static IServiceCollection AddOidcProvider(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(environment);

        services.AddOptions<OidcIdentityTokenOptions>()
            .Bind(configuration.GetSection(OidcIdentityTokenOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var tokenOptions = configuration
            .GetSection(OidcIdentityTokenOptions.SectionName)
            .Get<OidcIdentityTokenOptions>()
            ?? throw new InvalidOperationException(
                $"The {OidcIdentityTokenOptions.SectionName} configuration section is required by the OIDC module.");

        if (!Uri.TryCreate(tokenOptions.Issuer, UriKind.Absolute, out Uri? issuerUri) ||
            (issuerUri.Scheme != Uri.UriSchemeHttps && issuerUri.Scheme != Uri.UriSchemeHttp) ||
            !string.Equals(tokenOptions.Issuer, issuerUri.AbsoluteUri, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"{OidcIdentityTokenOptions.SectionName}:Issuer must be a canonical absolute HTTP or HTTPS URL, including its trailing slash.");
        }

        OidcServerOptions serverOptions = configuration
            .GetSection(OidcServerOptions.SectionName)
            .Get<OidcServerOptions>()
            ?? new OidcServerOptions();

        X509Certificate2? signingCertificate = null;
        if (!string.IsNullOrWhiteSpace(serverOptions.SigningCertificatePath))
        {
            signingCertificate = X509CertificateLoader.LoadPkcs12FromFile(
                serverOptions.SigningCertificatePath,
                serverOptions.SigningCertificatePassword,
                X509KeyStorageFlags.EphemeralKeySet);

            if (!signingCertificate.HasPrivateKey ||
                signingCertificate.NotBefore > DateTime.UtcNow ||
                signingCertificate.NotAfter <= DateTime.UtcNow)
            {
                throw new InvalidOperationException(
                    $"{OidcServerOptions.SectionName}:SigningCertificatePath must point to a currently valid certificate with a private key.");
            }
        }
        else if (!environment.IsDevelopment() && !serverOptions.UseDevelopmentCertificates)
        {
            throw new InvalidOperationException(
                $"{OidcServerOptions.SectionName}:SigningCertificatePath must be configured outside Development.");
        }

        services.AddOpenIddict()
            .AddCore(options =>
            {
                options.UseEntityFrameworkCore()
                    .UseDbContext<OidcDbContext>()
                    .ReplaceDefaultEntities<OidcApplication, OidcAuthorization, OidcScope, OidcToken, string>();
            })
            .AddServer(options =>
            {
                options.SetIssuer(issuerUri)
                    .SetAuthorizationEndpointUris("/connect/authorize")
                    .SetTokenEndpointUris("/connect/token")
                    .SetUserInfoEndpointUris("/connect/userinfo")
                    .SetConfigurationEndpointUris("/.well-known/openid-configuration")
                    .SetJsonWebKeySetEndpointUris("/.well-known/jwks");

                options.AllowAuthorizationCodeFlow()
                    .RequireProofKeyForCodeExchange();

                options.AllowRefreshTokenFlow();

                options.RegisterScopes("openid", "profile", "email", "roles");

                // Access tokens use Identity's HMAC key; keep this first. OpenIddict uses an asymmetric key for ID tokens.
                if (signingCertificate is null)
                {
                    options.AddDevelopmentEncryptionCertificate()
                        .AddDevelopmentSigningCertificate();
                }
                else
                {
                    options.AddEncryptionCertificate(signingCertificate)
                        .AddSigningCertificate(signingCertificate);
                }

                options.DisableAccessTokenEncryption();
                options.SetAccessTokenLifetime(TimeSpan.FromMinutes(tokenOptions.AccessTokenMinutes));
                options.SetRefreshTokenLifetime(TimeSpan.FromDays(tokenOptions.RefreshTokenDays));

                options.UseAspNetCore()
                    .EnableAuthorizationEndpointPassthrough()
                    .EnableTokenEndpointPassthrough()
                    .EnableUserInfoEndpointPassthrough();
            })
            .AddValidation(options =>
            {
                options.UseLocalServer();
                options.UseAspNetCore();
            });

        return services;
    }
}
