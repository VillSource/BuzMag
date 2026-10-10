using System.Security.Claims;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using OpenIddict.Validation.AspNetCore;
using Villsource.Modules.Oidc.Services;

namespace Villsource.Modules.Oidc.Features.v1;

public static class OidcEndpoints
{
    public static IEndpointRouteBuilder MapOidcEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapMethods("/connect/authorize", ["GET", "POST"], async (
            HttpContext context,
            IOidcUserClaimsService claimsService) =>
        {
            var request = context.GetOpenIddictServerRequest()
                          ?? throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

            // Check if user is authenticated via OIDC Cookie session
            var authResult = await context.AuthenticateAsync(OidcConstant.CookieScheme);
            if (!authResult.Succeeded || authResult.Principal is null)
            {
                var returnUrl = $"{context.Request.Path}{context.Request.QueryString}";
                return Results.Redirect($"/api/v1/oidc/login?returnUrl={Uri.EscapeDataString(returnUrl)}");
            }

            var cookieUser = authResult.Principal;
            var subject = cookieUser.FindFirstValue(ClaimTypes.NameIdentifier)
                          ?? cookieUser.FindFirstValue(OpenIddictConstants.Claims.Subject);
            var tenant = cookieUser.FindFirstValue("tenant") ?? "root";

            if (string.IsNullOrEmpty(subject))
            {
                var returnUrl = $"{context.Request.Path}{context.Request.QueryString}";
                return Results.Redirect($"/api/v1/oidc/login?returnUrl={Uri.EscapeDataString(returnUrl)}");
            }

            var identity = await claimsService.BuildOidcIdentityForSubjectAsync(
                subject: subject,
                tenantId: tenant,
                requestedScopes: request.GetScopes(),
                ct: context.RequestAborted);

            if (identity is null)
            {
                await context.SignOutAsync(OidcConstant.CookieScheme);
                var returnUrl = $"{context.Request.Path}{context.Request.QueryString}";
                return Results.Redirect($"/api/v1/oidc/login?returnUrl={Uri.EscapeDataString(returnUrl)}");
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
                var result = await context.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
                if (result.Principal is null)
                {
                    return Results.BadRequest(new
                    {
                        error = "invalid_grant",
                        error_description = "The token is invalid or expired."
                    });
                }

                return Results.SignIn(result.Principal,
                    authenticationScheme: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            return Results.BadRequest(new
            {
                error = "unsupported_grant_type",
                error_description = "Only authorization_code and refresh_token grant types are supported."
            });
        })
        .ExcludeFromDescription()
        .AllowAnonymous();

        endpoints.MapMethods("/connect/userinfo", ["GET", "POST"], async (
            HttpContext context,
            IOidcUserClaimsService claimsService) =>
        {
            var result = await context.AuthenticateAsync(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
            if (!result.Succeeded || result.Principal is null)
            {
                return Results.Challenge(
                    authenticationSchemes: [OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme]);
            }

            var subject = result.Principal.FindFirstValue(OpenIddictConstants.Claims.Subject)
                          ?? result.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
            var tenant = result.Principal.FindFirstValue("tenant") ?? "root";

            if (string.IsNullOrEmpty(subject))
            {
                return Results.NotFound();
            }

            var identity = await claimsService.BuildOidcIdentityForSubjectAsync(
                subject: subject,
                tenantId: tenant,
                requestedScopes: result.Principal.GetScopes(),
                ct: context.RequestAborted);

            if (identity is null)
            {
                return Results.NotFound();
            }

            var claims = new Dictionary<string, object>(StringComparer.Ordinal)
            {
                [OpenIddictConstants.Claims.Subject] = subject
            };

            var name = identity.FindFirst(OpenIddictConstants.Claims.Name)?.Value;
            if (!string.IsNullOrEmpty(name))
            {
                claims[OpenIddictConstants.Claims.Name] = name;
            }

            var email = identity.FindFirst(OpenIddictConstants.Claims.Email)?.Value;
            if (!string.IsNullOrEmpty(email))
            {
                claims[OpenIddictConstants.Claims.Email] = email;
                claims[OpenIddictConstants.Claims.EmailVerified] = true;
            }

            var roles = identity.FindAll(OpenIddictConstants.Claims.Role).Select(c => c.Value).ToArray();
            if (roles.Length > 0)
            {
                claims[OpenIddictConstants.Claims.Role] = roles;
            }

            claims["tenant"] = tenant;

            return Results.Ok(claims);
        })
        .ExcludeFromDescription()
        .AllowAnonymous();

        return endpoints;
    }
}
