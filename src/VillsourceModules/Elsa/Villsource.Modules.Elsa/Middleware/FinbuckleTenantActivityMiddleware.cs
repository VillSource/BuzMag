using Elsa.Common.Multitenancy;
using Elsa.Workflows;
using Elsa.Workflows.Pipelines.ActivityExecution;
using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;
using FSH.Framework.Shared.Multitenancy;

namespace Villsource.Modules.Elsa.Middleware;

public sealed class FinbuckleTenantActivityPipelineContributor : IActivityExecutionPipelineContributor
{
    public int Order => 0;

    public void Configure(IActivityExecutionPipelineBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.Insert(0, next => new FinbuckleTenantActivityMiddleware(next).InvokeAsync);
    }
}

/// <summary>
/// Synchronizes Finbuckle's <see cref="IMultiTenantContextAccessor{AppTenantInfo}"/> from Elsa's
/// active tenant context before executing workflow activities (including background invocations)
/// so EF Core <c>BaseDbContext</c> instances have a valid tenant filter and connection string.
/// </summary>
public sealed class FinbuckleTenantActivityMiddleware(ActivityMiddlewareDelegate next)
{
    public async ValueTask InvokeAsync(ActivityExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var accessor = context.GetService<IMultiTenantContextAccessor<AppTenantInfo>>();
        var setter = context.GetService<IMultiTenantContextSetter>();
        var tenantStore = context.GetService<IMultiTenantStore<AppTenantInfo>>();

        if (accessor is null || setter is null || tenantStore is null)
        {
            await next(context).ConfigureAwait(false);
            return;
        }

        var existingContext = accessor.MultiTenantContext;
        if (existingContext?.TenantInfo is not null && !string.IsNullOrWhiteSpace(existingContext.TenantInfo.Id))
        {
            await next(context).ConfigureAwait(false);
            return;
        }

        var tenantId = context.GetService<ITenantAccessor>()?.Tenant?.Id;
        if (string.IsNullOrWhiteSpace(tenantId) ||
            string.Equals(tenantId, Tenant.AgnosticTenantId, StringComparison.Ordinal))
        {
            tenantId = context.WorkflowExecutionContext.Workflow.Identity.TenantId;
        }

        if (string.IsNullOrWhiteSpace(tenantId) ||
            string.Equals(tenantId, Tenant.AgnosticTenantId, StringComparison.Ordinal))
        {
            await next(context).ConfigureAwait(false);
            return;
        }

        var tenantInfo = await tenantStore.GetAsync(tenantId).ConfigureAwait(false);
        if (tenantInfo is null)
        {
            await next(context).ConfigureAwait(false);
            return;
        }

        setter.MultiTenantContext = new MultiTenantContext<AppTenantInfo>(tenantInfo);
        try
        {
            await next(context).ConfigureAwait(false);
        }
        finally
        {
            setter.MultiTenantContext = existingContext!;
        }
    }
}
