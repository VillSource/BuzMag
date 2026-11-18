using Elsa.Extensions;
using Elsa.Workflows;
using Elsa.Workflows.Activities;
using Elsa.Workflows.Management;
using Elsa.Workflows.Memory;
using Elsa.Workflows.Models;
using Elsa.Workflows.UIHints;
using Elsa.Workflows.UIHints.CheckList;
using Elsa.Workflows.UIHints.Dropdown;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Villsource.FSH.Modules.Organization.Contracts.Services;
using Villsource.FSH.Modules.Organization.Contracts.Services.Dtos;
using Villsource.FSH.Modules.Organization.Workflows;
using Villsource.Modules.Elsa.Core;

namespace Villsource.FSH.Modules.Organization.Tests.Workflows;

internal static class WorkflowTestHost
{
    public static ServiceProvider BuildServiceProvider(IChainOfCommandService? chainOfCommandService = null)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddScoped(_ => chainOfCommandService ?? Substitute.For<IChainOfCommandService>());
        services.AddElsa(elsa =>
        {
            elsa.AddCoreWorkflowsModule();
            elsa.AddOrganizationWorkflowsModule();
        });

        return services.BuildServiceProvider();
    }

    public static async Task<IActivityRegistry> PopulateRegistryAsync(IServiceProvider services)
    {
        var populator = services.GetRequiredService<IActivityRegistryPopulator>();
        await populator.PopulateRegistryAsync();
        return services.GetRequiredService<IActivityRegistry>();
    }

    public static async Task<RunWorkflowResult> RunActivityAsync(
        IServiceProvider services,
        IActivity activity,
        CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var runner = scope.ServiceProvider.GetRequiredService<IWorkflowRunner>();
        return await runner.RunAsync(activity, cancellationToken: cancellationToken);
    }

    public static async Task<(RunWorkflowResult RunResult, T? Result)> RunCodeActivityAsync<T>(
        IServiceProvider services,
        CodeActivity<T> activity)
    {
        if (string.IsNullOrWhiteSpace(activity.Id))
        {
            activity.Id = "target-activity";
        }

        var runResult = await RunActivityAsync(services, activity);
        var result = GetOutput<T>(runResult, activity, "Result");
        return (runResult, result);
    }

    public static TOut? GetOutput<TOut>(RunWorkflowResult runResult, IActivity activity, string outputName)
    {
        var register = runResult.WorkflowExecutionContext.GetActivityOutputRegister();
        var value = register.FindOutputByActivityId(activity.Id, outputName);
        if (value is null)
        {
            return default;
        }

        return (TOut?)value;
    }

    public static SelectList GetDropDownSelectList(InputDescriptor inputDescriptor)
    {
        inputDescriptor.UIHint.ShouldBe(InputUIHints.DropDown);
        inputDescriptor.UISpecifications.ShouldNotBeNull();
        inputDescriptor.UISpecifications.ShouldContainKey(InputUIHints.DropDown);
        var props = inputDescriptor.UISpecifications[InputUIHints.DropDown].ShouldBeOfType<DropDownProps>();
        props.SelectList.ShouldNotBeNull();
        return props.SelectList;
    }

    public static ChainOfCommandPositionDto CreateSamplePosition(
        string refId = "POS-001",
        string code = "CODE-001",
        string title = "Engineering Manager",
        string internationalTitle = "Engineering Manager (EN)",
        string legalEntityRefId = "LE-001",
        string orgUnitRefId = "OU-001",
        string orgUnitCode = "DEPT-ENG",
        string orgUnitName = "Engineering Department",
        string jobCode = "JOB-ENG-MGR",
        string jobGradeCode = "G5",
        int jobGradeRankOrder = 50,
        string? jobLevelCategory = "MANAGEMENT",
        decimal? approvalLimitAmount = 100_000m,
        string? approvalLimitCurrency = "THB",
        bool isPrimaryLeadership = true,
        bool isActive = true,
        DateTimeOffset? validFrom = null,
        DateTimeOffset? validTo = null)
    {
        return new ChainOfCommandPositionDto(
            RefId: refId,
            Code: code,
            Title: title,
            InternationalTitle: internationalTitle,
            LegalEntityRefId: legalEntityRefId,
            OrgUnitRefId: orgUnitRefId,
            OrgUnitCode: orgUnitCode,
            OrgUnitName: orgUnitName,
            JobCode: jobCode,
            JobGradeCode: jobGradeCode,
            JobGradeRankOrder: jobGradeRankOrder,
            JobLevelCategory: jobLevelCategory,
            ApprovalLimitAmount: approvalLimitAmount,
            ApprovalLimitCurrency: approvalLimitCurrency,
            IsPrimaryLeadership: isPrimaryLeadership,
            IsActive: isActive,
            ValidFrom: validFrom ?? new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
            ValidTo: validTo ?? new DateTimeOffset(2027, 12, 31, 23, 59, 59, TimeSpan.Zero));
    }
}
