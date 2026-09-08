using Elsa.Common.Multitenancy;
using Finbuckle.MultiTenant.Abstractions;
using FSH.Framework.Shared.Multitenancy;
using Microsoft.Extensions.DependencyInjection;
using ITenantResolver = Elsa.Common.Multitenancy.ITenantResolver;

namespace Villsource.Modules.Elsa;

public class AppTenantResolver(IMultiTenantContextAccessor<AppTenantInfo> info) : ITenantResolver
{
    public Task<TenantResolverResult> ResolveAsync(TenantResolverContext context)
    {
        var tenantInfo = info.MultiTenantContext?.TenantInfo;
        return Task.FromResult(tenantInfo is not null && !string.IsNullOrEmpty(tenantInfo.Id)
            ? TenantResolverResult.Resolved(tenantInfo.Id)
            : TenantResolverResult.Unresolved());
    }
}