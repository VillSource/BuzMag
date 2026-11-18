using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;
using FSH.Framework.Core.Exceptions;
using FSH.Framework.Shared.Multitenancy;
using FSH.Framework.Shared.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NSubstitute;
using Villsource.FSH.Modules.Organization.Contracts.Constants;
using Villsource.FSH.Modules.Organization.Contracts.Services.Dtos;
using Villsource.FSH.Modules.Organization.Data;
using Villsource.FSH.Modules.Organization.Domain.JobGrades;
using Villsource.FSH.Modules.Organization.Domain.Jobs;
using Villsource.FSH.Modules.Organization.Domain.LegalEntities;
using Villsource.FSH.Modules.Organization.Domain.OrganizationUnits;
using Villsource.FSH.Modules.Organization.Domain.OrganizationUnitTypes;
using Villsource.FSH.Modules.Organization.Domain.PositionDelegations;
using Villsource.FSH.Modules.Organization.Domain.Positions;
using Villsource.FSH.Modules.Organization.Services;

namespace Villsource.FSH.Modules.Organization.Tests.Services;

public sealed class ChainOfCommandServiceTests
{
    private const string TenantId = "tenant-org";
    private readonly TestTenantAccessor _tenantAccessor = new();

    #region Scenario 1: Direct Manager

    [Fact]
    public async Task ResolveDirectManagerAsync_Should_Return_Active_Direct_Manager()
    {
        await using var db = CreateContext();
        var data = await SeedHierarchyAsync(db);
        var sut = new ChainOfCommandService(db);

        var manager = await sut.ResolveDirectManagerAsync(data.Staff.ReferenceId, asOfUtc: data.Now);

        manager.ShouldNotBeNull();
        manager.RefId.ShouldBe(data.TeamLead.ReferenceId);
        manager.Code.ShouldBe("POS-LEAD");
        manager.JobGradeCode.ShouldBe("G2");
        manager.JobLevelCategory.ShouldBe(JobLevelCategory.Supervisory.Key);
        manager.ApprovalLimitAmount.ShouldBe(10_000m);
        manager.ApprovalLimitCurrency.ShouldBe("THB");
    }

    [Fact]
    public async Task ResolveDirectManagerAsync_Should_Climb_Past_Inactive_Manager_When_Requested()
    {
        await using var db = CreateContext();
        var data = await SeedHierarchyAsync(db, teamLeadActive: false);
        var sut = new ChainOfCommandService(db);

        var immediateOnly = await sut.ResolveDirectManagerAsync(
            data.Staff.ReferenceId,
            climbToFirstActive: false,
            asOfUtc: data.Now);
        immediateOnly.ShouldBeNull();

        var includeInactive = await sut.ResolveDirectManagerAsync(
            data.Staff.ReferenceId,
            climbToFirstActive: false,
            includeInactive: true,
            asOfUtc: data.Now);
        includeInactive.ShouldNotBeNull();
        includeInactive.RefId.ShouldBe(data.TeamLead.ReferenceId);

        var climbed = await sut.ResolveDirectManagerAsync(
            data.Staff.ReferenceId,
            climbToFirstActive: true,
            asOfUtc: data.Now);
        climbed.ShouldNotBeNull();
        climbed.RefId.ShouldBe(data.DeptManager.ReferenceId);
    }

    [Fact]
    public async Task ResolveDirectManagerAsync_Should_Throw_NotFoundException_When_Position_Missing()
    {
        await using var db = CreateContext();
        SetTenant(TenantId);
        var sut = new ChainOfCommandService(db);

        await Should.ThrowAsync<NotFoundException>(
            () => sut.ResolveDirectManagerAsync("NON_EXISTENT"));
    }

    #endregion

    #region Scenario 2: Dotted-Line Manager

    [Fact]
    public async Task ResolveDottedLineManagerAsync_Should_Return_DottedLine_Manager_And_Respect_Active_Flag()
    {
        await using var db = CreateContext();
        var data = await SeedHierarchyAsync(db);
        var sut = new ChainOfCommandService(db);

        var dotted = await sut.ResolveDottedLineManagerAsync(data.Staff.ReferenceId, asOfUtc: data.Now);
        dotted.ShouldNotBeNull();
        dotted.RefId.ShouldBe(data.DottedDirector.ReferenceId);

        var inactiveDotted = await sut.ResolveDottedLineManagerAsync(data.TeamLead.ReferenceId, asOfUtc: data.Now);
        inactiveDotted.ShouldBeNull();
    }

