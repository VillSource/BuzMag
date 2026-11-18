using Elsa.Workflows.Management.Options;
using Elsa.Workflows.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSubstitute;
using Villsource.FSH.Modules.Organization.Contracts.Constants;
using Villsource.FSH.Modules.Organization.Contracts.Services;
using Villsource.FSH.Modules.Organization.Contracts.Services.Dtos;
using Villsource.FSH.Modules.Organization.Workflows.Activities.ChainOfCommand;
using Villsource.FSH.Modules.Organization.Workflows.Activities.ChainOfCommand.Base;
using Villsource.FSH.Modules.Organization.Workflows.Constants;

namespace Villsource.FSH.Modules.Organization.Tests.Workflows;

public sealed class ChainOfCommandActivitiesTests
{
    [Fact]
    public async Task OrganizationModule_Should_Register_All_ChainOfCommand_Activities_And_VariableTypes()
    {
        await using var sp = WorkflowTestHost.BuildServiceProvider();
        var registry = await WorkflowTestHost.PopulateRegistryAsync(sp);

        Type[] expectedActivities =
        [
            typeof(ChainOfCommandPositionFieldSelector),
            typeof(ResolveDirectManager),
            typeof(ResolveDottedLineManager),
            typeof(ResolveOuManagerFromPosition),
            typeof(ResolveOuManagerFromOrgUnit),
            typeof(ResolveEffectiveDelegates),
            typeof(ResolveActiveDelegators),
            typeof(ResolveEffectiveManager),
            typeof(ResolveReportingChain),
            typeof(ResolveOuLeadershipChain),
            typeof(ResolveApprovalChain),
            typeof(ResolveManagerByApprovalLimit),
            typeof(ResolveManagerByMinJobGrade),
            typeof(GetDirectReports),
            typeof(GetSubordinateSubtree),
            typeof(IsInChainOfCommand),
        ];

        foreach (var activityType in expectedActivities)
        {
            var descriptor = registry.Find(d => d.ClrType == activityType);
            descriptor.ShouldNotBeNull($"Expected '{activityType.Name}' to be registered in Elsa ActivityRegistry.");
            descriptor.Category.ShouldBe(OrganizationActivityConstants.Category);
        }

        Type[] baseTypes =
        [
            typeof(PositionResolutionActivityBase),
            typeof(OuManagerResolutionActivityBase),
            typeof(EffectiveManagerResolutionActivityBase),
            typeof(PositionListResolutionActivityBase),
            typeof(DelegationListResolutionActivityBase),
        ];

        foreach (var baseType in baseTypes)
        {
            registry.Find(d => d.ClrType == baseType).ShouldBeNull();
        }

        var effectiveMgrDescriptor = registry.Find(d => d.ClrType == typeof(ResolveEffectiveManager))!;
        effectiveMgrDescriptor.Outputs.Select(o => o.Name).ShouldContain(nameof(ResolveEffectiveManager.EffectiveApproverPosition));
        effectiveMgrDescriptor.Outputs.Select(o => o.Name).ShouldContain(nameof(ResolveEffectiveManager.NominalManagerPosition));
        effectiveMgrDescriptor.Outputs.Select(o => o.Name).ShouldContain(nameof(ResolveEffectiveManager.IsDelegated));
        effectiveMgrDescriptor.Outputs.Select(o => o.Name).ShouldContain(nameof(ResolveEffectiveManager.IsFound));

        var minGradeDescriptor = registry.Find(d => d.ClrType == typeof(ResolveManagerByMinJobGrade))!;
        var jobLevelInput = minGradeDescriptor.Inputs.Single(i => i.Name == nameof(ResolveManagerByMinJobGrade.MinJobLevelCategory));
        var jobLevelItems = WorkflowTestHost.GetDropDownSelectList(jobLevelInput)
            .Items
            .Select(i => i.Value)
            .ToArray();
        jobLevelItems.ShouldContain(string.Empty);
        foreach (var category in JobLevelCategory.Items)
        {
            jobLevelItems.ShouldContain(category.Key);
        }

        var delegatesDescriptor = registry.Find(d => d.ClrType == typeof(ResolveEffectiveDelegates))!;
        var delegationTypeInput = delegatesDescriptor.Inputs.Single(i => i.Name == nameof(ResolveEffectiveDelegates.DelegationTypeKey));
        WorkflowTestHost.GetDropDownSelectList(delegationTypeInput)
            .Items
            .Select(i => i.Value)
            .ShouldBe([
                string.Empty,
                OrganizationActivityConstants.DelegationActingFull,
                OrganizationActivityConstants.DelegationApprovalOnly,
                OrganizationActivityConstants.DelegationSpecificDocuments
            ]);

        var managementOptions = sp.GetRequiredService<IOptions<ManagementOptions>>().Value;
        var registeredVarTypes = managementOptions.VariableDescriptors.Select(v => v.Type).ToHashSet();
        registeredVarTypes.ShouldContain(typeof(ChainOfCommandPositionDto));
        registeredVarTypes.ShouldContain(typeof(OuManagerResolutionDto));
        registeredVarTypes.ShouldContain(typeof(EffectiveManagerResolutionDto));
        registeredVarTypes.ShouldContain(typeof(ResolvedDelegationDto));
        registeredVarTypes.ShouldContain(typeof(OuLeadershipStepDto));
        registeredVarTypes.ShouldContain(typeof(ManagerResolutionSource));
    }

