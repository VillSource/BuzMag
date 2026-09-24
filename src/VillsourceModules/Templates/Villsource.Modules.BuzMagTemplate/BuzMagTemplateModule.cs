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
using Villsource.Modules.BuzMagTemplate.Constants;
using Villsource.Modules.BuzMagTemplate.Contracts.Authorization;
using Villsource.Modules.BuzMagTemplate.Data;
using Villsource.Modules.BuzMagTemplate.Features.v1;

namespace Villsource.Modules.BuzMagTemplate;

public sealed class BuzMagTemplateModule : IModule
{
    public void ConfigureServices(IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        PermissionConstants.Register(BuzMagTemplatePermissions.All);

        builder.Services.AddHeroDbContext<BuzMagTemplateDbContext>();
        builder.Services.AddScoped<IDbInitializer, BuzMagTemplateDbInitializer>();
        builder.Services.AddValidatorsFromAssembly(typeof(BuzMagTemplateModule).Assembly);

        builder.Services.AddHealthChecks().AddDbContextCheck<BuzMagTemplateDbContext>(
            name: "db:BuzMagTemplate",
            failureStatus: HealthStatus.Unhealthy);
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var versionSet = endpoints.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1))
            .ReportApiVersions()
            .Build();

        var group = endpoints.MapGroup("api/v{version:apiVersion}/BuzMagTemplate")
            .WithTags(ApiTags.BuzMagTemplate)
            .WithApiVersionSet(versionSet)
            .RequireAuthorization();
        
        group.MapPingEndpoints();
        
    }
}
