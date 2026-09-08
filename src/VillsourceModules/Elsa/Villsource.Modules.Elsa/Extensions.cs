using Elsa.Common.Multitenancy;
using Finbuckle.MultiTenant.Abstractions;
using FSH.Framework.Shared.Multitenancy;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Villsource.Modules.Elsa;

public static class Extensions
{
    public static WebApplication UseElsaMultiTenantDatabases(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.Use(async (context, next) =>
        {
            var tenantAccessor = context.RequestServices.GetRequiredService<ITenantAccessor>();
            var multiTenantContextAccessor =
                context.RequestServices.GetService<IMultiTenantContextAccessor<AppTenantInfo>>();

            var tenantInfo = multiTenantContextAccessor?.MultiTenantContext?.TenantInfo;

            if (tenantInfo is not null && !string.IsNullOrEmpty(tenantInfo.Id))
            {
                tenantAccessor.PushContext(new Tenant
                {
                Id = tenantInfo.Id,
                Name = tenantInfo.Name ?? tenantInfo.Id,
                TenantId = tenantInfo.Identifier ?? tenantInfo.Id,
                });
            }

            await next();
        });

        return app;
    }
}