    [Fact]
    public async Task ResolveDirectManager_Should_Call_Service_And_Set_Outputs()
    {
        var manager = WorkflowTestHost.CreateSamplePosition(refId: "POS-MGR");
        var asOf = new DateTimeOffset(2026, 11, 18, 12, 0, 0, TimeSpan.Zero);
        var service = Substitute.For<IChainOfCommandService>();
        service.ResolveDirectManagerAsync("POS-EMP", true, true, asOf, Arg.Any<CancellationToken>())
            .Returns(manager);
        service.ResolveDirectManagerAsync("POS-CEO", false, false, null, Arg.Any<CancellationToken>())
            .Returns((ChainOfCommandPositionDto?)null);

        await using var sp = WorkflowTestHost.BuildServiceProvider(service);

        var act = new ResolveDirectManager
        {
            PositionRefId = new("POS-EMP"),
            ClimbToFirstActive = new(true),
            IncludeInactive = new(true),
            AsOfUtc = new(asOf)
        };
        var (res, result) = await WorkflowTestHost.RunCodeActivityAsync(sp, act);
        result.ShouldBe(manager);
        WorkflowTestHost.GetOutput<bool>(res, act, nameof(ResolveDirectManager.IsFound)).ShouldBeTrue();

        var nullAct = new ResolveDirectManager
        {
            PositionRefId = new("POS-CEO")
        };
        var (nullRes, nullResult) = await WorkflowTestHost.RunCodeActivityAsync(sp, nullAct);
        nullResult.ShouldBeNull();
        WorkflowTestHost.GetOutput<bool>(nullRes, nullAct, nameof(ResolveDirectManager.IsFound)).ShouldBeFalse();
    }

    [Fact]
    public async Task ResolveDottedLineManager_Should_Call_Service_And_Set_Outputs()
    {
        var dotted = WorkflowTestHost.CreateSamplePosition(refId: "POS-DOT");
        var service = Substitute.For<IChainOfCommandService>();
        service.ResolveDottedLineManagerAsync("POS-EMP", true, null, Arg.Any<CancellationToken>())
            .Returns(dotted);

        await using var sp = WorkflowTestHost.BuildServiceProvider(service);
        var act = new ResolveDottedLineManager
        {
            PositionRefId = new("POS-EMP"),
            IncludeInactive = new(true)
        };

        var (res, result) = await WorkflowTestHost.RunCodeActivityAsync(sp, act);

        result.ShouldBe(dotted);
        WorkflowTestHost.GetOutput<bool>(res, act, nameof(ResolveDottedLineManager.IsFound)).ShouldBeTrue();
    }

