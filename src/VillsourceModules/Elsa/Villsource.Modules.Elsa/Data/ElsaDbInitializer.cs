
using Elsa.Extensions;
using Elsa.Common.Multitenancy;
using Elsa.Persistence.EFCore.Modules.Labels;
using Elsa.Persistence.EFCore.Modules.Management;
using Elsa.Persistence.EFCore.Modules.Runtime;
using Elsa.Workflows;
using Elsa.Workflows.Management;
using Elsa.Workflows.Management.Entities;
using Elsa.Workflows.Management.Filters;
using Finbuckle.MultiTenant.Abstractions;
using FSH.Framework.Persistence;
using FSH.Framework.Shared.Multitenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Villsource.Modules.Elsa.Workflows;

namespace Villsource.Modules.Elsa.Data;

public sealed class ElsaDbInitializer(
    IMultiTenantContextAccessor<AppTenantInfo> tenantContextAccessor,
    ITenantScopeFactory tenantScopeFactory,
    ILogger<ElsaDbInitializer> logger) : IDbInitializer
{
    public async Task MigrateAsync(CancellationToken cancellationToken)
    {
        await using var tenantScope = CreateElsaTenantScope();
        var services = tenantScope.ServiceProvider;
        var dbContext = services.GetRequiredService<ElsaDbContext>();
        var managementDbContext = services.GetRequiredService<ManagementElsaDbContext>();
        var runtimeDbContext = services.GetRequiredService<RuntimeElsaDbContext>();
        var labelsDbContext = services.GetRequiredService<LabelsElsaDbContext>();
        UseTenantConnectionString(
            dbContext,
            managementDbContext,
            runtimeDbContext,
            labelsDbContext,
            tenantContextAccessor.MultiTenantContext.TenantInfo?.ConnectionString);

        if ((await dbContext.Database.GetPendingMigrationsAsync(cancellationToken).ConfigureAwait(false)).Any())
        {
            await dbContext.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);
            logger.LogInformation("[elsa] applied migrations");
        }
        
        if ((await managementDbContext.Database.GetPendingMigrationsAsync(cancellationToken).ConfigureAwait(false)).Any())
        {
            await managementDbContext.Database.MigrateAsync(cancellationToken);
            logger.LogInformation("[elsa-management] applied migrations");
        }
        
        if ((await runtimeDbContext.Database.GetPendingMigrationsAsync(cancellationToken).ConfigureAwait(false)).Any())
        {
            await runtimeDbContext.Database.MigrateAsync(cancellationToken);
            logger.LogInformation("[elsa-runtime] applied migrations");
        }
        
        if ((await labelsDbContext.Database.GetPendingMigrationsAsync(cancellationToken).ConfigureAwait(false)).Any())
        {
            await labelsDbContext.Database.MigrateAsync(cancellationToken);
            logger.LogInformation("[elsa-labels] applied migrations");
        }

    }

    public async Task SeedAsync(CancellationToken cancellationToken)
    {
#pragma warning disable S125
        // await using var tenantScope = CreateElsaTenantScope();
#pragma warning restore S125
        // var services = tenantScope.ServiceProvider;
        // var dbContext = services.GetRequiredService<ElsaDbContext>();
        // var managementDbContext = services.GetRequiredService<ManagementElsaDbContext>();
        // var runtimeDbContext = services.GetRequiredService<RuntimeElsaDbContext>();
        // UseTenantConnectionString(
        //     dbContext,
        //     managementDbContext,
        //     runtimeDbContext,
        //     tenantContextAccessor.MultiTenantContext.TenantInfo?.ConnectionString);
        //
        // var workflowBuilderFactory = services.GetRequiredService<IWorkflowBuilderFactory>();
        // var workflowDefinitionPublisher = services.GetRequiredService<IWorkflowDefinitionPublisher>();
        //
        // await SeedWorkflowAsync(managementDbContext, workflowBuilderFactory, workflowDefinitionPublisher,
        //     "HelloWorldWorkflow", new HelloWorldWorkflow(), cancellationToken)
        //     .ConfigureAwait(false);
        // await SeedWorkflowAsync(managementDbContext, workflowBuilderFactory, workflowDefinitionPublisher,
        //     "DocumentApprovalWorkflow", new DocumentApprovalWorkflow(), cancellationToken)
        //     .ConfigureAwait(false);
        // await SeedWorkflowAsync(managementDbContext, workflowBuilderFactory, workflowDefinitionPublisher,
        //     "LeaveApprovalWorkflow", new LeaveApprovalWorkflow(), cancellationToken)
        //     .ConfigureAwait(false);
    }

    private static void UseTenantConnectionString(
        ElsaDbContext dbContext,
        ManagementElsaDbContext managementDbContext,
        RuntimeElsaDbContext runtimeDbContext,
        LabelsElsaDbContext labelsElsaDbContext,
        string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        dbContext.Database.SetConnectionString(connectionString);
        managementDbContext.Database.SetConnectionString(connectionString);
        runtimeDbContext.Database.SetConnectionString(connectionString);
        labelsElsaDbContext.Database.SetConnectionString(connectionString);

    }

    private TenantScope CreateElsaTenantScope()
    {
        var tenantInfo = tenantContextAccessor.MultiTenantContext.TenantInfo
            ?? throw new InvalidOperationException("Elsa initialization requires an active Finbuckle tenant.");

        var tenant = new Tenant
        {
            Id = tenantInfo.Id,
            Name = string.IsNullOrWhiteSpace(tenantInfo.Name) ? tenantInfo.Id : tenantInfo.Name,
            Configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:PostgreSql"] = tenantInfo.ConnectionString
                })
                .Build()
        };

        return tenantScopeFactory.CreateScope(tenant);
    }

    // ReSharper disable once UnusedMember.Local
#pragma warning disable S1144
    private async Task SeedWorkflowAsync(
#pragma warning restore S1144
        ManagementElsaDbContext managementDbContext,
        IWorkflowBuilderFactory workflowBuilderFactory,
        IWorkflowDefinitionPublisher workflowDefinitionPublisher,
        string definitionId,
        IWorkflow source,
        CancellationToken cancellationToken)
    {
        var existingDefinitions = await managementDbContext.Set<WorkflowDefinition>()
            .IgnoreQueryFilters()
            .Where(definition => definition.DefinitionId == definitionId)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (existingDefinitions.Count > 0)
        {
            foreach (var existingDefinition in existingDefinitions)
            {
                managementDbContext.Entry(existingDefinition)
                    .Property("TenantId")
                    .CurrentValue = Tenant.AgnosticTenantId;
            }

            await managementDbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return;
        }

        var workflow = await workflowBuilderFactory.CreateBuilder()
            .WithDefinitionId(definitionId)
            .WithTenantId(Tenant.AgnosticTenantId)
            .BuildWorkflowAsync(source, cancellationToken)
            .ConfigureAwait(false);
        var definition = await workflowDefinitionPublisher.NewAsync(workflow, cancellationToken)
            .ConfigureAwait(false);
        definition.DefinitionId = definitionId;
        definition.Name = workflow.Name;
        await workflowDefinitionPublisher.PublishAsync(definition, cancellationToken)
            .ConfigureAwait(false);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("[elsa] seeded built-in workflow {WorkflowDefinitionId}", definitionId);
        }
    }
}