    #endregion

    #region Scenario 3: OU Manager Crawl-Up

    [Fact]
    public async Task ResolveOuManagerFromPositionAsync_Should_Crawl_Up_To_First_Active_Ou_Manager()
    {
        await using var db = CreateContext();
        var data = await SeedHierarchyAsync(db);
        var sut = new ChainOfCommandService(db);

        // Staff is in TeamOu (no ManagerPosition assigned) -> crawls up 1 level to DeptOu (DeptManager).
        var result = await sut.ResolveOuManagerFromPositionAsync(data.Staff.ReferenceId, asOfUtc: data.Now);

        result.ShouldNotBeNull();
        result.OriginOrgUnitRefId.ShouldBe(data.TeamOu.ReferenceId);
        result.ResolvedOrgUnitRefId.ShouldBe(data.DeptOu.ReferenceId);
        result.StepsCrawledUp.ShouldBe(1);
        result.ManagerPosition.RefId.ShouldBe(data.DeptManager.ReferenceId);
    }

    [Fact]
    public async Task ResolveOuManagerFromPositionAsync_Should_Exclude_Self_And_Climb_To_Parent_Ou()
    {
        await using var db = CreateContext();
        var data = await SeedHierarchyAsync(db);
        var sut = new ChainOfCommandService(db);

        // DeptManager IS the ManagerPosition of DeptOu -> excludeSelf: true climbs 1 level to RootOu (Ceo).
        var climbed = await sut.ResolveOuManagerFromPositionAsync(
            data.DeptManager.ReferenceId,
            excludeSelf: true,
            asOfUtc: data.Now);

        climbed.ShouldNotBeNull();
        climbed.ResolvedOrgUnitRefId.ShouldBe(data.RootOu.ReferenceId);
        climbed.StepsCrawledUp.ShouldBe(1);
        climbed.ManagerPosition.RefId.ShouldBe(data.Ceo.ReferenceId);

        // excludeSelf: false returns DeptOu itself.
        var self = await sut.ResolveOuManagerFromPositionAsync(
            data.DeptManager.ReferenceId,
            excludeSelf: false,
            asOfUtc: data.Now);

        self.ShouldNotBeNull();
        self.ResolvedOrgUnitRefId.ShouldBe(data.DeptOu.ReferenceId);
        self.StepsCrawledUp.ShouldBe(0);
        self.ManagerPosition.RefId.ShouldBe(data.DeptManager.ReferenceId);
    }

    [Fact]
    public async Task ResolveOuManagerFromOrgUnitAsync_Should_Support_StartFromParent()
    {
        await using var db = CreateContext();
        var data = await SeedHierarchyAsync(db);
        var sut = new ChainOfCommandService(db);

        var current = await sut.ResolveOuManagerFromOrgUnitAsync(
            data.DeptOu.ReferenceId,
            startFromParent: false,
            asOfUtc: data.Now);
        current.ShouldNotBeNull();
        current.ManagerPosition.RefId.ShouldBe(data.DeptManager.ReferenceId);
        current.StepsCrawledUp.ShouldBe(0);

        var parent = await sut.ResolveOuManagerFromOrgUnitAsync(
            data.DeptOu.ReferenceId,
            startFromParent: true,
            asOfUtc: data.Now);
        parent.ShouldNotBeNull();
        parent.ManagerPosition.RefId.ShouldBe(data.Ceo.ReferenceId);
        parent.StepsCrawledUp.ShouldBe(1);
    }

    #endregion

    #region Scenario 4: Delegation Both Directions

