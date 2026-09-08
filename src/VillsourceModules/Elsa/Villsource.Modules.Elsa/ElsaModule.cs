using Elsa.Extensions;
using Elsa.Persistence.EFCore.Extensions;
using Elsa.Persistence.EFCore.Modules.Management;
using Elsa.Persistence.EFCore.Modules.Runtime;
using Elsa.Tenants.Extensions;
using Elsa.Tenants.Features;
using FluentValidation;
using FSH.Framework.Persistence;
using FSH.Framework.Shared.Constants;
using FSH.Framework.Shared.Multitenancy;
using FSH.Framework.Web.Modules;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Villsource.Modules.Elsa.Contracts.Authorization;
using Villsource.Modules.Elsa.Contracts.v1;
using Villsource.Modules.Elsa.Data;
using ITenantResolver = Elsa.Common.Multitenancy.ITenantResolver;

namespace Villsource.Modules.Elsa;

public sealed class ElsaModule : IModule
{
    public void ConfigureServices(IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        PermissionConstants.Register(ElsaPermissions.All);

        builder.Services.AddHeroDbContext<ElsaDbContext>();
        builder.Services.AddScoped<IDbInitializer, ElsaDbInitializer>();
        builder.Services.AddValidatorsFromAssembly(typeof(ElsaModule).Assembly);


        builder.Services.AddElsa(elsa =>
        {
            elsa.UseTenants( (TenantsFeature tenants) =>
            {
                tenants.Services.AddScoped<ITenantResolver, AppTenantResolver>();
            });

            elsa.UseWorkflowManagement(management => management.UseEntityFrameworkCore(
                (WorkflowManagementPersistenceFeature ef) => 
                {
                    ef.UsePostgreSql(sp =>
                    {
                        var tenantInfo = sp.GetService<AppTenantInfo>();
                        return (tenantInfo != null && !string.IsNullOrWhiteSpace(tenantInfo.ConnectionString))
                            ? tenantInfo.ConnectionString
                            : builder.Configuration["DatabaseOptions:ConnectionString"]!;
                    });
                
                    ef.RunMigrations = false; 
                }));
            elsa.UseWorkflowRuntime(runtime => runtime.UseEntityFrameworkCore(
                (EFCoreWorkflowRuntimePersistenceFeature ef) => 
                {
                    ef.UsePostgreSql(sp =>
                    {
                        var tenantInfo = sp.GetService<AppTenantInfo>();
                        return (tenantInfo != null && !string.IsNullOrWhiteSpace(tenantInfo.ConnectionString))
                            ? tenantInfo.ConnectionString
                            : builder.Configuration["DatabaseOptions:ConnectionString"]!;
                    });
                
                    ef.RunMigrations = false; 
                }));

            elsa.AddWorkflowsFrom<ElsaModule>();
        });
        
        builder.Services.AddHealthChecks().AddDbContextCheck<ElsaDbContext>(
            name: "db:elsa",
            failureStatus: HealthStatus.Unhealthy);
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        endpoints.MapGet("el", async(IMediator m)=>await m.Send(new Mock()));
    }
}
