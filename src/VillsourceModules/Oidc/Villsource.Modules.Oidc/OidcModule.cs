using Asp.Versioning;
using FluentValidation;
using FSH.Framework.Persistence;
using FSH.Framework.Shared.Constants;
using FSH.Framework.Web.Modules;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Villsource.Modules.Oidc.Constants;
using Villsource.Modules.Oidc.Contracts.Authorization;
using Villsource.Modules.Oidc.Data;
using Villsource.Modules.Oidc.Features.v1;

namespace Villsource.Modules.Oidc;

public sealed class OidcModule : IModule
{
    public void ConfigureServices(IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        PermissionConstants.Register(OidcPermissions.All);

        builder.Services.AddHeroDbContext<OidcDbContext>();
        builder.Services.AddScoped<IDbInitializer, OidcDbInitializer>();
        builder.Services.AddValidatorsFromAssembly(typeof(OidcModule).Assembly);

        builder.Services.AddOidcProvider(builder.Configuration);
        
        builder.Services.AddHealthChecks().AddDbContextCheck<OidcDbContext>(
            name: "db:oidc",
            failureStatus: HealthStatus.Unhealthy);
        
        builder.Services.AddAuthentication()
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
            {
                options.Cookie.Name = "fsh_oidc_session";
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.ExpireTimeSpan = TimeSpan.FromHours(8);
                options.SlidingExpiration = true;

                // เมื่อ Unauthenticated และถูก Challenge ไม่ให้ Redirect ไปหน้า /login
                // แต่ส่ง 401 ออกไปพร้อม Header WWW-Authenticate เพื่อ trigger Basic Auth Popup ของ Browser
                options.Events.OnRedirectToLogin = context =>
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Response.Headers.Append("WWW-Authenticate", "Basic realm=\"FSH OIDC Authorization Server\"");
                    return Task.CompletedTask;
                };
            })
            .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicScheme", null);
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        endpoints.MapOidcEndpoints();

        var versionSet = endpoints.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1))
            .ReportApiVersions()
            .Build();

        var group = endpoints.MapGroup("api/v{version:apiVersion}/oidc")
            .WithTags(ApiTags.Oidc)
            .WithApiVersionSet(versionSet)
            .RequireAuthorization();

        group.MapPingEndpoints();
        
        group.MapGet("/authorize", async (HttpContext httpContext) =>
            {
                var user = httpContext.User;

                // หาก Authenticate ผ่าน Basic Auth ให้สั่งออก Cookie Session ทันที
                if (user.Identity?.AuthenticationType == "BasicScheme")
                {
                    var cookieIdentity = new ClaimsIdentity(user.Claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    await httpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(cookieIdentity));
                }

                var tenant = user.FindFirstValue("tenant");
                var username = user.Identity?.Name;

                return Results.Ok(new
                {
                    Message = "OIDC Authorize Successful",
                    User = username,
                    Tenant = tenant,
                    AuthType = user.Identity?.AuthenticationType
                });
            })
            // กำหนด Scheme เฉพาะของ OIDC ใน Minimal API (ตรวจ Cookie ก่อน ถ้าไม่มีให้ fallback ไป Basic Auth)
            .RequireAuthorization(new Microsoft.AspNetCore.Authorization.AuthorizeAttribute
            {
                AuthenticationSchemes = $"{CookieAuthenticationDefaults.AuthenticationScheme},BasicScheme"
            });
        
        group.MapPost("/logout", async (HttpContext httpContext) =>
            {
                await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return Results.Ok(new { Message = "Logged out from OIDC session" });
            })
            .RequireAuthorization(new Microsoft.AspNetCore.Authorization.AuthorizeAttribute
            {
                AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme
            });
    }
}


public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public BasicAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.ContainsKey("Authorization"))
            return AuthenticateResult.NoResult();

        try
        {
            var authHeader = AuthenticationHeaderValue.Parse(Request.Headers.Authorization!);
            if (!authHeader.Scheme.Equals("Basic", StringComparison.OrdinalIgnoreCase))
            {
                return AuthenticateResult.NoResult();
            }

            var credentialBytes = Convert.FromBase64String(authHeader.Parameter ?? string.Empty);
            var credentials = Encoding.UTF8.GetString(credentialBytes).Split(':', 2);
            var rawUsername = credentials[0];
            var rawPassword = credentials[1];

            // 1. แกะ Tenant ID และ Username
            string tenantId =  "root";
            string username = rawUsername;

            if (rawUsername.Contains('\\',  StringComparison.InvariantCultureIgnoreCase))
            {
                var userParts = rawUsername.Split('\\', 2);
                tenantId = userParts[0];
                username = userParts[1];
            }

            // 2. แกะ Password และ 2FA Code
            string password = rawPassword;
            string? twoFactorCode = null;

            if (rawPassword.Contains(':', StringComparison.CurrentCultureIgnoreCase))
            {
                var passParts = rawPassword.Split(':', 2);
                password = passParts[0];
                twoFactorCode = passParts[1];
            }

            _ = password;
            _ = twoFactorCode;
            bool isValid = true; // Mock สำหรับทดสอบ

            if (!isValid)
            {
                return AuthenticateResult.Fail("Invalid Credentials or 2FA Code");
            }

            // 3. สร้าง Principal & Claims
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, username),
                new Claim(ClaimTypes.Name, username),
                new Claim("tenant", tenantId)
            };

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }
#pragma warning disable CA1031
        catch
#pragma warning restore CA1031
        {
            return AuthenticateResult.Fail("Invalid Basic Authorization Header");
        }
    }

    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = StatusCodes.Status401Unauthorized;
        Response.Headers.Append("WWW-Authenticate", "Basic realm=\"FSH OIDC Authorization Server\"");
        return Task.CompletedTask;
    }
}