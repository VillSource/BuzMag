using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Finbuckle.MultiTenant.Abstractions;
using FSH.Framework.Shared.Multitenancy;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;
using Villsource.Modules.Oidc.Services;

namespace Villsource.Modules.Oidc.Features.v1;

public static class LoginPageEndpoint
{
    public sealed record LoginForm(string Email, string Password, string? TwoFactorCode = null);

    public static IEndpointRouteBuilder MapLoginPageEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/v1/oidc/is-authenticated", ()=>Results.Ok())
            .RequireAuthorization(new AuthorizeAttribute { AuthenticationSchemes = OidcConstant.CookieScheme });
        
        endpoints.MapPost("/api/v1/oidc/logout", async (HttpContext context) =>
        {
            await context.SignOutAsync(OidcConstant.CookieScheme);
            var returnUrl = context.Request.Query["returnUrl"];
            if (string.IsNullOrEmpty(returnUrl))
                return Results.NoContent();
            return Results.Redirect(returnUrl.ToString());
        }).RequireAuthorization(new AuthorizeAttribute { AuthenticationSchemes = OidcConstant.CookieScheme });

        endpoints.MapGet("/api/v1/oidc/claims", async (HttpContext context) =>
        {
            return Results.Ok(context.User.Claims.ToDictionary(i=>i.Type, i=>i.Value));
        }).RequireAuthorization(new AuthorizeAttribute { AuthenticationSchemes = OidcConstant.CookieScheme });

        
        endpoints.MapPost("/connect/login", async (
                HttpContext context,
                IMultiTenantContextAccessor<AppTenantInfo> tenantContextAccessor,
                IServiceProvider services,
                [FromBody] LoginForm data) =>
            {
                var tenantId = ResolveTenantId(context, tenantContextAccessor);
                if (string.IsNullOrWhiteSpace(tenantId))
                {
                    return Results.BadRequest(new { error = "A tenant is required to sign in." });
                }

                var claimsService = services.GetRequiredService<IOidcUserClaimsService>();
                var identity = await claimsService.BuildOidcIdentityAsync(
                    tenantId: tenantId,
                    email: data.Email,
                    password: data.Password,
                    twoFactorCode: data.TwoFactorCode,
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
                IMultiTenantContextAccessor<AppTenantInfo> tenantContextAccessor,
                IServiceProvider services,
                [FromBody] LoginForm data) =>
            {
                var form = await context.Request.ReadFormAsync();
                var returnUrl = form["returnUrl"].ToString();

                if (string.IsNullOrWhiteSpace(returnUrl))
                {
                    returnUrl = "/";
                }

                var tenantId = ResolveTenantId(context, tenantContextAccessor);
                if (string.IsNullOrWhiteSpace(tenantId))
                {
                    return Results.BadRequest(new { error = "A tenant is required to sign in." });
                }

                var claimsService = services.GetRequiredService<IOidcUserClaimsService>();
                var identity = await claimsService.BuildOidcIdentityAsync(
                    tenantId: tenantId,
                    email: data.Email,
                    password: data.Password,
                    twoFactorCode: data.TwoFactorCode,
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

    private static string? ResolveTenantId(
        HttpContext context,
        IMultiTenantContextAccessor<AppTenantInfo> tenantContextAccessor)
    {
        var tenantId = tenantContextAccessor.MultiTenantContext.TenantInfo?.Id;
        return !string.IsNullOrWhiteSpace(tenantId)
            ? tenantId
            : context.Request.Headers[MultitenancyConstants.Identifier].ToString();
    }
}