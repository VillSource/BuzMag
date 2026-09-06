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
using Villsource.Modules.HumanResource.Constants;
using Villsource.Modules.HumanResource.Contracts.Authorization;
using Villsource.Modules.HumanResource.Data;
using Villsource.Modules.HumanResource.Features.v1;
using Villsource.Modules.HumanResource.Services;

namespace Villsource.Modules.HumanResource;

public sealed class HumanResourceModule : IModule
{
    public void ConfigureServices(IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        PermissionConstants.Register(HumanResourcePermissions.All);

        builder.Services.AddHeroDbContext<HumanResourceDbContext>();
        builder.Services.AddScoped<IDbInitializer, HumanResourceDbInitializer>();
        builder.Services.AddValidatorsFromAssembly(typeof(HumanResourceModule).Assembly);
        
        builder.Services.AddTransient<IEmployeeCodeFactory, EmployeeCodeFactory>();

        builder.Services.AddHealthChecks().AddDbContextCheck<HumanResourceDbContext>(
            name: "db:human-resource",
            failureStatus: HealthStatus.Unhealthy);
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        endpoints.MapHumanResourceEndpoints();
    }
}
