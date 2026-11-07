using Elsa.Common.Multitenancy;
using Finbuckle.MultiTenant.Abstractions;
using FSH.Framework.Shared.Multitenancy;

namespace Villsource.Modules.Elsa;

public sealed class FinbuckleTenantsProvider(IMultiTenantStore<AppTenantInfo> tenantStore) : ITenantsProvider
{
    public async Task<IEnumerable<Tenant>> ListAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var tenantInfos = await tenantStore.GetAllAsync().ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();

        return new[] { Tenant.Default }
            .Concat(tenantInfos
                .Where(tenantInfo => !string.IsNullOrWhiteSpace(tenantInfo.Id))
                .Select(ToTenant));
    }

    public async Task<Tenant?> FindAsync(TenantFilter filter, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(filter.Id))
        {
            return Tenant.Default;
        }

        var tenantInfo = await tenantStore.GetAsync(filter.Id).ConfigureAwait(false);
        return tenantInfo is null || string.IsNullOrWhiteSpace(tenantInfo.Id)
            ? null
            : ToTenant(tenantInfo);
    }

    private static Tenant ToTenant(AppTenantInfo tenantInfo) => new()
    {
        Id = tenantInfo.Id,
        Name = string.IsNullOrWhiteSpace(tenantInfo.Name) ? tenantInfo.Id : tenantInfo.Name
    };
}