    [Fact]
    public async Task ResolveEffectiveDelegatesAsync_And_ResolveActiveDelegatorsAsync_Should_Work_In_Both_Directions()
    {
        await using var db = CreateContext();
        var data = await SeedHierarchyAsync(db);

        var specificDel = PositionDelegation.Create(
            data.LegalEntity.Id,
            data.DeptManager.Id,
            data.TeamLead.Id,
            DelegationType.SpecificDocuments,
            data.Now.AddDays(-2),
            data.Now.AddDays(5),
            documentTypeScope: "PR,PO",
            reason: "Procurement cover");

        var fullDel = PositionDelegation.Create(
            data.LegalEntity.Id,
            data.DeptManager.Id,
            data.BackupManager.Id,
            DelegationType.ActingFull,
            data.Now.AddDays(-1),
            data.Now.AddDays(5),
            reason: "Annual leave");

        var revokedDel = PositionDelegation.Create(
            data.LegalEntity.Id,
            data.DeptManager.Id,
            data.DottedDirector.Id,
            DelegationType.ApprovalOnly,
            data.Now.AddDays(-1),
            data.Now.AddDays(5));
        revokedDel.Revoke("admin", "Cancelled");

        db.PositionDelegations.AddRange(specificDel, fullDel, revokedDel);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var sut = new ChainOfCommandService(db);

        // Direction 1: Delegates acting FOR DeptManager (filtered to PR)
        var delegates = await sut.ResolveEffectiveDelegatesAsync(
            data.DeptManager.ReferenceId,
            documentTypeCode: "PR",
            asOfUtc: data.Now);

        delegates.Count.ShouldBe(2);
        delegates[0].DelegationTypeKey.ShouldBe(DelegationType.ActingFull.Key);
        delegates[0].DelegatePosition.RefId.ShouldBe(data.BackupManager.ReferenceId);
        delegates[1].DelegationTypeKey.ShouldBe(DelegationType.SpecificDocuments.Key);
        delegates[1].DelegatePosition.RefId.ShouldBe(data.TeamLead.ReferenceId);

        // Direction 1 with non-matching doc scope ("LEAVE") excludes SPECIFIC_DOCUMENTS("PR,PO")
        var leaveDelegates = await sut.ResolveEffectiveDelegatesAsync(
            data.DeptManager.ReferenceId,
            documentTypeCode: "LEAVE",
            asOfUtc: data.Now);
        leaveDelegates.Count.ShouldBe(1);
        leaveDelegates[0].DelegatePosition.RefId.ShouldBe(data.BackupManager.ReferenceId);

        // Direction 2: Delegators that TeamLead is acting ON BEHALF OF
        var delegators = await sut.ResolveActiveDelegatorsAsync(
            data.TeamLead.ReferenceId,
            documentTypeCode: "PR",
            asOfUtc: data.Now);
        delegators.Count.ShouldBe(1);
        delegators[0].DelegatorPosition.RefId.ShouldBe(data.DeptManager.ReferenceId);
        delegators[0].DelegatePosition.RefId.ShouldBe(data.TeamLead.ReferenceId);
    }

    #endregion

    #region Scenario 5 & 6: Effective Manager & Upward Chains

    [Fact]
    public async Task ResolveEffectiveManagerAsync_Should_Fallback_To_Ou_And_Substitute_Active_Delegate()
    {
        await using var db = CreateContext();
        var data = await SeedHierarchyAsync(db);

        // Create an orphan position without ReportsToPositionId in TeamOu
        var orphan = Position.Create(
            data.LegalEntity.Id,
            data.TeamOu.Id,
            data.Job.Id,
            data.Grade1.Id,
            "POS-ORPHAN",
            "Orphan Staff",
            "Orphan Staff",
            reportsToPositionId: null);
        db.Positions.Add(orphan);

        // DeptManager delegates approval to BackupManager for PR documents
        var delegation = PositionDelegation.Create(
            data.LegalEntity.Id,
            data.DeptManager.Id,
            data.BackupManager.Id,
            DelegationType.SpecificDocuments,
            data.Now.AddDays(-1),
            data.Now.AddDays(3),
            documentTypeScope: "PR");
        db.PositionDelegations.Add(delegation);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var sut = new ChainOfCommandService(db);

        // With matching documentTypeCode = "PR", SPECIFIC_DOCUMENTS substitutes BackupManager
        var withDoc = await sut.ResolveEffectiveManagerAsync(
            orphan.ReferenceId,
            documentTypeCode: "PR",
            fallbackToOuManager: true,
            asOfUtc: data.Now);

        withDoc.ShouldNotBeNull();
        withDoc.ResolutionSource.ShouldBe(ManagerResolutionSource.OrganizationUnitHierarchy);
        withDoc.ResolvedFromOrgUnitRefId.ShouldBe(data.DeptOu.ReferenceId);
        withDoc.NominalManagerPosition.RefId.ShouldBe(data.DeptManager.ReferenceId);
        withDoc.IsDelegated.ShouldBeTrue();
        withDoc.EffectiveApproverPosition.RefId.ShouldBe(data.BackupManager.ReferenceId);

        // Without documentTypeCode, SPECIFIC_DOCUMENTS should NOT auto-substitute
        var withoutDoc = await sut.ResolveEffectiveManagerAsync(
            orphan.ReferenceId,
            documentTypeCode: null,
            fallbackToOuManager: true,
            asOfUtc: data.Now);

        withoutDoc.ShouldNotBeNull();
        withoutDoc.IsDelegated.ShouldBeFalse();
        withoutDoc.EffectiveApproverPosition.RefId.ShouldBe(data.DeptManager.ReferenceId);
    }

