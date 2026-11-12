using System.Security.Claims;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using OpenIddict.Validation.AspNetCore;
using System.Security.Cryptography;
using Villsource.Modules.Oidc.Services;

namespace Villsource.Modules.Oidc.Features.v1;

public static class OidcEndpoints
{
    public static IEndpointRouteBuilder MapOidcEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapMethods("/connect/authorize", ["GET", "POST"], async (
            HttpContext context,
            IOidcUserClaimsService claimsService,
            IOptions<OidcIdentityTokenOptions> tokenOptions) =>
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

            var bypassConfirmLogin = context.Request.Query["bypass_confirm_login"].Count > 0;
            if (!bypassConfirmLogin)
            {
                var claims = authResult.Principal.Claims;
                var rows = string.Join( Environment.NewLine,
                    claims.Select(c => //lang=html
                        $"""
                        <tr>
                            <td>{System.Net.WebUtility.HtmlEncode(c.Type)}</td>
                            <td>{System.Net.WebUtility.HtmlEncode(c.Value)}</td>
                        </tr>
                        """));
                
                var returnUrl = $"{context.Request.Path}{context.Request.QueryString}&bypass_confirm_login=true";
                var html =  // lang=html
                    $$$$$$"""
                              <!DOCTYPE html>
                              <html>
                              <head>
                              <style>
                                  :root {
                                      color-scheme: light dark;
                                  }

                                  body {
                                      font-family: Inter, Segoe UI, sans-serif;
                                      max-width: 960px;
                                      margin: 40px auto;
                                      padding: 0 20px;
                                      color: #111827;
                                      background: #f9fafb;
                                  }

                                  .card {
                                      background: white;
                                      border: 1px solid #e5e7eb;
                                      border-radius: 12px;
                                      overflow: hidden;
                                      box-shadow: 0 1px 3px rgba(0,0,0,.05);
                                  }

                                  h1 {
                                      margin: 0 0 20px;
                                      font-size: 1.5rem;
                                      font-weight: 600;
                                  }

                                  table {
                                      width: 100%;
                                      border-collapse: collapse;
                                  }

                                  th {
                                      text-align: left;
                                      background: #f3f4f6;
                                      color: #6b7280;
                                      font-size: .85rem;
                                      font-weight: 600;
                                      padding: 12px 16px;
                                      border-bottom: 1px solid #e5e7eb;
                                  }

                                  td {
                                      padding: 12px 16px;
                                      border-bottom: 1px solid #f3f4f6;
                                      vertical-align: top;
                                  }

                                  code {
                                      font-family: Consolas, monospace;
                                      font-size: .9rem;
                                  }

                                  tr:last-child td {
                                      border-bottom: none;
                                  }

                                  .actions {
                                      margin-top: 20px;
                                  }

                                  .btn {
                                      display: inline-block;
                                      padding: 10px 16px;
                                      border-radius: 8px;
                                      background: #111827;
                                      color: white;
                                      text-decoration: none;
                                      font-weight: 500;
                                  }

                                  .btn:hover {
                                      opacity: .9;
                                  }
                              </style>
                              </head>
                              <body>

                                  <h1>Login Confirmation as {{{{{{claims.First(i => i.Type == "fullName").Value}}}}}}</h1>

                                  <div class="actions" style="margin-bottom: 10px;">
                                      <a class="btn" href="{{{{{{returnUrl}}}}}}">
                                          Login as {{{{{{claims.First(i => i.Type == "fullName").Value}}}}}}
                                      </a>
                                      <form method="post" action="/api/v1/oidc/logout?returnUrl={{{{{{Uri.EscapeDataString(returnUrl)}}}}}}" style="display:inline">
                                          <button type="submit" class="btn btn-secondary">
                                             Login as other account 
                                          </button>
                                      </form>
                                  </div>
                                  
                                  <div class="card">
                                      <table>
                                          <thead>
                                              <tr>
                                                  <th>Claim Type</th>
                                                  <th>Value</th>
                                              </tr>
                                          </thead>
                                          <tbody>
                                              {{{{{{rows}}}}}}
                                          </tbody>
                                      </table>
                                  </div>

                              </body>
                              </html>
                              """; 
                return Results.Content(html, "text/html");

            }

            var cookieUser = authResult.Principal;
            var subject = cookieUser.FindFirstValue(ClaimTypes.NameIdentifier)
                          ?? cookieUser.FindFirstValue(OpenIddictConstants.Claims.Subject);
            var tenant = cookieUser.FindFirstValue("tenant");

            if (string.IsNullOrEmpty(subject) || string.IsNullOrEmpty(tenant))
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
            principal.SetResources(tokenOptions.Value.Audience);

            return Results.SignIn(principal,
                authenticationScheme: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        })
        .ExcludeFromDescription()
        .AllowAnonymous();

        endpoints.MapPost("/connect/token", async (
            HttpContext context,
            IOidcUserClaimsService claimsService,
            IOptions<OidcIdentityTokenOptions> tokenOptions) =>
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

                ClaimsPrincipal principal = result.Principal;
                if (request.IsRefreshTokenGrantType())
                {
                    var subject = principal.FindFirstValue(OpenIddictConstants.Claims.Subject)
                                  ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);
                    var tenant = principal.FindFirstValue("tenant");
                    if (string.IsNullOrEmpty(subject) || string.IsNullOrEmpty(tenant))
                    {
                        return Results.BadRequest(new
                        {
                            error = "invalid_grant",
                            error_description = "The token is invalid or expired."
                        });
                    }

                    var scopes = principal.GetScopes();
                    var identity = await claimsService.BuildOidcIdentityForSubjectAsync(
                        subject,
                        tenant,
                        scopes,
                        context.RequestAborted);
                    if (identity is null)
                    {
                        return Results.BadRequest(new
                        {
                            error = "invalid_grant",
                            error_description = "The token is invalid or expired."
                        });
                    }

                    principal = new ClaimsPrincipal(identity);
                    principal.SetScopes(scopes);
                    principal.SetResources(tokenOptions.Value.Audience);
                }

                return Results.SignIn(principal,
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
            var tenant = result.Principal.FindFirstValue("tenant");

            if (string.IsNullOrEmpty(subject) || string.IsNullOrEmpty(tenant))
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