    [Fact]
    public async Task ResolveOuManagers_FromPosition_And_FromOrgUnit_Should_Set_All_Outputs()
    {
        var mgr = WorkflowTestHost.CreateSamplePosition(refId: "POS-OU-MGR");
        var ouDto = new OuManagerResolutionDto("OU-1", "OU-0", "DEPT", "Department", 1, 1, mgr);
        var service = Substitute.For<IChainOfCommandService>();
        service.ResolveOuManagerFromPositionAsync("POS-1", true, false, null, Arg.Any<CancellationToken>())
            .Returns(ouDto);
        service.ResolveOuManagerFromOrgUnitAsync("OU-1", true, true, null, Arg.Any<CancellationToken>())
            .Returns(ouDto);
        service.ResolveOuManagerFromOrgUnitAsync("OU-EMPTY", false, false, null, Arg.Any<CancellationToken>())
            .Returns((OuManagerResolutionDto?)null);

        await using var sp = WorkflowTestHost.BuildServiceProvider(service);

        var actFromPos = new ResolveOuManagerFromPosition { PositionRefId = new("POS-1") };
        var (resFromPos, dtoFromPos) = await WorkflowTestHost.RunCodeActivityAsync(sp, actFromPos);

        dtoFromPos.ShouldBe(ouDto);
        WorkflowTestHost.GetOutput<ChainOfCommandPositionDto?>(resFromPos, actFromPos, nameof(ResolveOuManagerFromPosition.ManagerPosition)).ShouldBe(mgr);
        WorkflowTestHost.GetOutput<string?>(resFromPos, actFromPos, nameof(ResolveOuManagerFromPosition.ResolvedOrgUnitRefId)).ShouldBe("OU-0");
        WorkflowTestHost.GetOutput<string?>(resFromPos, actFromPos, nameof(ResolveOuManagerFromPosition.ResolvedOrgUnitCode)).ShouldBe("DEPT");
        WorkflowTestHost.GetOutput<string?>(resFromPos, actFromPos, nameof(ResolveOuManagerFromPosition.ResolvedOrgUnitName)).ShouldBe("Department");
        WorkflowTestHost.GetOutput<int?>(resFromPos, actFromPos, nameof(ResolveOuManagerFromPosition.ResolvedOrgUnitDepth)).ShouldBe(1);
        WorkflowTestHost.GetOutput<int?>(resFromPos, actFromPos, nameof(ResolveOuManagerFromPosition.StepsCrawledUp)).ShouldBe(1);
        WorkflowTestHost.GetOutput<bool>(resFromPos, actFromPos, nameof(ResolveOuManagerFromPosition.IsFound)).ShouldBeTrue();

        var actFromOu = new ResolveOuManagerFromOrgUnit
        {
            OrgUnitRefId = new("OU-1"),
            StartFromParent = new(true),
            IncludeInactiveOu = new(true)
        };
        var (resFromOu, _) = await WorkflowTestHost.RunCodeActivityAsync(sp, actFromOu);
        WorkflowTestHost.GetOutput<ChainOfCommandPositionDto?>(resFromOu, actFromOu, nameof(ResolveOuManagerFromOrgUnit.ManagerPosition)).ShouldBe(mgr);

        var emptyAct = new ResolveOuManagerFromOrgUnit { OrgUnitRefId = new("OU-EMPTY") };
        var (resEmpty, emptyDto) = await WorkflowTestHost.RunCodeActivityAsync(sp, emptyAct);
        emptyDto.ShouldBeNull();
        WorkflowTestHost.GetOutput<bool>(resEmpty, emptyAct, nameof(ResolveOuManagerFromOrgUnit.IsFound)).ShouldBeFalse();
        WorkflowTestHost.GetOutput<ChainOfCommandPositionDto?>(resEmpty, emptyAct, nameof(ResolveOuManagerFromOrgUnit.ManagerPosition)).ShouldBeNull();
    }

