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
    private static readonly string[] _mockRoles = new[] { "Admin", "tester" };

    public static IEndpointRouteBuilder MapOidcEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapMethods("/connect/authorize", ["GET", "POST"], async (HttpContext context) =>
            {
                var request = context.GetOpenIddictServerRequest()
                              ?? throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

                var identity = new ClaimsIdentity(
                    authenticationType: TokenValidationParameters.DefaultAuthenticationType,
                    nameType: OpenIddictConstants.Claims.Name,
                    roleType: OpenIddictConstants.Claims.Role);

                // 1. Subject ID (อย่าใช้ Guid.Empty เพราะ Elsa / Audit System จะ Parse ไม่ผ่าน)
                var userId = "d3b07384-d113-40a6-a713-d82382f63774";
                
                identity.AddClaim(OpenIddictConstants.Claims.Subject, userId);
                identity.AddClaim(ClaimTypes.NameIdentifier, userId);
                
                // 2. User Profile Info
                identity.AddClaim(OpenIddictConstants.Claims.Name, "Mr. Mock");
                identity.AddClaim(OpenIddictConstants.Claims.PreferredUsername, "mr_mock");
                identity.AddClaim(OpenIddictConstants.Claims.Email, "mr_mock@gmail.com");
                identity.AddClaim(new Claim(
                    OpenIddictConstants.Claims.EmailVerified, 
                    "true", 
                    ClaimValueTypes.Boolean));

                // 3. Roles & Permissions สำหรับ Elsa v3 (Elsa มองหาทั้ง role และ permissions)
                identity.AddClaim(OpenIddictConstants.Claims.Role, "Admin");
                identity.AddClaim("permissions", "*"); 
                
                // 4. Custom Tenant Info
                identity.AddClaim("tenant", "mock");

                // 5. กำหนด Destinations สำหรับ Claims
                foreach (var claim in identity.Claims)
                {
                    if (claim.Type == "permissions")
                    {
                        claim.SetDestinations(OpenIddictConstants.Destinations.AccessToken);
                    }
                    else
                    {
                        claim.SetDestinations(
                            OpenIddictConstants.Destinations.AccessToken,
                            OpenIddictConstants.Destinations.IdentityToken
                        );
                    }
                }

                var principal = new ClaimsPrincipal(identity);
                
                // กำหนด Scopes ที่ Elsa ร้องขอ (openid, profile, email, roles)
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
        
        endpoints.MapMethods("/connect/userinfo", ["GET", "POST"], () =>
            {
                // Return claims ที่ Elsa Studio จะดึงไปป้อนให้ UI
                return Results.Ok(new
                {
                    sub = "d3b07384-d113-40a6-a713-d82382f63774",
                    name = "Mr. Mock",
                    preferred_username = "mr_mock",
                    email = "mr_mock@gmail.com",
                    email_verified = true,
                    role = _mockRoles,
                    permissions = "*",
                    tenant = "mock"
                });
            })
            .ExcludeFromDescription()
            .AllowAnonymous();
        
        return endpoints;
    }
}