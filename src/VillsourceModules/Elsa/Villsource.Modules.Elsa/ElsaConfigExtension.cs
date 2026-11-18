using Elsa.Extensions;
using Elsa.Common.Features;
using Elsa.Common.Multitenancy;
using Elsa.Persistence.EFCore.Extensions;
using Elsa.Persistence.EFCore.Modules.Labels;
using Elsa.Persistence.EFCore.Modules.Management;
using Elsa.Persistence.EFCore.Modules.Runtime;
using Elsa.Tenants.Extensions;
using Finbuckle.MultiTenant.Abstractions;
using FSH.Framework.Shared.Multitenancy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi;
using Villsource.FSH.Modules.Organization.Workflows;
using Villsource.Modules.Elsa.Core;

namespace Villsource.Modules.Elsa;

public static class ElsaConfigExtension
{
    public static void ConfigElsa(this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        
        builder.Services.AddScoped<AppTenantResolver>();
        builder.Services.AddElsa(elsa =>
        {
            elsa.Configure<MultitenancyFeature>(feature =>
                feature.UseTenantsProvider<FinbuckleTenantsProvider>());

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
                        GetTenantConnectionString(sp, builder));

                    ef.RunMigrations = false;
                }));
            
            elsa.UseWorkflowRuntime(runtime =>
                runtime.UseEntityFrameworkCore(ef =>
                {
                    ef.UsePostgreSql(sp =>
                        GetTenantConnectionString(sp, builder));

                    ef.RunMigrations = false;
                }));

            elsa.UseWorkflowsApi( c =>
            {
            });
            elsa.AddWorkflowsFrom<ElsaModule>();
            elsa.AddCoreWorkflowsModule();
            elsa.AddOrganizationWorkflowsModule();
            elsa.UseLabels(labels => 
                labels.UseEntityFrameworkCore(ef => 
                {
                    ef.UsePostgreSql(sp => GetTenantConnectionString(sp, builder));
                    ef.RunMigrations = false;
                }));
            elsa.UseDashboardApi();
            elsa.UseWorkflowRuntimeDashboard();
            elsa.UseScheduling(scheduling =>
            {
                scheduling.UseHangfireScheduler();
            });
        });
        
        builder.Services.AddOpenApi(options =>
        {
            options.AddOperationTransformer((operation, context, _) =>
            {
                var path = context.Description.RelativePath?.TrimStart('/');
                if (path != null && path.StartsWith("api/v1/elsa", StringComparison.OrdinalIgnoreCase))
                {
                    var tags = new HashSet<OpenApiTagReference>();
                    tags.Add(new OpenApiTagReference("Elsa"));
                    operation.Tags = tags;
                }

                return Task.CompletedTask;
            });
        });
    }

    private static string GetTenantConnectionString(IServiceProvider services, IHostApplicationBuilder builder)
    {
        var connectionString = services.GetService<IMultiTenantContextAccessor<AppTenantInfo>>()?
            .MultiTenantContext.TenantInfo?.ConnectionString;
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            return connectionString;
        }

        connectionString = services.GetService<ITenantAccessor>()?.Tenant?.Configuration?
            ["ConnectionStrings:PostgreSql"];
        return !string.IsNullOrWhiteSpace(connectionString)
            ? connectionString
            : builder.Configuration["DatabaseOptions:ConnectionString"]!;
    }
}