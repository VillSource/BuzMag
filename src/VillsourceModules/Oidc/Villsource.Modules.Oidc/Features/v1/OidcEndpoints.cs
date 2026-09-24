using FSH.Modules.Identity.Contracts.Services;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;

// หรือ Namespace ของ Identity Module ใน FSH

namespace Villsource.Modules.Oidc.Features.v1;

public static class OidcEndpoints
{
    public static IEndpointRouteBuilder MapOidcEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapMethods("/connect/authorize", ["GET", "POST"], async (
                HttpContext context
                ) => // Inject IUserService ของ FSH เดิม
            {
                var request = context.GetOpenIddictServerRequest()
                              ?? throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

                var identity = new ClaimsIdentity(
                    authenticationType: TokenValidationParameters.DefaultAuthenticationType,
                    nameType: OpenIddictConstants.Claims.Name,
                    roleType: OpenIddictConstants.Claims.Role);

                identity.AddClaim(OpenIddictConstants.Claims.Subject, Guid.Empty);
                identity.AddClaim(ClaimTypes.NameIdentifier, Guid.Empty);
                identity.AddClaim("tenant", "mock");
                identity.AddClaim(OpenIddictConstants.Claims.Name, "Mr. Mock");
                identity.AddClaim(OpenIddictConstants.Claims.Email, "mr_mock@gmail.com");

                identity.AddClaim(OpenIddictConstants.Claims.Role, "tester");

                identity.AddClaim("permissions", "*");

                foreach (var claim in identity.Claims)
                {
                    claim.SetDestinations(OpenIddictConstants.Destinations.AccessToken,
                        OpenIddictConstants.Destinations.IdentityToken);
                }

                var principal = new ClaimsPrincipal(identity);
                principal.SetScopes(request.GetScopes());

                return Results.SignIn(principal,
                    authenticationScheme: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            })
            .ExcludeFromDescription()
            .AllowAnonymous();


        endpoints.MapPost("/connect/token", async (HttpContext context) =>
            {
                var request = context.GetOpenIddictServerRequest()
                              ?? throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

                if (request.IsAuthorizationCodeGrantType() || request.IsRefreshTokenGrantType())
                {
                    var result =
                        await context.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

                    return Results.SignIn(result.Principal!,
                        authenticationScheme: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
                }

                return Results.BadRequest(new
                {
                    error = "unsupported_grant_type",
                    error_description = "The specified grant type is not supported."
                });
            })
            .ExcludeFromDescription()
            .AllowAnonymous();

        return endpoints;
    }
}