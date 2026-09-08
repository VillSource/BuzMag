using Elsa.Extensions;
using Elsa.Persistence.EFCore.Extensions;
using Elsa.Persistence.EFCore.Modules.Management;
using Elsa.Persistence.EFCore.Modules.Runtime;
using Elsa.Tenants.Extensions;
using Elsa.Tenants.Features;
using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.Stores;
using FluentValidation;
using FSH.Framework.Persistence;
using FSH.Framework.Shared.Constants;
using FSH.Framework.Shared.Multitenancy;
using FSH.Framework.Web.Modules;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Villsource.Modules.Elsa.Contracts.Authorization;
using Villsource.Modules.Elsa.Contracts.v1;
using Villsource.Modules.Elsa.Data;
using Villsource.Modules.Elsa.Feature.v1;
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
                tenants.ConfigureMultitenancy(options =>
                {
                    options.TenantResolverPipelineBuilder.Append<AppTenantResolver>();
                });
            });

            elsa.UseWorkflowManagement(management => management.UseEntityFrameworkCore(
                (WorkflowManagementPersistenceFeature ef) => 
                {
                    ef.UsePostgreSql(sp =>
                    {
                        var tenantInfo = sp.GetService<IMultiTenantContextAccessor<AppTenantInfo>>()?.MultiTenantContext.TenantInfo;
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
                        var tenantInfo = sp.GetService<IMultiTenantContextAccessor<AppTenantInfo>>()?.MultiTenantContext.TenantInfo;
                        return (tenantInfo != null && !string.IsNullOrWhiteSpace(tenantInfo?.ConnectionString))
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

        endpoints.MapGet("el", async(IMediator m)=>await m.Send(new Mock())).AllowAnonymous();
        var group = endpoints.MapGroup("elsa-demo").WithTags("Elsa Human-in-the-Loop").AllowAnonymous();

        group.MapPost("submit", async (string name, string reqId, IMediator m) => 
            await m.Send(new SubmitRequestCommand(name, reqId)));

        // 2. Endpoint สำหรับอนุมัติหรือปฏิเสธ (Resume Workflow)
        // การใช้งาน: POST /elsa-demo/decision/REQ001?decision=Approve
        group.MapPost("decision/{reqId}", async (string reqId, string decision, IMediator m) => 
            await m.Send(new ApproveRequestCommand(reqId, decision)));
    }
}
