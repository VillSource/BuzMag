using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;
using FSH.Framework.Shared.Multitenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Villsource.FSH.Modules.Organization.Contracts.Constants;
using Villsource.FSH.Modules.Organization.Data;
using Villsource.FSH.Modules.Organization.Domain.DocumentTypes;

namespace Villsource.FSH.Modules.Organization.Services;

/// <summary>
/// Idempotently syncs module-registered <see cref="DocumentTypeConstants.All"/> entries into
/// the <c>organization.DocumentTypes</c> table on startup.
/// </summary>
internal sealed class DocumentTypeSyncHostedService(
    IServiceProvider serviceProvider,
    ILogger<DocumentTypeSyncHostedService> logger) : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(2);
    private static readonly TimeSpan MaxWait = TimeSpan.FromMinutes(2);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var registered = DocumentTypeConstants.All;
        if (registered.Count == 0)
        {
            return;
        }

        try
        {
            var tenant = await WaitForRootTenantAsync(stoppingToken).ConfigureAwait(false);
            if (tenant is null)
            {
                logger.LogWarning("Document type sync skipped — tenant catalog was not ready within {Timeout}", MaxWait);
                return;
            }

            using var scope = serviceProvider.CreateScope();
            scope.ServiceProvider.GetRequiredService<IMultiTenantContextSetter>()
                .MultiTenantContext = new MultiTenantContext<AppTenantInfo>(tenant);

            var dbContext = scope.ServiceProvider.GetRequiredService<OrganizationDbContext>();
            var existing = await dbContext.DocumentTypes
                .ToDictionaryAsync(d => d.Code, StringComparer.OrdinalIgnoreCase, stoppingToken)
                .ConfigureAwait(false);

            var changed = false;
            foreach (var item in registered)
            {
                if (existing.TryGetValue(item.Code, out var entity))
                {
                    if (entity.SyncMetadata(item.Name, item.ModuleName, item.Description))
                    {
                        changed = true;
                    }
                }
                else
                {
                    await dbContext.DocumentTypes.AddAsync(
                        DocumentType.Create(item.Code, item.Name, item.ModuleName, item.Description),
                        stoppingToken).ConfigureAwait(false);
                    changed = true;
                }
            }

            if (changed)
            {
                await dbContext.SaveChangesAsync(stoppingToken).ConfigureAwait(false);
                if (logger.IsEnabled(LogLevel.Information))
                {
                    logger.LogInformation("Synced {Count} registered document types", registered.Count);
                }
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Document type sync failed");
        }
    }

    private async Task<AppTenantInfo?> WaitForRootTenantAsync(CancellationToken stoppingToken)
    {
        var deadline = DateTimeOffset.UtcNow + MaxWait;
        while (DateTimeOffset.UtcNow < deadline)
        {
            if (stoppingToken.IsCancellationRequested) return null;

            try
            {
                using var scope = serviceProvider.CreateScope();
                var tenantStore = scope.ServiceProvider.GetRequiredService<IMultiTenantStore<AppTenantInfo>>();
                var tenants = (await tenantStore.GetAllAsync().ConfigureAwait(false)).ToList();
                if (tenants.Count > 0)
                {
                    return tenants[0];
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                if (logger.IsEnabled(LogLevel.Debug))
                {
                    logger.LogDebug(ex, "Tenant store not ready for document type sync; retrying in {Interval}", PollInterval);
                }
            }

            await Task.Delay(PollInterval, stoppingToken).ConfigureAwait(false);
        }

        return null;
    }
}