    [Fact]
    public async Task ResolveReportingChainAsync_And_ResolveApprovalChainAsync_Should_Return_Full_Upward_Chain()
    {
        await using var db = CreateContext();
        var data = await SeedHierarchyAsync(db);

        var delegation = PositionDelegation.Create(
            data.LegalEntity.Id,
            data.DeptManager.Id,
            data.BackupManager.Id,
            DelegationType.ApprovalOnly,
            data.Now.AddDays(-1),
            data.Now.AddDays(5));
        db.PositionDelegations.Add(delegation);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var sut = new ChainOfCommandService(db);

        var chain = await sut.ResolveReportingChainAsync(data.Staff.ReferenceId, asOfUtc: data.Now);
        chain.Select(c => c.RefId).ShouldBe([
            data.TeamLead.ReferenceId,
            data.DeptManager.ReferenceId,
            data.Ceo.ReferenceId
        ]);

        var approvalChain = await sut.ResolveApprovalChainAsync(data.Staff.ReferenceId, asOfUtc: data.Now);
        approvalChain.Count.ShouldBe(3);
        approvalChain[0].StepOrder.ShouldBe(1);
        approvalChain[0].NominalManagerPosition.RefId.ShouldBe(data.TeamLead.ReferenceId);
        approvalChain[0].IsDelegated.ShouldBeFalse();

        approvalChain[1].StepOrder.ShouldBe(2);
        approvalChain[1].NominalManagerPosition.RefId.ShouldBe(data.DeptManager.ReferenceId);
        approvalChain[1].IsDelegated.ShouldBeTrue();
        approvalChain[1].EffectiveApproverPosition.RefId.ShouldBe(data.BackupManager.ReferenceId);

        approvalChain[2].StepOrder.ShouldBe(3);
        approvalChain[2].NominalManagerPosition.RefId.ShouldBe(data.Ceo.ReferenceId);
    }

    [Fact]
    public async Task ResolveOuLeadershipChainAsync_Should_Return_BottomUp_Ou_Chain()
    {
        await using var db = CreateContext();
        var data = await SeedHierarchyAsync(db);
        var sut = new ChainOfCommandService(db);

        var allSteps = await sut.ResolveOuLeadershipChainAsync(
            data.TeamOu.ReferenceId,
            onlyUnitsWithManager: false,
            asOfUtc: data.Now);
        allSteps.Select(s => s.OuRefId).ShouldBe([
            data.TeamOu.ReferenceId,
            data.DeptOu.ReferenceId,
            data.RootOu.ReferenceId
        ]);

        var managersOnly = await sut.ResolveOuLeadershipChainAsync(
            data.TeamOu.ReferenceId,
            onlyUnitsWithManager: true,
            asOfUtc: data.Now);
        managersOnly.Select(s => s.OuRefId).ShouldBe([
            data.DeptOu.ReferenceId,
            data.RootOu.ReferenceId
        ]);
    }

    #endregion

    #region Scenario 7: Authority Threshold Escalation

    [Fact]
    public async Task ResolveManagerByApprovalLimitAsync_And_ByMinJobGradeAsync_Should_Escalate_To_Qualifying_Manager()
    {
        await using var db = CreateContext();
        var data = await SeedHierarchyAsync(db);
        var sut = new ChainOfCommandService(db);

        // TeamLead limit = 10k, DeptManager limit = 100k, CEO limit = 1M THB
        var for50k = await sut.ResolveManagerByApprovalLimitAsync(
            data.Staff.ReferenceId,
            50_000m,
            "thb",
            asOfUtc: data.Now);
        for50k.ShouldNotBeNull();
        for50k.NominalManagerPosition.RefId.ShouldBe(data.DeptManager.ReferenceId);

        var for500k = await sut.ResolveManagerByApprovalLimitAsync(
            data.Staff.ReferenceId,
            500_000m,
            "THB",
            asOfUtc: data.Now);
        for500k.ShouldNotBeNull();
        for500k.NominalManagerPosition.RefId.ShouldBe(data.Ceo.ReferenceId);

        var tooHigh = await sut.ResolveManagerByApprovalLimitAsync(
            data.Staff.ReferenceId,
            5_000_000m,
            "THB",
            asOfUtc: data.Now);
        tooHigh.ShouldBeNull();

        // Grade escalation: Executive category -> CEO
        var exec = await sut.ResolveManagerByMinJobGradeAsync(
            data.Staff.ReferenceId,
            minJobLevelCategory: JobLevelCategory.Executive.Key,
            asOfUtc: data.Now);
        exec.ShouldNotBeNull();
        exec.NominalManagerPosition.RefId.ShouldBe(data.Ceo.ReferenceId);
    }