    [Fact]
    public async Task Delegation_Resolvers_Should_Normalize_Empty_Strings_And_Set_Outputs()
    {
        var delegator = WorkflowTestHost.CreateSamplePosition(refId: "POS-MGR");
        var delegatePos = WorkflowTestHost.CreateSamplePosition(refId: "POS-ACTING");
        var delegation = new ResolvedDelegationDto(
            "DEL-1",
            OrganizationActivityConstants.DelegationActingFull,
            null,
            "Leave",
            DateTimeOffset.UtcNow.AddDays(-1),
            DateTimeOffset.UtcNow.AddDays(5),
            delegator,
            delegatePos);

        var service = Substitute.For<IChainOfCommandService>();
        service.ResolveEffectiveDelegatesAsync("POS-MGR", null, null, null, Arg.Any<CancellationToken>())
            .Returns([delegation]);
        service.ResolveActiveDelegatorsAsync("POS-ACTING", "PR", OrganizationActivityConstants.DelegationSpecificDocuments, null, Arg.Any<CancellationToken>())
            .Returns([delegation]);

        await using var sp = WorkflowTestHost.BuildServiceProvider(service);

        var effAct = new ResolveEffectiveDelegates
        {
            DelegatorPositionRefId = new("POS-MGR"),
            DocumentTypeCode = new("   "),
            DelegationTypeKey = new(string.Empty)
        };
        var (effRes, effList) = await WorkflowTestHost.RunCodeActivityAsync(sp, effAct);
        effList.ShouldNotBeNull().Count.ShouldBe(1);
        WorkflowTestHost.GetOutput<int>(effRes, effAct, nameof(ResolveEffectiveDelegates.Count)).ShouldBe(1);
        WorkflowTestHost.GetOutput<ResolvedDelegationDto?>(effRes, effAct, nameof(ResolveEffectiveDelegates.FirstDelegation)).ShouldBe(delegation);
        WorkflowTestHost.GetOutput<ChainOfCommandPositionDto?>(effRes, effAct, nameof(ResolveEffectiveDelegates.FirstDelegatePosition)).ShouldBe(delegatePos);

        var actAct = new ResolveActiveDelegators
        {
            DelegatePositionRefId = new("POS-ACTING"),
            DocumentTypeCode = new("PR"),
            DelegationTypeKey = new(OrganizationActivityConstants.DelegationSpecificDocuments)
        };
        var (actRes, _) = await WorkflowTestHost.RunCodeActivityAsync(sp, actAct);
        WorkflowTestHost.GetOutput<ChainOfCommandPositionDto?>(actRes, actAct, nameof(ResolveActiveDelegators.FirstDelegatorPosition)).ShouldBe(delegator);
    }

