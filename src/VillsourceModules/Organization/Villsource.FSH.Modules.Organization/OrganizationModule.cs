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
using Villsource.FSH.Modules.Organization.Contracts.Authorization;
using Villsource.FSH.Modules.Organization.Data;

namespace Villsource.FSH.Modules.Organization;

/// <summary>
/// Notifications module: per-user inbox driven by integration events from other modules. Module
/// Order 750 places it BEFORE Chat (800) so its integration-event handlers are registered
/// before Chat starts publishing — handler registration is order-sensitive.
/// </summary>
public sealed class OrganizationModule : IModule
{
    public void ConfigureServices(IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        PermissionConstants.Register(OrganizationPermissions.All);

        builder.Services.AddHeroDbContext<OrganizationDbContext>();
        builder.Services.AddScoped<IDbInitializer, OrganizationDbInitializer>();
        builder.Services.AddValidatorsFromAssembly(typeof(OrganizationModule).Assembly);

        builder.Services.AddHealthChecks().AddDbContextCheck<OrganizationDbContext>(
            name: "db:organization",
            failureStatus: HealthStatus.Unhealthy);
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var versionSet = endpoints.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1))
            .ReportApiVersions()
            .Build();

        var group = endpoints.MapGroup("api/v{version:apiVersion}/organization")
            .WithTags("Organization")
            .WithApiVersionSet(versionSet)
            .RequireAuthorization();
        
        group.MapGet("/", () => "Welcome to Villsource Organization!");
    }
}