    #endregion

    #region Scenario 8: Downward & Relationship Verification

    [Fact]
    public async Task Downward_And_IsInChainOfCommand_Should_Resolve_Accurately()
    {
        await using var db = CreateContext();
        var data = await SeedHierarchyAsync(db);

        var delegation = PositionDelegation.Create(
            data.LegalEntity.Id,
            data.DeptManager.Id,
            data.BackupManager.Id,
            DelegationType.ActingFull,
            data.Now.AddDays(-1),
            data.Now.AddDays(5));
        db.PositionDelegations.Add(delegation);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var sut = new ChainOfCommandService(db);

        var directReports = await sut.GetDirectReportsAsync(data.DeptManager.ReferenceId, asOfUtc: data.Now);
        directReports.Select(p => p.RefId).ShouldBe([data.TeamLead.ReferenceId]);

        var subtree = await sut.GetSubordinateSubtreeAsync(data.Ceo.ReferenceId, maxDepth: 10, asOfUtc: data.Now);
        subtree.Count.ShouldBe(4); // DeptManager, DottedDirector, TeamLead, Staff

        (await sut.IsInChainOfCommandAsync(data.Ceo.ReferenceId, data.Staff.ReferenceId, asOfUtc: data.Now)).ShouldBeTrue();
        (await sut.IsInChainOfCommandAsync(data.Staff.ReferenceId, data.Staff.ReferenceId, asOfUtc: data.Now)).ShouldBeFalse();
        (await sut.IsInChainOfCommandAsync(data.DottedDirector.ReferenceId, data.Staff.ReferenceId, includeDottedLine: false, asOfUtc: data.Now)).ShouldBeFalse();
        (await sut.IsInChainOfCommandAsync(data.DottedDirector.ReferenceId, data.Staff.ReferenceId, includeDottedLine: true, asOfUtc: data.Now)).ShouldBeTrue();
        (await sut.IsInChainOfCommandAsync(data.BackupManager.ReferenceId, data.Staff.ReferenceId, includeActiveDelegations: true, asOfUtc: data.Now)).ShouldBeTrue();
    }

    #endregion

    #region Seed & Context Helpers

    private OrganizationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<OrganizationDbContext>()
            .UseInMemoryDatabase($"org-coc-{Guid.NewGuid():N}")
            .Options;

        var settings = Options.Create(new DatabaseOptions());
        var environment = Substitute.For<IHostEnvironment>();
        environment.EnvironmentName.Returns("Development");

