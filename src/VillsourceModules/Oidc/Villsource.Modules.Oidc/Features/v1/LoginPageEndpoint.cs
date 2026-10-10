using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System.ComponentModel;
using System.Net;
using System.Security.Claims;
using Villsource.Modules.Oidc.Services;

namespace Villsource.Modules.Oidc.Features.v1;

public static class LoginPageEndpoint
{
    public record LF(string Email, string Password);

    public static IEndpointRouteBuilder MapLoginPageEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/v1/oidc/is-authenticated", ()=>Results.Ok())
            .RequireAuthorization(new AuthorizeAttribute { AuthenticationSchemes = OidcConstant.CookieScheme });
        
        endpoints.MapPost("/api/v1/oidc/logout", async (HttpContext context) =>
        {
            await context.SignOutAsync(OidcConstant.CookieScheme);
            return Results.Ok();
        }).RequireAuthorization(new AuthorizeAttribute { AuthenticationSchemes = OidcConstant.CookieScheme });

        endpoints.MapGet("/api/v1/oidc/claims", async (HttpContext context) =>
        {
            return Results.Ok(context.User.Claims.ToDictionary(i=>i.Type, i=>i.Value));
        }).RequireAuthorization(new AuthorizeAttribute { AuthenticationSchemes = OidcConstant.CookieScheme });

        
        endpoints.MapPost("/connect/login", async (
                HttpContext context,
                IOidcUserClaimsService claimsService,
                [DefaultValue("root")][FromHeader] string tenant,
                [FromBody] LF data) =>
            {
                var email = data.Email;
                var password = data.Password;
                
                var identity = await claimsService.BuildOidcIdentityAsync(
                    tenantId: tenant,
                    email: email,
                    password: password,
                    twoFactorCode: null,
                    requestedScopes: ["openid", "profile", "email", "roles"],
                    ct: context.RequestAborted);

                if (identity is null)
                {
                    var errorQuery = Uri.EscapeDataString("Invalid credentials or deactivated account.");
                    return Results.BadRequest(new { error = errorQuery });
                }

                var cookieIdentity = new ClaimsIdentity(identity.Claims, OidcConstant.CookieScheme);
                await context.SignInAsync(OidcConstant.CookieScheme, new ClaimsPrincipal(cookieIdentity));

                return Results.Ok();
            })
            .DisableAntiforgery()
            .ExcludeFromDescription()
            .AllowAnonymous();

        endpoints.MapPost("/connect/loginx", async (
                HttpContext context,
                IOidcUserClaimsService claimsService,
                [DefaultValue("root")] [FromHeader] string tenant,
                [FromBody] LF data) =>
            {
                var form = await context.Request.ReadFormAsync();
                var email = data.Email;
                var password = data.Password;
                var returnUrl = form["returnUrl"].ToString();

                if (string.IsNullOrWhiteSpace(returnUrl))
                {
                    returnUrl = "/";
                }

                var identity = await claimsService.BuildOidcIdentityAsync(
                    tenantId: tenant,
                    email: email,
                    password: password,
                    twoFactorCode: null,
                    requestedScopes: ["openid", "profile", "email", "roles"],
                    ct: context.RequestAborted);

                if (identity is null)
                {
                    var errorQuery = Uri.EscapeDataString("Invalid credentials or deactivated account.");
                    var returnUrlQuery = Uri.EscapeDataString(returnUrl);
                    return Results.Ok($"/connect/login?error={errorQuery}&returnUrl={returnUrlQuery}");
                }

                // Create Cookie identity for session
                var cookieIdentity = new ClaimsIdentity(identity.Claims, OidcConstant.CookieScheme);
                await context.SignInAsync(OidcConstant.CookieScheme, new ClaimsPrincipal(cookieIdentity));

                return Results.Redirect(returnUrl);
            })
            .DisableAntiforgery()
            .ExcludeFromDescription()
            .AllowAnonymous();

        return endpoints;
    }
}