using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi;

namespace Villsource.Modules.Oidc;

public sealed class OidcSecurityTransformer(IConfiguration configuration) : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(context);

        document.Components ??= new OpenApiComponents();

        var oidcUrl = configuration["Oidc:DiscoveryUrl"]
                      ?? $"https://localhost:7030/.well-known/openid-configuration";

        document.Components?.SecuritySchemes?["OIDC"] =
            new OpenApiSecurityScheme { Type = SecuritySchemeType.OpenIdConnect, OpenIdConnectUrl = new Uri(oidcUrl), };

        return Task.CompletedTask;
    }
}