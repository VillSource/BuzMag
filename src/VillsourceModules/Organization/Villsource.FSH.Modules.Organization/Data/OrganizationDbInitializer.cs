using FSH.Framework.Core.Domain;
using FSH.Framework.Persistence;
using FSH.Modules.Identity.Contracts.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Villsource.FSH.Modules.Organization.Contracts.Constants;
using Villsource.FSH.Modules.Organization.Domain.CostCenters;
using Villsource.FSH.Modules.Organization.Domain.JobFamilies;
using Villsource.FSH.Modules.Organization.Domain.JobGrades;
using Villsource.FSH.Modules.Organization.Domain.Jobs;
using Villsource.FSH.Modules.Organization.Domain.LegalEntities;
using Villsource.FSH.Modules.Organization.Domain.OrganizationUnits;
using Villsource.FSH.Modules.Organization.Domain.OrganizationUnitTypes;
using Villsource.FSH.Modules.Organization.Domain.PositionDelegations;
using Villsource.FSH.Modules.Organization.Domain.Positions;
using Villsource.ObjectValue;

namespace Villsource.FSH.Modules.Organization.Data;

public sealed partial class OrganizationDbInitializer(
    OrganizationDbContext dbContext,
    ICurrentUserService currentUserService,
    ILogger<OrganizationDbInitializer> logger) : IDbInitializer
{
    public async Task MigrateAsync(CancellationToken cancellationToken)
    {
        if ((await dbContext.Database.GetPendingMigrationsAsync(cancellationToken).ConfigureAwait(false)).Any())
        {
            await dbContext.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);
            LogMigrationsApplied();
        }
    }

    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        var tenant = ResolveTenantIdentifier();
        LogSeedingStarted(tenant);

        await SeedLegalEntitiesAsync(cancellationToken).ConfigureAwait(false);
        await SeedJobFamiliesAsync(cancellationToken).ConfigureAwait(false);
        await SeedJobGradesAsync(cancellationToken).ConfigureAwait(false);
        await SeedJobsAsync(cancellationToken).ConfigureAwait(false);
        await SeedOrganizationUnitTypesAsync(cancellationToken).ConfigureAwait(false);
        await SeedOrganizationUnitsAsync(cancellationToken).ConfigureAwait(false);
        await SeedCostCentersAsync(cancellationToken).ConfigureAwait(false);
        await SeedPositionsAndLeadershipAsync(cancellationToken).ConfigureAwait(false);
        await SeedPositionDelegationsAsync(cancellationToken).ConfigureAwait(false);
    }

    private string ResolveTenantIdentifier()
    {
        var tenant = currentUserService.GetTenant();
        if (!string.IsNullOrWhiteSpace(tenant))
        {
            return tenant.Trim();
        }

        return dbContext.TenantInfo?.Identifier
            ?? dbContext.TenantInfo?.Id
            ?? "DEFAULT";
    }

    private async Task SaveSeedChangesAsync(CancellationToken cancellationToken)
    {
        foreach (var entry in dbContext.ChangeTracker.Entries<IHasDomainEvents>())
        {
            entry.Entity.ClearDomainEvents();
        }

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task SeedLegalEntitiesAsync(CancellationToken cancellationToken)
    {
        if (await dbContext.LegalEntities.AnyAsync(cancellationToken).ConfigureAwait(false))
        {
            return;
        }

        var thCorp = LegalEntity.Create(
            code: "TH-CORP",
            countryCode: "TH",
            legalName: "บริษัท วิลล์ซอร์ส เอ็นเตอร์ไพรส์ จำกัด",
            internationalName: "Villsource Enterprise Co., Ltd.",
            taxId: "0105560012345",
            baseCurrency: "THB",
            registrationNumber: "0105560012345",
            profile: new CompanyProfile
            {
                TradeName = "Villsource",
                Website = "https://villsource.example.com",
                ContactEmail = "contact@villsource.example.com"
            },
            headOfficeAddress: new Address(),
            headOfficeBranchCode: "00000",
            headOfficeName: "สำนักงานใหญ่",
            headOfficeInternationalName: "Head Office",
            fiscalYearStartMonth: 1,
            timeZoneId: "Asia/Bangkok");

        thCorp.AddBranch(
            branchCode: "00001",
            name: "สาขาลาดกระบัง",
            internationalName: "Lat Krabang Branch",
            registeredAddress: new Address(),
            isHeadOffice: false);

        var sgSub = LegalEntity.Create(
            code: "SG-SUB",
            countryCode: "SG",
            legalName: "Villsource Singapore Pte. Ltd.",
            internationalName: "Villsource Singapore Pte. Ltd.",
            taxId: "202012345A",
            baseCurrency: "SGD",
            registrationNumber: "202012345A",
            headOfficeAddress: new Address(),
            headOfficeBranchCode: "HQ",
            headOfficeName: "Singapore Head Office",
            headOfficeInternationalName: "Singapore Head Office",
            fiscalYearStartMonth: 1,
            timeZoneId: "Asia/Singapore");

        List<LegalEntity> entities = [thCorp, sgSub];
        await dbContext.LegalEntities.AddRangeAsync(entities, cancellationToken).ConfigureAwait(false);
        await SaveSeedChangesAsync(cancellationToken).ConfigureAwait(false);
        LogSeedLegalEntitiesSuccess(entities.Count);
    }

    private async Task SeedJobFamiliesAsync(CancellationToken cancellationToken)
    {
        if (await dbContext.JobFamilies.AnyAsync(cancellationToken).ConfigureAwait(false))
        {
            return;
        }

        List<JobFamily> jobFamilies =
        [
            JobFamily.Create("EXEC", "ผู้บริหาร", "Executive Leadership", "C-level and executive management positions."),
            JobFamily.Create("ENG", "วิศวกรรมและเทคโนโลยี", "Engineering & Technology", "Software engineering, architecture, and technical leadership."),
            JobFamily.Create("FIN", "การเงินและบัญชี", "Finance & Accounting", "Financial planning, accounting, and treasury."),
            JobFamily.Create("HR", "ทรัพยากรบุคคล", "Human Resources", "People operations, talent, and compliance."),
            JobFamily.Create("OPS", "ปฏิบัติการ", "Operations", "Business operations and service delivery.")
        ];

        await dbContext.JobFamilies.AddRangeAsync(jobFamilies, cancellationToken).ConfigureAwait(false);
        await SaveSeedChangesAsync(cancellationToken).ConfigureAwait(false);
        LogSeedJobFamiliesSuccess(jobFamilies.Count);
    }

    private async Task SeedJobGradesAsync(CancellationToken cancellationToken)
    {
        if (await dbContext.JobGrades.AnyAsync(cancellationToken).ConfigureAwait(false))
        {
            return;
        }

        List<JobGrade> jobGrades =
        [
            JobGrade.Create("G1", "ระดับ 1 - ปฏิบัติการ", "Grade 1 - Junior Staff", 1, "Entry-level operational staff.", JobLevelCategory.Operational),
            JobGrade.Create("G2", "ระดับ 2 - ชำนาญการ", "Grade 2 - Senior Staff", 2, "Experienced individual contributor / specialist.", JobLevelCategory.Operational),
            JobGrade.Create("G3", "ระดับ 3 - หัวหน้างาน", "Grade 3 - Supervisor / Lead", 3, "First-line team lead or supervisor.", JobLevelCategory.Supervisory),
            JobGrade.Create("G4", "ระดับ 4 - ผู้จัดการ", "Grade 4 - Manager", 4, "Department or functional manager.", JobLevelCategory.Management),
            JobGrade.Create("G5", "ระดับ 5 - ผู้อำนวยการ", "Grade 5 - Director", 5, "Senior director or general manager.", JobLevelCategory.Management),
            JobGrade.Create("G6", "ระดับ 6 - ผู้บริหารระดับสูง", "Grade 6 - Executive", 6, "C-level executive leadership.", JobLevelCategory.Executive)
        ];

        await dbContext.JobGrades.AddRangeAsync(jobGrades, cancellationToken).ConfigureAwait(false);
        await SaveSeedChangesAsync(cancellationToken).ConfigureAwait(false);
        LogSeedJobGradesSuccess(jobGrades.Count);
    }

    private async Task SeedJobsAsync(CancellationToken cancellationToken)
    {
        if (await dbContext.Jobs.AnyAsync(cancellationToken).ConfigureAwait(false))
        {
            return;
        }

        var families = await dbContext.JobFamilies
            .ToDictionaryAsync(f => f.Code, StringComparer.OrdinalIgnoreCase, cancellationToken)
            .ConfigureAwait(false);

        var grades = await dbContext.JobGrades
            .ToDictionaryAsync(g => g.Code, StringComparer.OrdinalIgnoreCase, cancellationToken)
            .ConfigureAwait(false);

        var defaultGrade = grades.Values.OrderBy(g => g.RankOrder).First();

        Guid GradeId(string code) => grades.TryGetValue(code, out var grade) ? grade.Id : defaultGrade.Id;
        Guid? FamilyId(string code) => families.TryGetValue(code, out var family) ? family.Id : null;

        List<Job> jobs =
        [
            Job.Create("CEO", "ประธานเจ้าหน้าที่บริหาร", "Chief Executive Officer", GradeId("G6"), FamilyId("EXEC")),
            Job.Create("CFO", "ประธานเจ้าหน้าที่ฝ่ายการเงิน", "Chief Financial Officer", GradeId("G6"), FamilyId("FIN")),
            Job.Create("CTO", "ประธานเจ้าหน้าที่ฝ่ายเทคโนโลยี", "Chief Technology Officer", GradeId("G6"), FamilyId("ENG")),
            Job.Create("GM", "ผู้จัดการทั่วไป", "General Manager", GradeId("G5"), FamilyId("EXEC")),
            Job.Create("ENG-DIR", "ผู้อำนวยการฝ่ายวิศวกรรม", "Engineering Director", GradeId("G5"), FamilyId("ENG")),
            Job.Create("HR-DIR", "ผู้อำนวยการฝ่ายทรัพยากรบุคคล", "HR Director", GradeId("G5"), FamilyId("HR")),
            Job.Create("ENG-MGR", "ผู้จัดการฝ่ายวิศวกรรม", "Engineering Manager", GradeId("G4"), FamilyId("ENG")),
            Job.Create("FIN-MGR", "ผู้จัดการฝ่ายการเงิน", "Finance Manager", GradeId("G4"), FamilyId("FIN")),
            Job.Create("OPS-MGR", "ผู้จัดการฝ่ายปฏิบัติการ", "Operations Manager", GradeId("G4"), FamilyId("OPS")),
            Job.Create("TECH-LEAD", "หัวหน้าทีมเทคโนโลยี", "Tech Lead", GradeId("G3"), FamilyId("ENG")),
            Job.Create("SWE-SR", "วิศวกรซอฟต์แวร์อาวุโส", "Senior Software Engineer", GradeId("G2"), FamilyId("ENG")),
            Job.Create("FIN-SPEC", "เจ้าหน้าที่การเงินอาวุโส", "Finance Specialist", GradeId("G2"), FamilyId("FIN")),
            Job.Create("SWE-JR", "วิศวกรซอฟต์แวร์", "Software Engineer", GradeId("G1"), FamilyId("ENG")),
            Job.Create("OPS-SPEC", "เจ้าหน้าที่ปฏิบัติการ", "Operations Specialist", GradeId("G1"), FamilyId("OPS"))
        ];

        await dbContext.Jobs.AddRangeAsync(jobs, cancellationToken).ConfigureAwait(false);
        await SaveSeedChangesAsync(cancellationToken).ConfigureAwait(false);
        LogSeedJobsSuccess(jobs.Count);
    }

    private async Task SeedOrganizationUnitTypesAsync(CancellationToken cancellationToken)
    {
        if (await dbContext.OrganizationUnitTypes.AnyAsync(cancellationToken).ConfigureAwait(false))
        {
            return;
        }

        List<OrganizationUnitType> ouTypes =
        [
            OrganizationUnitType.Create("ORG", "บริษัท / องค์กร", "Organization", 0, "Root legal/corporate organization unit."),
            OrganizationUnitType.Create("DIV", "สายงาน", "Division", 1, "Executive division under the root organization."),
            OrganizationUnitType.Create("DEPT", "ฝ่าย / แผนก", "Department", 2, "Functional department under a division."),
            OrganizationUnitType.Create("TEAM", "ทีม", "Team", 3, "Operational team under a department.")
        ];

        await dbContext.OrganizationUnitTypes.AddRangeAsync(ouTypes, cancellationToken).ConfigureAwait(false);
        await SaveSeedChangesAsync(cancellationToken).ConfigureAwait(false);
        LogSeedOuTypesSuccess(ouTypes.Count);
    }

    private async Task SeedOrganizationUnitsAsync(CancellationToken cancellationToken)
    {
        if (await dbContext.OrganizationUnits.AnyAsync(cancellationToken).ConfigureAwait(false))
        {
            return;
        }

        var legalEntities = await dbContext.LegalEntities
            .ToDictionaryAsync(l => l.Code, StringComparer.OrdinalIgnoreCase, cancellationToken)
            .ConfigureAwait(false);

        var ouTypes = await dbContext.OrganizationUnitTypes
            .ToDictionaryAsync(t => t.Code, StringComparer.OrdinalIgnoreCase, cancellationToken)
            .ConfigureAwait(false);

        var fallbackType = ouTypes.Values.OrderBy(t => t.HierarchyLevel).First();
        Guid TypeId(string code) => ouTypes.TryGetValue(code, out var t) ? t.Id : fallbackType.Id;

        var thCorpId = legalEntities["TH-CORP"].Id;
        var sgSubId = legalEntities["SG-SUB"].Id;

        var orgHq = OrganizationUnit.Create("ORG-HQ", "สำนักงานใหญ่", "Headquarters", TypeId("ORG"), "Root corporate organization unit.", thCorpId);
        var divTech = OrganizationUnit.Create("DIV-TECH", "สายงานเทคโนโลยี", "Technology Division", TypeId("DIV"), "Technology & Engineering division.", thCorpId, parent: orgHq);
        var deptEng = OrganizationUnit.Create("DEPT-ENG", "ฝ่ายวิศวกรรมซอฟต์แวร์", "Software Engineering Dept", TypeId("DEPT"), "Intentionally has no OU manager to test OU crawl-up.", thCorpId, parent: divTech);
        var teamBe = OrganizationUnit.Create("TEAM-BE", "ทีม Backend", "Backend Team", TypeId("TEAM"), "Backend services team.", thCorpId, parent: deptEng);
        var teamFe = OrganizationUnit.Create("TEAM-FE", "ทีม Frontend", "Frontend Team", TypeId("TEAM"), "Intentionally has no OU manager to test multi-hop OU crawl-up.", thCorpId, parent: deptEng);

        var deptInactive = OrganizationUnit.Create("DEPT-INACTIVE", "ฝ่ายระบบเดิม (ปิดใช้งาน)", "Legacy IT Dept (Inactive)", TypeId("DEPT"), "Inactive department for testing includeInactiveOu.", thCorpId, parent: divTech);
        var teamLegacy = OrganizationUnit.Create("TEAM-LEGACY", "ทีมสนับสนุนระบบเดิม", "Legacy Support Team", TypeId("TEAM"), "Active child team under an inactive parent department.", thCorpId, parent: deptInactive);

        var divFin = OrganizationUnit.Create("DIV-FIN", "สายงานการเงิน", "Finance Division", TypeId("DIV"), "Finance & Accounting division.", thCorpId, parent: orgHq);
        var deptAcc = OrganizationUnit.Create("DEPT-ACC", "ฝ่ายบัญชี", "Accounting Dept", TypeId("DEPT"), "Accounting department.", thCorpId, parent: divFin);
        var divOps = OrganizationUnit.Create("DIV-OPS", "สายงานปฏิบัติการ", "Operations Division", TypeId("DIV"), "Operations division.", thCorpId, parent: orgHq);

        var orgSgHq = OrganizationUnit.Create("ORG-SG-HQ", "สำนักงานสิงคโปร์", "Singapore HQ", TypeId("ORG"), "Separate legal entity root OU for cross-entity isolation tests.", sgSubId);

        List<OrganizationUnit> units =
        [
            orgHq,
            divTech,
            deptEng,
            teamBe,
            teamFe,
            deptInactive,
            teamLegacy,
            divFin,
            deptAcc,
            divOps,
            orgSgHq
        ];

        await dbContext.OrganizationUnits.AddRangeAsync(units, cancellationToken).ConfigureAwait(false);
        await SaveSeedChangesAsync(cancellationToken).ConfigureAwait(false);
        LogSeedOrganizationUnitsSuccess(units.Count);
    }

    private async Task SeedCostCentersAsync(CancellationToken cancellationToken)
    {
        if (await dbContext.CostCenters.AnyAsync(cancellationToken).ConfigureAwait(false))
        {
            return;
        }

        var thCorp = await dbContext.LegalEntities
            .Include(l => l.Branches)
            .FirstAsync(l => l.Code == "TH-CORP", cancellationToken)
            .ConfigureAwait(false);

        var orgUnits = await dbContext.OrganizationUnits
            .ToDictionaryAsync(u => u.Code, StringComparer.OrdinalIgnoreCase, cancellationToken)
            .ConfigureAwait(false);

        var hqBranchId = thCorp.Branches.First(b => b.IsHeadOffice).Id;
        var latKrabangBranchId = thCorp.Branches.FirstOrDefault(b => b.BranchCode == "00001")?.Id ?? hqBranchId;

        var ccHq = CostCenter.Create(
            thCorp.Id,
            "CC-HQ",
            "ศูนย์ต้นทุนสำนักงานใหญ่",
            "Headquarters Cost Center",
            CostCenterFunctionType.Administrative,
            "Corporate administrative cost center.",
            defaultTaxBranchId: hqBranchId,
            orgUnitId: orgUnits["ORG-HQ"].Id);

        var ccEng = CostCenter.Create(
            thCorp.Id,
            "CC-ENG",
            "ศูนย์ต้นทุนวิศวกรรม",
            "Engineering Cost Center",
            CostCenterFunctionType.ResearchAndDevelopment,
            "R&D and software engineering cost center.",
            parentCostCenterId: ccHq.Id,
            defaultTaxBranchId: hqBranchId,
            orgUnitId: orgUnits["DEPT-ENG"].Id);

        var ccFin = CostCenter.Create(
            thCorp.Id,
            "CC-FIN",
            "ศูนย์ต้นทุนการเงิน",
            "Finance Cost Center",
            CostCenterFunctionType.Administrative,
            "Finance and accounting cost center.",
            parentCostCenterId: ccHq.Id,
            defaultTaxBranchId: hqBranchId,
            orgUnitId: orgUnits["DIV-FIN"].Id);

        var ccOps = CostCenter.Create(
            thCorp.Id,
            "CC-OPS",
            "ศูนย์ต้นทุนปฏิบัติการ",
            "Operations Cost Center",
            CostCenterFunctionType.CostOfSales,
            "Operations and fulfillment cost center.",
            parentCostCenterId: ccHq.Id,
            defaultTaxBranchId: latKrabangBranchId,
            orgUnitId: orgUnits["DIV-OPS"].Id);

        List<CostCenter> costCenters = [ccHq, ccEng, ccFin, ccOps];
        await dbContext.CostCenters.AddRangeAsync(costCenters, cancellationToken).ConfigureAwait(false);
        await SaveSeedChangesAsync(cancellationToken).ConfigureAwait(false);
        LogSeedCostCentersSuccess(costCenters.Count);
    }

    private async Task SeedPositionsAndLeadershipAsync(CancellationToken cancellationToken)
    {
        if (await dbContext.Positions.AnyAsync(cancellationToken).ConfigureAwait(false))
        {
            return;
        }

        var legalEntities = await dbContext.LegalEntities
            .Include(l => l.Branches)
            .ToDictionaryAsync(l => l.Code, StringComparer.OrdinalIgnoreCase, cancellationToken)
            .ConfigureAwait(false);

        var orgUnits = await dbContext.OrganizationUnits
            .ToDictionaryAsync(u => u.Code, StringComparer.OrdinalIgnoreCase, cancellationToken)
            .ConfigureAwait(false);

        var jobs = await dbContext.Jobs
            .ToDictionaryAsync(j => j.Code, StringComparer.OrdinalIgnoreCase, cancellationToken)
            .ConfigureAwait(false);

        var grades = await dbContext.JobGrades
            .ToDictionaryAsync(g => g.Code, StringComparer.OrdinalIgnoreCase, cancellationToken)
            .ConfigureAwait(false);

        var costCenters = await dbContext.CostCenters
            .ToDictionaryAsync(c => c.Code, StringComparer.OrdinalIgnoreCase, cancellationToken)
            .ConfigureAwait(false);

        var thCorp = legalEntities["TH-CORP"];
        var sgSub = legalEntities["SG-SUB"];
        var hqBranchId = thCorp.Branches.First(b => b.IsHeadOffice).Id;
        var opsBranchId = thCorp.Branches.FirstOrDefault(b => b.BranchCode == "00001")?.Id ?? hqBranchId;
        var sgBranchId = sgSub.Branches.First(b => b.IsHeadOffice).Id;
        var now = DateTimeOffset.UtcNow;

        var posCeo = Position.Create(
            thCorp.Id, orgUnits["ORG-HQ"].Id, jobs["CEO"].Id, grades["G6"].Id,
            "POS-CEO", "ประธานเจ้าหน้าที่บริหาร", "Chief Executive Officer",
            description: "Top-level executive position with highest approval authority.",
            costCenterId: costCenters["CC-HQ"].Id, workLocationTaxBranchId: hqBranchId,
            approvalLimitAmount: 10_000_000m, approvalLimitCurrency: "THB",
            isPrimaryLeadership: true);

        var posCfo = Position.Create(
            thCorp.Id, orgUnits["DIV-FIN"].Id, jobs["CFO"].Id, grades["G6"].Id,
            "POS-CFO", "ประธานเจ้าหน้าที่ฝ่ายการเงิน", "Chief Financial Officer",
            costCenterId: costCenters["CC-FIN"].Id, workLocationTaxBranchId: hqBranchId,
            approvalLimitAmount: 5_000_000m, approvalLimitCurrency: "THB",
            reportsToPositionId: posCeo.Id, isPrimaryLeadership: true);

        var posCto = Position.Create(
            thCorp.Id, orgUnits["DIV-TECH"].Id, jobs["CTO"].Id, grades["G6"].Id,
            "POS-CTO", "ประธานเจ้าหน้าที่ฝ่ายเทคโนโลยี", "Chief Technology Officer",
            costCenterId: costCenters["CC-ENG"].Id, workLocationTaxBranchId: hqBranchId,
            approvalLimitAmount: 3_000_000m, approvalLimitCurrency: "THB",
            reportsToPositionId: posCeo.Id, isPrimaryLeadership: true);

        var posHrDir = Position.Create(
            thCorp.Id, orgUnits["ORG-HQ"].Id, jobs["HR-DIR"].Id, grades["G5"].Id,
            "POS-HR-DIR", "ผู้อำนวยการฝ่ายทรัพยากรบุคคล", "HR Director",
            costCenterId: costCenters["CC-HQ"].Id, workLocationTaxBranchId: hqBranchId,
            approvalLimitAmount: 1_000_000m, approvalLimitCurrency: "THB",
            reportsToPositionId: posCeo.Id);

        var posOpsMgr = Position.Create(
            thCorp.Id, orgUnits["DIV-OPS"].Id, jobs["OPS-MGR"].Id, grades["G4"].Id,
            "POS-OPS-MGR", "ผู้จัดการฝ่ายปฏิบัติการ", "Operations Manager",
            costCenterId: costCenters["CC-OPS"].Id, workLocationTaxBranchId: opsBranchId,
            approvalLimitAmount: 500_000m, approvalLimitCurrency: "THB",
            reportsToPositionId: posCeo.Id, isPrimaryLeadership: true);

        var posEngDir = Position.Create(
            thCorp.Id, orgUnits["DEPT-ENG"].Id, jobs["ENG-DIR"].Id, grades["G5"].Id,
            "POS-ENG-DIR", "ผู้อำนวยการฝ่ายวิศวกรรม", "Engineering Director",
            costCenterId: costCenters["CC-ENG"].Id, workLocationTaxBranchId: hqBranchId,
            approvalLimitAmount: 1_000_000m, approvalLimitCurrency: "THB",
            reportsToPositionId: posCto.Id);

        var posEngMgrInactive = Position.Create(
            thCorp.Id, orgUnits["DEPT-ENG"].Id, jobs["ENG-MGR"].Id, grades["G4"].Id,
            "POS-ENG-MGR-INACTIVE", "ผู้จัดการฝ่ายวิศวกรรม (ระงับใช้งาน)", "Engineering Manager (Inactive)",
            description: "Inactive intermediate manager for testing climbToFirstActive and includeInactive.",
            costCenterId: costCenters["CC-ENG"].Id, workLocationTaxBranchId: hqBranchId,
            approvalLimitAmount: 300_000m, approvalLimitCurrency: "THB",
            reportsToPositionId: posEngDir.Id, isActive: false);

        var posEngLead = Position.Create(
            thCorp.Id, orgUnits["TEAM-BE"].Id, jobs["TECH-LEAD"].Id, grades["G3"].Id,
            "POS-ENG-LEAD", "หัวหน้าทีม Backend", "Backend Tech Lead",
            costCenterId: costCenters["CC-ENG"].Id, workLocationTaxBranchId: hqBranchId,
            approvalLimitAmount: 100_000m, approvalLimitCurrency: "THB",
            reportsToPositionId: posEngDir.Id, isPrimaryLeadership: true);

        var posSweSr1 = Position.Create(
            thCorp.Id, orgUnits["TEAM-BE"].Id, jobs["SWE-SR"].Id, grades["G2"].Id,
            "POS-SWE-SR-1", "วิศวกร Backend อาวุโส", "Senior Backend Engineer",
            description: "Reports to an inactive manager with a dotted line to HR Director.",
            costCenterId: costCenters["CC-ENG"].Id, workLocationTaxBranchId: hqBranchId,
            approvalLimitAmount: 20_000m, approvalLimitCurrency: "THB",
            reportsToPositionId: posEngMgrInactive.Id,
            dottedLinePositionId: posHrDir.Id);

        var posSweJr1 = Position.Create(
            thCorp.Id, orgUnits["TEAM-BE"].Id, jobs["SWE-JR"].Id, grades["G1"].Id,
            "POS-SWE-JR-1", "วิศวกร Backend", "Junior Backend Engineer",
            description: "4-level reporting chain to CEO, with dotted line to Operations Manager.",
            costCenterId: costCenters["CC-ENG"].Id, workLocationTaxBranchId: hqBranchId,
            reportsToPositionId: posEngLead.Id,
            dottedLinePositionId: posOpsMgr.Id);

        var posSweFe1 = Position.Create(
            thCorp.Id, orgUnits["TEAM-FE"].Id, jobs["SWE-SR"].Id, grades["G2"].Id,
            "POS-SWE-FE-1", "วิศวกร Frontend", "Frontend Engineer",
            description: "No direct manager; resolves to CTO via 2-step OU crawl-up.",
            costCenterId: costCenters["CC-ENG"].Id, workLocationTaxBranchId: hqBranchId);

        var posFinMgr = Position.Create(
            thCorp.Id, orgUnits["DEPT-ACC"].Id, jobs["FIN-MGR"].Id, grades["G4"].Id,
            "POS-FIN-MGR", "ผู้จัดการฝ่ายบัญชี", "Accounting Manager",
            costCenterId: costCenters["CC-FIN"].Id, workLocationTaxBranchId: hqBranchId,
            approvalLimitAmount: 500_000m, approvalLimitCurrency: "THB",
            reportsToPositionId: posCfo.Id, isPrimaryLeadership: true);

        var posFinSpec = Position.Create(
            thCorp.Id, orgUnits["DEPT-ACC"].Id, jobs["FIN-SPEC"].Id, grades["G2"].Id,
            "POS-FIN-SPEC", "เจ้าหน้าที่บัญชีอาวุโส", "Accounting Specialist",
            costCenterId: costCenters["CC-FIN"].Id, workLocationTaxBranchId: hqBranchId,
            approvalLimitAmount: 50_000m, approvalLimitCurrency: "THB",
            reportsToPositionId: posFinMgr.Id);

        var posExpMgr = Position.Create(
            thCorp.Id, orgUnits["DIV-OPS"].Id, jobs["OPS-MGR"].Id, grades["G4"].Id,
            "POS-EXP-MGR", "ผู้จัดการปฏิบัติการ (หมดอายุ)", "Operations Manager (Expired)",
            description: "Temporally expired position for testing asOfUtc validity.",
            costCenterId: costCenters["CC-OPS"].Id, workLocationTaxBranchId: opsBranchId,
            approvalLimitAmount: 250_000m, approvalLimitCurrency: "THB",
            reportsToPositionId: posOpsMgr.Id,
            validFrom: now.AddYears(-1),
            validTo: now.AddDays(-1));

        var posOpsStaff = Position.Create(
            thCorp.Id, orgUnits["DIV-OPS"].Id, jobs["OPS-SPEC"].Id, grades["G1"].Id,
            "POS-OPS-STAFF", "เจ้าหน้าที่ปฏิบัติการ", "Operations Staff",
            description: "Reports to a temporally expired manager.",
            costCenterId: costCenters["CC-OPS"].Id, workLocationTaxBranchId: opsBranchId,
            reportsToPositionId: posExpMgr.Id);

        var posFutureMgr = Position.Create(
            thCorp.Id, orgUnits["DIV-OPS"].Id, jobs["OPS-MGR"].Id, grades["G4"].Id,
            "POS-FUTURE-MGR", "ผู้จัดการปฏิบัติการ (ในอนาคต)", "Operations Manager (Future)",
            description: "Future-dated position for testing asOfUtc validity.",
            costCenterId: costCenters["CC-OPS"].Id, workLocationTaxBranchId: opsBranchId,
            approvalLimitAmount: 250_000m, approvalLimitCurrency: "THB",
            reportsToPositionId: posOpsMgr.Id,
            validFrom: now.AddDays(30),
            validTo: now.AddYears(1));

        var posLegacyMgr = Position.Create(
            thCorp.Id, orgUnits["DEPT-INACTIVE"].Id, jobs["ENG-MGR"].Id, grades["G4"].Id,
            "POS-LEGACY-MGR", "ผู้จัดการระบบเดิม", "Legacy IT Manager",
            costCenterId: costCenters["CC-ENG"].Id, workLocationTaxBranchId: hqBranchId,
            approvalLimitAmount: 300_000m, approvalLimitCurrency: "THB",
            reportsToPositionId: posCto.Id);

        var posLegacyStaff = Position.Create(
            thCorp.Id, orgUnits["TEAM-LEGACY"].Id, jobs["SWE-JR"].Id, grades["G1"].Id,
            "POS-LEGACY-STAFF", "เจ้าหน้าที่ระบบเดิม", "Legacy Support Staff",
            description: "No direct manager; sits under an inactive parent OU.",
            costCenterId: costCenters["CC-ENG"].Id, workLocationTaxBranchId: hqBranchId);

        var posSgGm = Position.Create(
            sgSub.Id, orgUnits["ORG-SG-HQ"].Id, jobs["GM"].Id, grades["G5"].Id,
            "POS-SG-GM", "Singapore General Manager", "Singapore General Manager",
            description: "Separate legal entity position for cross-entity isolation testing.",
            workLocationTaxBranchId: sgBranchId,
            approvalLimitAmount: 1_000_000m, approvalLimitCurrency: "SGD",
            isPrimaryLeadership: true);

        List<Position> positions =
        [
            posCeo,
            posCfo,
            posCto,
            posHrDir,
            posOpsMgr,
            posEngDir,
            posEngMgrInactive,
            posEngLead,
            posSweSr1,
            posSweJr1,
            posSweFe1,
            posFinMgr,
            posFinSpec,
            posExpMgr,
            posOpsStaff,
            posFutureMgr,
            posLegacyMgr,
            posLegacyStaff,
            posSgGm
        ];

        await dbContext.Positions.AddRangeAsync(positions, cancellationToken).ConfigureAwait(false);
        await SaveSeedChangesAsync(cancellationToken).ConfigureAwait(false);

        orgUnits["ORG-HQ"].AssignManagerPosition(posCeo);
        orgUnits["DIV-TECH"].AssignManagerPosition(posCto);
        orgUnits["TEAM-BE"].AssignManagerPosition(posEngLead);
        orgUnits["DEPT-INACTIVE"].AssignManagerPosition(posLegacyMgr);
        orgUnits["DEPT-INACTIVE"].Deactivate();
        orgUnits["DIV-FIN"].AssignManagerPosition(posCfo);
        orgUnits["DEPT-ACC"].AssignManagerPosition(posFinMgr);
        orgUnits["DIV-OPS"].AssignManagerPosition(posOpsMgr);
        orgUnits["ORG-SG-HQ"].AssignManagerPosition(posSgGm);

        await SaveSeedChangesAsync(cancellationToken).ConfigureAwait(false);
        LogSeedPositionsSuccess(positions.Count);
    }

    private async Task SeedPositionDelegationsAsync(CancellationToken cancellationToken)
    {
        if (await dbContext.PositionDelegations.AnyAsync(cancellationToken).ConfigureAwait(false))
        {
            return;
        }

        var thCorpId = await dbContext.LegalEntities
            .Where(l => l.Code == "TH-CORP")
            .Select(l => l.Id)
            .FirstAsync(cancellationToken)
            .ConfigureAwait(false);

        var positions = await dbContext.Positions
            .Where(p => p.LegalEntityId == thCorpId)
            .ToDictionaryAsync(p => p.Code, StringComparer.OrdinalIgnoreCase, cancellationToken)
            .ConfigureAwait(false);

        var now = DateTimeOffset.UtcNow;

        var ctoToEngDir = PositionDelegation.Create(
            thCorpId,
            positions["POS-CTO"].Id,
            positions["POS-ENG-DIR"].Id,
            DelegationType.ActingFull,
            now.AddDays(-7),
            now.AddDays(14),
            reason: "CTO Annual Leave");

        var finMgrToOpsMgr = PositionDelegation.Create(
            thCorpId,
            positions["POS-FIN-MGR"].Id,
            positions["POS-OPS-MGR"].Id,
            DelegationType.ApprovalOnly,
            now.AddDays(-2),
            now.AddDays(5),
            reason: "Finance Manager Business Trip");

        var finMgrToHrDir = PositionDelegation.Create(
            thCorpId,
            positions["POS-FIN-MGR"].Id,
            positions["POS-HR-DIR"].Id,
            DelegationType.SpecificDocuments,
            now.AddDays(-2),
            now.AddDays(10),
            documentTypeScope: "PR,PO",
            reason: "Procurement Document Approval Delegation");

        var engLeadToSweSr = PositionDelegation.Create(
            thCorpId,
            positions["POS-ENG-LEAD"].Id,
            positions["POS-SWE-SR-1"].Id,
            DelegationType.SpecificDocuments,
            now.AddDays(-1),
            now.AddDays(7),
            documentTypeScope: "LEAVE,OT",
            reason: "Team Lead Leave & OT Approval Delegation");

        var revokedCfoToFinMgr = PositionDelegation.Create(
            thCorpId,
            positions["POS-CFO"].Id,
            positions["POS-FIN-MGR"].Id,
            DelegationType.ActingFull,
            now.AddDays(-5),
            now.AddDays(5),
            reason: "CFO Leave (Revoked)");
        revokedCfoToFinMgr.Revoke("SYSTEM", "Returned early from leave");

        var expiredCeoToCfo = PositionDelegation.Create(
            thCorpId,
            positions["POS-CEO"].Id,
            positions["POS-CFO"].Id,
            DelegationType.ActingFull,
            now.AddDays(-30),
            now.AddDays(-10),
            reason: "CEO Overseas Summit (Expired)");

        var futureOpsMgrToHrDir = PositionDelegation.Create(
            thCorpId,
            positions["POS-OPS-MGR"].Id,
            positions["POS-HR-DIR"].Id,
            DelegationType.ApprovalOnly,
            now.AddDays(10),
            now.AddDays(20),
            reason: "Scheduled Operations Leave (Future)");

        List<PositionDelegation> delegations =
        [
            ctoToEngDir,
            finMgrToOpsMgr,
            finMgrToHrDir,
            engLeadToSweSr,
            revokedCfoToFinMgr,
            expiredCeoToCfo,
            futureOpsMgrToHrDir
        ];

        await dbContext.PositionDelegations.AddRangeAsync(delegations, cancellationToken).ConfigureAwait(false);
        await SaveSeedChangesAsync(cancellationToken).ConfigureAwait(false);
        LogSeedPositionDelegationsSuccess(delegations.Count);
    }

    [LoggerMessage(LogLevel.Information, "[Organization] applied migrations")]
    partial void LogMigrationsApplied();

    [LoggerMessage(LogLevel.Information, "[Organization] seeding organization data for tenant {Tenant}")]
    partial void LogSeedingStarted(string tenant);

    [LoggerMessage(LogLevel.Information, "[Organization] seeded {LegalEntitiesCount} LegalEntities successfully")]
    partial void LogSeedLegalEntitiesSuccess(int legalEntitiesCount);

    [LoggerMessage(LogLevel.Information, "[Organization] seeded {JobFamiliesCount} JobFamilies successfully")]
    partial void LogSeedJobFamiliesSuccess(int jobFamiliesCount);

    [LoggerMessage(LogLevel.Information, "[Organization] seeded {JobGradesCount} JobGrades successfully")]
    partial void LogSeedJobGradesSuccess(int jobGradesCount);

    [LoggerMessage(LogLevel.Information, "[Organization] seeded {JobsCount} Jobs successfully")]
    partial void LogSeedJobsSuccess(int jobsCount);

    [LoggerMessage(LogLevel.Information, "[Organization] seeded {OrganizationTypeCount} OrganizationUnitTypes successfully")]
    partial void LogSeedOuTypesSuccess(int organizationTypeCount);

    [LoggerMessage(LogLevel.Information, "[Organization] seeded {OrganizationUnitCount} OrganizationUnits successfully")]
    partial void LogSeedOrganizationUnitsSuccess(int organizationUnitCount);

    [LoggerMessage(LogLevel.Information, "[Organization] seeded {CostCentersCount} CostCenters successfully")]
    partial void LogSeedCostCentersSuccess(int costCentersCount);

    [LoggerMessage(LogLevel.Information, "[Organization] seeded {PositionsCount} Positions successfully")]
    partial void LogSeedPositionsSuccess(int positionsCount);

    [LoggerMessage(LogLevel.Information, "[Organization] seeded {DelegationsCount} PositionDelegations successfully")]
    partial void LogSeedPositionDelegationsSuccess(int delegationsCount);
}
