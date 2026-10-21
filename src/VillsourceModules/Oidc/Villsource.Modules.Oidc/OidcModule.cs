using Asp.Versioning;
using FluentValidation;
using FSH.Framework.Persistence;
using FSH.Framework.Shared.Constants;
using FSH.Framework.Web.Modules;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using OpenIddict.Validation.AspNetCore;
using Villsource.Modules.Oidc.Constants;
using Villsource.Modules.Oidc.Contracts.Authorization;
using Villsource.Modules.Oidc.Data;
using Villsource.Modules.Oidc.Features.v1;
using Villsource.Modules.Oidc.Features.v1.Applications;
using Villsource.Modules.Oidc.Services;

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

        builder.Services.AddScoped<IOidcUserClaimsService, OidcUserClaimsService>();

        builder.Services.AddOidcProvider(builder.Configuration);

        builder.Services.AddHealthChecks().AddDbContextCheck<OidcDbContext>(
            name: "db:oidc",
            failureStatus: HealthStatus.Unhealthy);

        builder.Services.AddAuthentication()
            .AddCookie(OidcConstant.CookieScheme, options =>
            {
                options.Cookie.Name = OidcConstant.CookieScheme;
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.ExpireTimeSpan = TimeSpan.FromHours(24);
                options.SlidingExpiration = true;
                options.LoginPath = "/api/v1/oidc/login";
            });
        
        builder.Services.Configure<AuthorizationOptions>(options =>
        {
            // ดึง Policy เดิมที่สร้างไว้ก่อนหน้านี้ออกมา
            var existingPolicy = options.GetPolicy("RequiredPermission");
    
            if (existingPolicy != null)
            {
                // สร้าง Builder โดยโยน Policy เดิมเข้าไป (มันจะก็อปปี้ Requirements และ Schemes เดิมมาทั้งหมด)
                var policyBuilder = new AuthorizationPolicyBuilder(existingPolicy);
        
                // เพิ่ม Scheme ใหม่เข้าไป
                policyBuilder.AddAuthenticationSchemes(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
        
                // Build เป็น Policy ตัวใหม่
                var newPolicy = policyBuilder.Build();
        
                // นำไปเขียนทับ Policy ชื่อเดิมในระบบ (ASP.NET Core จะทำการ Overwrite ให้)
                options.AddPolicy("RequiredPermission", newPolicy);
        
                // สำคัญ: ต้องอัปเดต Default และ Fallback ด้วย 
                // เพราะโค้ดเดิมของคุณ (options.DefaultPolicy = ...) ได้ผูก reference ไว้กับ Object ของ Policy ตัวเก่าไปแล้ว
                if (options.DefaultPolicy == existingPolicy)
                {
                    options.DefaultPolicy = newPolicy;
                }
        
                if (options.FallbackPolicy == existingPolicy)
                {
                    options.FallbackPolicy = newPolicy;
                }
            }
        });
        
        
        var assetsPath = Path.Combine(AppContext.BaseDirectory, "Oidc");
        if (!Directory.Exists(assetsPath))
        {
            Directory.CreateDirectory(assetsPath);
        }
        builder.Services.AddTransient<IStartupFilter>(sp => new OidcStaticFileStartupFilter(assetsPath, "/api/v1/oidc"));
        
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        endpoints.MapLoginPageEndpoints();
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

        var appGroup = group.MapGroup("/applications")
            .WithTags("OidcApplications");

        appGroup.MapCreateApplicationEndpoint();
        appGroup.MapGetApplicationsEndpoint();
        appGroup.MapGetApplicationByIdEndpoint();
        appGroup.MapUpdateApplicationEndpoint();
        appGroup.MapDeleteApplicationEndpoint();

        endpoints.MapPost("/connect/logout", async (HttpContext httpContext) =>
        {
            await httpContext.SignOutAsync(OidcConstant.CookieScheme);
            return Results.Ok(new { Message = "Logged out from OIDC session" });
        })
        .RequireAuthorization(new Microsoft.AspNetCore.Authorization.AuthorizeAttribute
        {
            AuthenticationSchemes = OidcConstant.CookieScheme
        });
    }
    
}