    [Fact]
    public async Task EffectiveManager_ApprovalLimit_And_MinJobGrade_Resolvers_Should_Set_All_Outputs()
    {
        var nominal = WorkflowTestHost.CreateSamplePosition(refId: "POS-NOMINAL");
        var acting = WorkflowTestHost.CreateSamplePosition(refId: "POS-ACTING");
        var delegation = new ResolvedDelegationDto(
            "DEL-1", "ACTING_FULL", null, "Vacation",
            DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(3),
            nominal, acting);
        var resolution = new EffectiveManagerResolutionDto(
            1, ManagerResolutionSource.OrganizationUnitHierarchy, nominal, acting, true, delegation, "OU-DEPT");

        var service = Substitute.For<IChainOfCommandService>();
        service.ResolveEffectiveManagerAsync("POS-EMP", "PR", true, true, null, Arg.Any<CancellationToken>())
            .Returns(resolution);
        service.ResolveManagerByApprovalLimitAsync("POS-EMP", 50_000m, "THB", null, true, null, Arg.Any<CancellationToken>())
            .Returns(resolution);
        service.ResolveManagerByMinJobGradeAsync("POS-EMP", 30, JobLevelCategory.Management.Key, null, true, null, Arg.Any<CancellationToken>())
            .Returns(resolution);

        await using var sp = WorkflowTestHost.BuildServiceProvider(service);

        var act1 = new ResolveEffectiveManager
        {
            PositionRefId = new("POS-EMP"),
            DocumentTypeCode = new("PR")
        };
        var (res1, effDto) = await WorkflowTestHost.RunCodeActivityAsync(sp, act1);

        effDto.ShouldBe(resolution);
        WorkflowTestHost.GetOutput<ChainOfCommandPositionDto?>(res1, act1, nameof(ResolveEffectiveManager.EffectiveApproverPosition)).ShouldBe(acting);
        WorkflowTestHost.GetOutput<ChainOfCommandPositionDto?>(res1, act1, nameof(ResolveEffectiveManager.NominalManagerPosition)).ShouldBe(nominal);
        WorkflowTestHost.GetOutput<bool>(res1, act1, nameof(ResolveEffectiveManager.IsDelegated)).ShouldBeTrue();
        WorkflowTestHost.GetOutput<ManagerResolutionSource?>(res1, act1, nameof(ResolveEffectiveManager.ResolutionSource)).ShouldBe(ManagerResolutionSource.OrganizationUnitHierarchy);
        WorkflowTestHost.GetOutput<ResolvedDelegationDto?>(res1, act1, nameof(ResolveEffectiveManager.ActiveDelegation)).ShouldBe(delegation);
        WorkflowTestHost.GetOutput<string?>(res1, act1, nameof(ResolveEffectiveManager.ResolvedFromOrgUnitRefId)).ShouldBe("OU-DEPT");
        WorkflowTestHost.GetOutput<bool>(res1, act1, nameof(ResolveEffectiveManager.IsFound)).ShouldBeTrue();

        var act2 = new ResolveManagerByApprovalLimit
        {
            PositionRefId = new("POS-EMP"),
            RequiredAmount = new(50_000m),
            Currency = new("THB")
        };
        var (res2, _) = await WorkflowTestHost.RunCodeActivityAsync(sp, act2);
        WorkflowTestHost.GetOutput<ChainOfCommandPositionDto?>(res2, act2, nameof(ResolveManagerByApprovalLimit.EffectiveApproverPosition)).ShouldBe(acting);

        var act3 = new ResolveManagerByMinJobGrade
        {
            PositionRefId = new("POS-EMP"),
            MinRankOrder = new(30),
            MinJobLevelCategory = new(JobLevelCategory.Management.Key)
        };
        var (res3, _) = await WorkflowTestHost.RunCodeActivityAsync(sp, act3);
        WorkflowTestHost.GetOutput<ChainOfCommandPositionDto?>(res3, act3, nameof(ResolveManagerByMinJobGrade.EffectiveApproverPosition)).ShouldBe(acting);
    }

