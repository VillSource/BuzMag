using Elsa.Common.Multitenancy;
using Finbuckle.MultiTenant.Abstractions;
using FSH.Framework.Shared.Constants;
using FSH.Framework.Shared.Multitenancy;
using Microsoft.AspNetCore.Http;
using ITenantResolver = Elsa.Common.Multitenancy.ITenantResolver;

namespace Villsource.Modules.Elsa;

public class AppTenantResolver(
    IMultiTenantContextAccessor<AppTenantInfo> info,
    IMultiTenantStore<AppTenantInfo> tenantStore,
    IHttpContextAccessor httpContextAccessor) : ITenantResolver
{
    public async Task<TenantResolverResult> ResolveAsync(TenantResolverContext context)
    {
        var tenantInfo = info.MultiTenantContext?.TenantInfo;
        if (tenantInfo is not null && !string.IsNullOrWhiteSpace(tenantInfo.Id))
        {
            return TenantResolverResult.Resolved(tenantInfo.Id);
        }

        var tenantId = httpContextAccessor.HttpContext?.User.FindFirst(ClaimConstants.Tenant)?.Value;
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            return TenantResolverResult.Unresolved();
        }

        tenantInfo = await tenantStore.GetAsync(tenantId).ConfigureAwait(false);
        return tenantInfo is not null && !string.IsNullOrWhiteSpace(tenantInfo.Id)
            ? TenantResolverResult.Resolved(tenantInfo.Id)
            : TenantResolverResult.Unresolved();
    }
}