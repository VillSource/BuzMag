using Asp.Versioning;
using FluentValidation;
using FSH.Framework.Persistence;
using FSH.Framework.Shared.Constants;
using FSH.Framework.Web.Modules;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
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

        builder.Services.AddHealthChecks().AddDbContextCheck<OidcDbContext>(
            name: "db:Oidc",
            failureStatus: HealthStatus.Unhealthy);
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var versionSet = endpoints.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1))
            .ReportApiVersions()
            .Build();

        var group = endpoints.MapGroup("api/v{version:apiVersion}/Oidc")
            .WithTags(ApiTags.Oidc)
            .WithApiVersionSet(versionSet)
            .RequireAuthorization();

        group.MapPingEndpoints();
    }
}