        return new OrganizationDbContext(_tenantAccessor, options, settings, environment);
    }

    private void SetTenant(string tenantId) =>
        ((IMultiTenantContextSetter)_tenantAccessor).MultiTenantContext =
            new MultiTenantContext<AppTenantInfo>(new AppTenantInfo(tenantId, tenantId));

    private async Task<SeedData> SeedHierarchyAsync(OrganizationDbContext db, bool teamLeadActive = true)
    {
        SetTenant(TenantId);
        var now = new DateTimeOffset(2026, 11, 18, 12, 0, 0, TimeSpan.Zero);

        var le = LegalEntity.Create("ACME", "TH", "Acme Thailand", "Acme Thailand", "0105560000001", "THB");
        var ouType = OrganizationUnitType.Create("UNIT", "Unit", "Unit", 1);

        var grade1 = JobGrade.Create("G1", "Grade 1", "Grade 1", 10, jobLevelCategory: JobLevelCategory.Operational);
        var grade2 = JobGrade.Create("G2", "Grade 2", "Grade 2", 20, jobLevelCategory: JobLevelCategory.Supervisory);
        var grade3 = JobGrade.Create("G3", "Grade 3", "Grade 3", 30, jobLevelCategory: JobLevelCategory.Management);
        var grade4 = JobGrade.Create("G4", "Grade 4", "Grade 4", 40, jobLevelCategory: JobLevelCategory.Executive);

        var job = Job.Create("ENG", "Engineer", "Engineer", grade1.Id);

        var rootOu = OrganizationUnit.Create("ROOT", "Headquarters", "Headquarters", ouType.Id, legalEntityId: le.Id);
        var deptOu = OrganizationUnit.Create("DEPT", "Engineering", "Engineering", ouType.Id, legalEntityId: le.Id, parent: rootOu);
        var teamOu = OrganizationUnit.Create("TEAM", "Backend Team", "Backend Team", ouType.Id, legalEntityId: le.Id, parent: deptOu);

        var ceo = Position.Create(
            le.Id, rootOu.Id, job.Id, grade4.Id,
            "POS-CEO", "Chief Executive Officer", "CEO",
            approvalLimitAmount: 1_000_000m, approvalLimitCurrency: "THB", isPrimaryLeadership: true);

        var deptManager = Position.Create(
            le.Id, deptOu.Id, job.Id, grade3.Id,
            "POS-MGR", "Engineering Manager", "Engineering Manager",
            approvalLimitAmount: 100_000m, approvalLimitCurrency: "THB",
            reportsToPositionId: ceo.Id, isPrimaryLeadership: true);

        var dottedDirector = Position.Create(
            le.Id, rootOu.Id, job.Id, grade4.Id,
            "POS-DOT", "Architecture Director", "Architecture Director",
            approvalLimitAmount: 200_000m, approvalLimitCurrency: "THB",
            reportsToPositionId: ceo.Id);

        var teamLead = Position.Create(
            le.Id, teamOu.Id, job.Id, grade2.Id,
            "POS-LEAD", "Tech Lead", "Tech Lead",
            approvalLimitAmount: 10_000m, approvalLimitCurrency: "THB",
            reportsToPositionId: deptManager.Id,
            dottedLinePositionId: null,
            isActive: teamLeadActive);

        var staff = Position.Create(
            le.Id, teamOu.Id, job.Id, grade1.Id,
            "POS-STAFF", "Software Engineer", "Software Engineer",
            reportsToPositionId: teamLead.Id,
            dottedLinePositionId: dottedDirector.Id);

        var backupManager = Position.Create(
            le.Id, rootOu.Id, job.Id, grade3.Id,
            "POS-BACKUP", "Operations Manager", "Operations Manager",
            approvalLimitAmount: 100_000m, approvalLimitCurrency: "THB");

        rootOu.AssignManagerPosition(ceo, allowExternalManagerPosition: true);
        deptOu.AssignManagerPosition(deptManager, allowExternalManagerPosition: true);

        db.LegalEntities.Add(le);
        db.OrganizationUnitTypes.Add(ouType);
        db.JobGrades.AddRange(grade1, grade2, grade3, grade4);
        db.Jobs.Add(job);
        db.OrganizationUnits.AddRange(rootOu, deptOu, teamOu);
        db.Positions.AddRange(ceo, deptManager, dottedDirector, teamLead, staff, backupManager);

        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        return new SeedData(now, le, rootOu, deptOu, teamOu, job, grade1, ceo, deptManager, dottedDirector, teamLead, staff, backupManager);
    }

    private sealed record SeedData(
        DateTimeOffset Now,
        LegalEntity LegalEntity,
        OrganizationUnit RootOu,
        OrganizationUnit DeptOu,
        OrganizationUnit TeamOu,
        Job Job,
        JobGrade Grade1,
        Position Ceo,
        Position DeptManager,
        Position DottedDirector,
        Position TeamLead,
        Position Staff,
        Position BackupManager);

    private sealed class TestTenantAccessor : IMultiTenantContextAccessor<AppTenantInfo>, IMultiTenantContextSetter
    {
        private IMultiTenantContext<AppTenantInfo> _context = new MultiTenantContext<AppTenantInfo>(new AppTenantInfo());

        public IMultiTenantContext<AppTenantInfo> MultiTenantContext => _context;

        IMultiTenantContext IMultiTenantContextAccessor.MultiTenantContext => _context;

        IMultiTenantContext IMultiTenantContextSetter.MultiTenantContext
        {
            set => _context = (IMultiTenantContext<AppTenantInfo>)value;
        }
    }

    #endregion
}