    [Fact]
    public async Task Chain_List_Subtree_And_IsInChain_Activities_Should_Call_Service_And_Set_Outputs()
    {
        var pos1 = WorkflowTestHost.CreateSamplePosition(refId: "POS-1");
        var pos2 = WorkflowTestHost.CreateSamplePosition(refId: "POS-2");
        var ouStep = new OuLeadershipStepDto(0, 1, "OU-1", "ENG", "Engineering", "Engineering", true, pos1);
        var approvalStep = new EffectiveManagerResolutionDto(1, ManagerResolutionSource.DirectReporting, pos1, pos1, false, null);

        var service = Substitute.For<IChainOfCommandService>();
        service.ResolveReportingChainAsync("POS-EMP", 15, false, null, Arg.Any<CancellationToken>())
            .Returns([pos1, pos2]);
        service.ResolveOuLeadershipChainAsync("OU-1", true, false, null, Arg.Any<CancellationToken>())
            .Returns([ouStep]);
        service.ResolveApprovalChainAsync("POS-EMP", null, 15, true, null, Arg.Any<CancellationToken>())
            .Returns([approvalStep]);
        service.GetDirectReportsAsync("POS-MGR", false, null, Arg.Any<CancellationToken>())
            .Returns([pos2]);
        service.GetSubordinateSubtreeAsync("POS-MGR", 10, false, null, Arg.Any<CancellationToken>())
            .Returns([pos1, pos2]);
        service.IsInChainOfCommandAsync("POS-MGR", "POS-EMP", false, true, true, null, Arg.Any<CancellationToken>())
            .Returns(true);

        await using var sp = WorkflowTestHost.BuildServiceProvider(service);

        var reportingAct = new ResolveReportingChain { PositionRefId = new("POS-EMP") };
        var (reportingRes, reportingList) = await WorkflowTestHost.RunCodeActivityAsync(sp, reportingAct);
        reportingList.ShouldBe([pos1, pos2]);
        WorkflowTestHost.GetOutput<int>(reportingRes, reportingAct, nameof(ResolveReportingChain.Count)).ShouldBe(2);
        WorkflowTestHost.GetOutput<ChainOfCommandPositionDto?>(reportingRes, reportingAct, nameof(ResolveReportingChain.FirstPosition)).ShouldBe(pos1);

        var ouAct = new ResolveOuLeadershipChain
        {
            OrgUnitRefId = new("OU-1"),
            OnlyUnitsWithManager = new(true)
        };
        var (ouRes, _) = await WorkflowTestHost.RunCodeActivityAsync(sp, ouAct);
        WorkflowTestHost.GetOutput<int>(ouRes, ouAct, nameof(ResolveOuLeadershipChain.Count)).ShouldBe(1);
        WorkflowTestHost.GetOutput<OuLeadershipStepDto?>(ouRes, ouAct, nameof(ResolveOuLeadershipChain.FirstStep)).ShouldBe(ouStep);
        WorkflowTestHost.GetOutput<ChainOfCommandPositionDto?>(ouRes, ouAct, nameof(ResolveOuLeadershipChain.FirstManagerPosition)).ShouldBe(pos1);

        var appAct = new ResolveApprovalChain { PositionRefId = new("POS-EMP") };
        var (appRes, _) = await WorkflowTestHost.RunCodeActivityAsync(sp, appAct);
        WorkflowTestHost.GetOutput<int>(appRes, appAct, nameof(ResolveApprovalChain.Count)).ShouldBe(1);
        WorkflowTestHost.GetOutput<EffectiveManagerResolutionDto?>(appRes, appAct, nameof(ResolveApprovalChain.FirstStep)).ShouldBe(approvalStep);
        WorkflowTestHost.GetOutput<ChainOfCommandPositionDto?>(appRes, appAct, nameof(ResolveApprovalChain.FirstApproverPosition)).ShouldBe(pos1);

        var drAct = new GetDirectReports { ManagerPositionRefId = new("POS-MGR") };
        var (drRes, _) = await WorkflowTestHost.RunCodeActivityAsync(sp, drAct);
        WorkflowTestHost.GetOutput<int>(drRes, drAct, nameof(GetDirectReports.Count)).ShouldBe(1);
        WorkflowTestHost.GetOutput<ChainOfCommandPositionDto?>(drRes, drAct, nameof(GetDirectReports.FirstPosition)).ShouldBe(pos2);

        var subAct = new GetSubordinateSubtree { ManagerPositionRefId = new("POS-MGR") };
        var (subRes, _) = await WorkflowTestHost.RunCodeActivityAsync(sp, subAct);
        WorkflowTestHost.GetOutput<int>(subRes, subAct, nameof(GetSubordinateSubtree.Count)).ShouldBe(2);
        WorkflowTestHost.GetOutput<ChainOfCommandPositionDto?>(subRes, subAct, nameof(GetSubordinateSubtree.FirstPosition)).ShouldBe(pos1);

        var (_, isInChain) = await WorkflowTestHost.RunCodeActivityAsync(
            sp,
            new IsInChainOfCommand
            {
                SupervisorPositionRefId = new("POS-MGR"),
                SubordinatePositionRefId = new("POS-EMP")
            });
        isInChain.ShouldBeTrue();
    }
}
