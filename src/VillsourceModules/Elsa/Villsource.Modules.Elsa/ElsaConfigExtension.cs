using Elsa.Extensions;
using Elsa.Persistence.EFCore.Extensions;
using Elsa.Persistence.EFCore.Modules.Management;
using Elsa.Persistence.EFCore.Modules.Runtime;
using Elsa.Tenants.Extensions;
using Elsa.Tenants.Features;
using Elsa.Workflows.Management.Features;
using Finbuckle.MultiTenant.Abstractions;
using FSH.Framework.Shared.Multitenancy;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Villsource.Modules.Elsa.Core;
using Villsource.Modules.HumanResource.Workflow;

namespace Villsource.Modules.Elsa;

public static class ElsaConfigExtension
{
    public static void ConfigElsa(this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

#pragma warning disable S125
        // DO NOT REMOVE
        // global::Elsa.EndpointSecurityOptions.DisableSecurity();
#pragma warning restore S125

        builder.Services.AddTransient<IClaimsTransformation, ElsaPermissionTransformer>();
        builder.Services.AddScoped<AppTenantResolver>();
        builder.Services.AddElsa(elsa =>
        {
            elsa.UseTenants(tenants =>
            {
                tenants.ConfigureMultitenancy(options =>
                {
                    options.TenantResolverPipelineBuilder.Clear()
                        .Append<AppTenantResolver>();
                });
            });

            elsa.UseWorkflowManagement(management =>
                management.UseEntityFrameworkCore(ef =>
                {
                    ef.UsePostgreSql(sp =>
                    {
                        var tenantInfo = ServiceProviderServiceExtensions.GetService<IMultiTenantContextAccessor<AppTenantInfo>>(sp)?.MultiTenantContext
                            .TenantInfo;
                        return (tenantInfo != null && !string.IsNullOrWhiteSpace(tenantInfo.ConnectionString))
                            ? tenantInfo.ConnectionString
                            : builder.Configuration["DatabaseOptions:ConnectionString"]!;
                    });

                    ef.RunMigrations = false;
                }));
            
            elsa.UseWorkflowRuntime(runtime =>
                runtime.UseEntityFrameworkCore(ef =>
                {
                    ef.UsePostgreSql(sp =>
                    {
                        var tenantInfo = sp.GetService<IMultiTenantContextAccessor<AppTenantInfo>>()?.MultiTenantContext
                            .TenantInfo;
                        return (tenantInfo != null && !string.IsNullOrWhiteSpace(tenantInfo?.ConnectionString))
                            ? tenantInfo.ConnectionString
                            : builder.Configuration["DatabaseOptions:ConnectionString"]!;
                    });

                    ef.RunMigrations = false;
                }));

            elsa.UseWorkflowsApi();
            elsa.AddWorkflowsFrom<ElsaModule>();
            elsa.AddCoreWorkflowsModule();
            elsa.AddHrWorkflowsModule();
        });
    }
}