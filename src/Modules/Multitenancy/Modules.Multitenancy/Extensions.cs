using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.AspNetCore.Extensions;
using FSH.Modules.Multitenancy.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace FSH.Modules.Multitenancy;

public static class Extensions
{
#pragma warning disable S1133
    [Obsolete("This Middleware move to module setup `FSH.Modules.Multitenancy.FSH.Modules.Multitenancy`")]
#pragma warning restore S1133
    public static WebApplication UseHeroMultiTenantDatabases(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);
        app.UseMultiTenant();

        return app;
    }
}