using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Villsource.FSH.Modules.Organization.Contracts.Constants;
using Villsource.FSH.Modules.Organization.Data;
using Villsource.FSH.Modules.Organization.Domain.CostCenters;
using Villsource.FSH.Modules.Organization.Domain.JobFamilies;
using Villsource.FSH.Modules.Organization.Domain.JobGrades;
using Villsource.FSH.Modules.Organization.Domain.Jobs;
using Villsource.FSH.Modules.Organization.Domain.LegalEntities;
using Villsource.FSH.Modules.Organization.Domain.OrganizationUnits;
using Villsource.FSH.Modules.Organization.Domain.OrganizationUnitTypes;
using Villsource.FSH.Modules.Organization.Domain.PositionDelegations;
using Villsource.ObjectValue;
using Position = Villsource.FSH.Modules.Organization.Domain.Positions.Position;

namespace Villsource.BuzMag.DbMigrator.DemoSeed;

/// <summary>
/// Seeds a realistic global big-tech corporate group ("NexaTech Group") into the Organization module
/// for demo and development environments. Idempotent: skips if legal entities already exist.
/// </summary>
internal sealed class OrganizationDemoSeeder(
    OrganizationDbContext dbContext,
    ILogger<OrganizationDemoSeeder> logger)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await dbContext.LegalEntities.AnyAsync(cancellationToken).ConfigureAwait(false))
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;

        // 1. Legal Entities & Tax Branches
        var (usHq, usSeattleBranch, sgApac, thEng, thChiangMaiBranch, jpAi) = CreateLegalEntities(now);
        dbContext.LegalEntities.AddRange(usHq, sgApac, thEng, jpAi);

        // 2. Organization Unit Types
        var unitTypes = CreateOrganizationUnitTypes();
        dbContext.OrganizationUnitTypes.AddRange(unitTypes.Values);

        // 3. Job Grades & Job Families
        var jobGrades = CreateJobGrades();
        var jobFamilies = CreateJobFamilies();
        dbContext.JobGrades.AddRange(jobGrades.Values);
        dbContext.JobFamilies.AddRange(jobFamilies.Values);

        // 4. Jobs
        var jobs = CreateJobs(jobGrades, jobFamilies);
        dbContext.Jobs.AddRange(jobs.Values);

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        // 5. Cost Centers (parent + child hierarchy)
        var costCenters = CreateCostCenters(usHq, sgApac, thEng, jpAi, now);
        dbContext.CostCenters.AddRange(costCenters.Values);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        // 6. Organization Units (multi-level corporate hierarchy)
        var orgUnits = CreateOrganizationUnits(usHq, sgApac, thEng, jpAi, unitTypes, costCenters);
        dbContext.OrganizationUnits.AddRange(orgUnits.Values);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        // 7. Positions (layered by reporting hierarchy so ReportsToPositionId / DottedLinePositionId resolve cleanly)
        var positions = CreatePositions(
            usHq, usSeattleBranch, sgApac, thEng, thChiangMaiBranch, jpAi,
            orgUnits, jobs, jobGrades, costCenters, now);
        dbContext.Positions.AddRange(positions.Values);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        // 8. Assign primary leadership positions onto their Organization Units
        AssignOrganizationUnitManagers(orgUnits, positions);

        // 9. Position Delegations
        var delegations = CreatePositionDelegations(usHq, thEng, positions, now);
        dbContext.PositionDelegations.AddRange(delegations);

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "[demo-seed] seeded Organization big-tech group: {LegalEntities} legal entities, {OrgUnits} org units, {CostCenters} cost centers, {Jobs} jobs, {Positions} positions, {Delegations} delegations",
                4, orgUnits.Count, costCenters.Count, jobs.Count, positions.Count, delegations.Count);
        }
    }

    private static (
        LegalEntity UsHq,
        TaxBranch UsSeattleBranch,
        LegalEntity SgApac,
        LegalEntity ThEng,
        TaxBranch ThChiangMaiBranch,
        LegalEntity JpAi) CreateLegalEntities(DateTimeOffset now)
    {
        var usHq = LegalEntity.Create(
            code: "NXH-US",
            countryCode: "US",
            legalName: "NexaTech Holdings Inc.",
            internationalName: "NexaTech Holdings Inc.",
            taxId: "94-3829104",
            baseCurrency: "USD",
            registrationNumber: "DE-20160419-001",
            profile: new CompanyProfile
            {
                TradeName = "NexaTech Group",
                Website = "https://nexatech.example.com",
                ContactEmail = "corporate@nexatech.example.com",
                ContactPhone = "+1-415-555-0100"
            },
            headOfficeAddress: new Address(
                houseNo: "550",
                subDistrict: "SoMa",
                district: "San Francisco",
                province: "CA",
                postalCode: "94107",
                country: "United States",
                building: "NexaTech Tower",
                road: "Howard Street"),
            headOfficeBranchCode: "HQ",
            headOfficeName: "San Francisco Global HQ",
            headOfficeInternationalName: "San Francisco Global HQ",
            headOfficeRegisteredDate: now.AddYears(-8),
            fiscalYearStartMonth: 1,
            timeZoneId: "America/Los_Angeles");

        var usSeattleBranch = usHq.AddBranch(
            branchCode: "SEA-01",
            name: "Seattle Cloud Campus",
            internationalName: "Seattle Cloud Campus",
            registeredAddress: new Address(
                houseNo: "400",
                subDistrict: "South Lake Union",
                district: "Seattle",
                province: "WA",
                postalCode: "98109",
                country: "United States",
                building: "Cascade Cloud Center",
                road: "Westlake Ave N"),
            isHeadOffice: false,
            registeredDate: now.AddYears(-5));

        var sgApac = LegalEntity.Create(
            code: "NXC-SG",
            countryCode: "SG",
            legalName: "NexaTech Cloud APAC Pte. Ltd.",
            internationalName: "NexaTech Cloud APAC Pte. Ltd.",
            taxId: "201904821K",
            baseCurrency: "SGD",
            registrationNumber: "201904821K",
            profile: new CompanyProfile
            {
                TradeName = "NexaTech APAC",
                Website = "https://apac.nexatech.example.com",
                ContactEmail = "apac-ops@nexatech.example.com",
                ContactPhone = "+65-6555-0188"
            },
            headOfficeAddress: new Address(
                houseNo: "10",
                subDistrict: "Marina Bay",
                district: "Downtown Core",
                province: "Singapore",
                postalCode: "018956",
                country: "Singapore",
                building: "Marina Bay Financial Centre Tower 2",
                road: "Marina Boulevard"),
            headOfficeBranchCode: "HQ",
            headOfficeName: "Singapore APAC Hub",
            headOfficeInternationalName: "Singapore APAC Hub",
            headOfficeRegisteredDate: now.AddYears(-6),
            fiscalYearStartMonth: 1,
            timeZoneId: "Asia/Singapore");

        var thEng = LegalEntity.Create(
            code: "NXT-TH",
            countryCode: "TH",
            legalName: "บริษัท เน็กซ่าเทค (ประเทศไทย) จำกัด",
            internationalName: "NexaTech (Thailand) Co., Ltd.",
            taxId: "0105564098211",
            baseCurrency: "THB",
            registrationNumber: "0105564098211",
            profile: new CompanyProfile
            {
                TradeName = "NexaTech Thailand",
                Website = "https://th.nexatech.example.com",
                ContactEmail = "bkk-hub@nexatech.example.com",
                ContactPhone = "+66-2-555-0199"
            },
            headOfficeAddress: new Address(
                houseNo: "1",
                subDistrict: "สีลม",
                district: "บางรัก",
                province: "กรุงเทพมหานคร",
                postalCode: "10500",
                country: "Thailand",
                building: "อาคารปาร์ค สีลม ชั้น 24",
                road: "ถนนคอนแวนต์"),
            headOfficeBranchCode: "00000",
            headOfficeName: "สำนักงานใหญ่ (กรุงเทพมหานคร)",
            headOfficeInternationalName: "Bangkok Head Office",
            headOfficeRegisteredDate: now.AddYears(-4),
            fiscalYearStartMonth: 1,
            timeZoneId: "Asia/Bangkok");

        var thChiangMaiBranch = thEng.AddBranch(
            branchCode: "00001",
            name: "สาขาเชียงใหม่ (ศูนย์วิศวกรรมซอฟต์แวร์)",
            internationalName: "Chiang Mai Engineering Hub",
            registeredAddress: new Address(
                houseNo: "88",
                subDistrict: "สุเทพ",
                district: "เมืองเชียงใหม่",
                province: "เชียงใหม่",
                postalCode: "50200",
                country: "Thailand",
                building: "One Nimman Tech Wing",
                road: "ถนนนิมมานเหมินท์"),
            isHeadOffice: false,
            taxSubId: "00001",
            registeredDate: now.AddYears(-2));

        var jpAi = LegalEntity.Create(
            code: "NXA-JP",
            countryCode: "JP",
            legalName: "ネクサテックAIラボ株式会社",
            internationalName: "NexaTech AI Labs K.K.",
            taxId: "1010401123456",
            baseCurrency: "JPY",
            registrationNumber: "0104-01-123456",
            profile: new CompanyProfile
            {
                TradeName = "NexaTech AI Labs",
                Website = "https://ai.nexatech.example.com",
                ContactEmail = "tokyo-ai@nexatech.example.com",
                ContactPhone = "+81-3-5555-0140"
            },
            headOfficeAddress: new Address(
                houseNo: "3-21-3",
                subDistrict: "Shibuya",
                district: "Shibuya-ku",
                province: "Tokyo",
                postalCode: "150-0002",
                country: "Japan",
                building: "Shibuya Stream 28F",
                road: "Shibuya"),
            headOfficeBranchCode: "HQ",
            headOfficeName: "Tokyo AI Research Center",
            headOfficeInternationalName: "Tokyo AI Research Center",
            headOfficeRegisteredDate: now.AddYears(-3),
            fiscalYearStartMonth: 4,
            timeZoneId: "Asia/Tokyo");

        return (usHq, usSeattleBranch, sgApac, thEng, thChiangMaiBranch, jpAi);
    }

    private static Dictionary<string, OrganizationUnitType> CreateOrganizationUnitTypes() => new(StringComparer.Ordinal)
    {
        ["GROUP"] = OrganizationUnitType.Create("GROUP", "Corporate Group", "Corporate Group", 0, "Top-level holding group."),
        ["SUBSIDIARY"] = OrganizationUnitType.Create("SUBSIDIARY", "Operating Subsidiary", "Operating Subsidiary", 1, "Regional or legal operating subsidiary."),
        ["DIVISION"] = OrganizationUnitType.Create("DIVISION", "Business Division", "Business Division", 2, "Core technology or corporate business pillar."),
        ["DEPARTMENT"] = OrganizationUnitType.Create("DEPARTMENT", "Department", "Department", 3, "Functional engineering, product, or corporate department."),
        ["TEAM"] = OrganizationUnitType.Create("TEAM", "Team", "Team", 4, "Specialized functional or platform team."),
        ["SQUAD"] = OrganizationUnitType.Create("SQUAD", "Delivery Squad", "Delivery Squad", 5, "Cross-functional agile delivery squad.")
    };

    private static Dictionary<string, JobGrade> CreateJobGrades() => new(StringComparer.Ordinal)
    {
        ["L3"] = JobGrade.Create("L3", "Associate", "Associate", 1, "Entry-level individual contributor.", JobLevelCategory.Operational),
        ["L4"] = JobGrade.Create("L4", "Intermediate", "Intermediate", 2, "Mid-level individual contributor.", JobLevelCategory.Operational),
        ["L5"] = JobGrade.Create("L5", "Senior", "Senior", 3, "Senior individual contributor.", JobLevelCategory.Operational),
        ["L6"] = JobGrade.Create("L6", "Staff", "Staff", 4, "Staff-level technical or domain lead.", JobLevelCategory.Supervisory),
        ["L7"] = JobGrade.Create("L7", "Principal", "Principal", 5, "Principal architect or researcher.", JobLevelCategory.Supervisory),
        ["M1"] = JobGrade.Create("M1", "Manager", "Manager", 6, "First-line people manager.", JobLevelCategory.Supervisory),
        ["M2"] = JobGrade.Create("M2", "Senior Manager", "Senior Manager", 7, "Multi-team manager or controller.", JobLevelCategory.Management),
        ["D1"] = JobGrade.Create("D1", "Director", "Director", 8, "Department director.", JobLevelCategory.Management),
        ["VP"] = JobGrade.Create("VP", "Vice President", "Vice President", 9, "Division or regional executive.", JobLevelCategory.Executive),
        ["C-SUITE"] = JobGrade.Create("C-SUITE", "C-Level Executive", "C-Level Executive", 10, "Group executive officer.", JobLevelCategory.Executive)
    };

    private static Dictionary<string, JobFamily> CreateJobFamilies() => new(StringComparer.Ordinal)
    {
        ["ENG"] = JobFamily.Create("ENG", "Software & Platform Engineering", "Software & Platform Engineering", "Backend, cloud infrastructure, SRE, and core platform systems."),
        ["AI"] = JobFamily.Create("AI", "Artificial Intelligence & Data", "Artificial Intelligence & Data", "Foundation models, applied ML, and data engineering."),
        ["PROD"] = JobFamily.Create("PROD", "Product Management", "Product Management", "Product strategy, roadmaps, and platform capabilities."),
        ["DES"] = JobFamily.Create("DES", "Product Design & UX", "Product Design & UX", "Design systems, user research, and product UX."),
        ["SEC"] = JobFamily.Create("SEC", "Cybersecurity & Trust", "Cybersecurity & Trust", "Application security, cloud security, and compliance."),
        ["GTM"] = JobFamily.Create("GTM", "Go-To-Market & Enterprise Sales", "Go-To-Market & Enterprise Sales", "Enterprise sales, solutions architecture, and customer success."),
        ["CORP"] = JobFamily.Create("CORP", "Corporate & Finance", "Corporate & Finance", "Executive leadership, finance, legal, and people operations.")
    };

    private static Dictionary<string, Job> CreateJobs(
        Dictionary<string, JobGrade> grades,
        Dictionary<string, JobFamily> families) => new(StringComparer.Ordinal)
    {
        ["CEO"] = Job.Create("CEO", "Chief Executive Officer", "Chief Executive Officer", grades["C-SUITE"].Id, families["CORP"].Id, "Group chief executive."),
        ["CFO"] = Job.Create("CFO", "Chief Financial Officer", "Chief Financial Officer", grades["C-SUITE"].Id, families["CORP"].Id, "Group financial strategy and governance."),
        ["CTO"] = Job.Create("CTO", "Chief Technology Officer", "Chief Technology Officer", grades["C-SUITE"].Id, families["ENG"].Id, "Group technology architecture and R&D."),
        ["VP-ENG"] = Job.Create("VP-ENG", "Vice President of Engineering", "Vice President of Engineering", grades["VP"].Id, families["ENG"].Id),
        ["VP-AI"] = Job.Create("VP-AI", "Vice President of AI Research", "Vice President of AI Research", grades["VP"].Id, families["AI"].Id),
        ["DIR-ENG"] = Job.Create("DIR-ENG", "Director of Engineering", "Director of Engineering", grades["D1"].Id, families["ENG"].Id),
        ["FIN-CTRL"] = Job.Create("FIN-CTRL", "Finance Controller", "Finance Controller", grades["M2"].Id, families["CORP"].Id),
        ["ENG-MGR"] = Job.Create("ENG-MGR", "Engineering Manager", "Engineering Manager", grades["M1"].Id, families["ENG"].Id),
        ["PRIN-AI"] = Job.Create("PRIN-AI", "Principal AI Researcher", "Principal AI Researcher", grades["L7"].Id, families["AI"].Id),
        ["SEC-ARCH"] = Job.Create("SEC-ARCH", "Principal Security Architect", "Principal Security Architect", grades["L7"].Id, families["SEC"].Id),
        ["PM-LEAD"] = Job.Create("PM-LEAD", "Principal Product Manager", "Principal Product Manager", grades["L7"].Id, families["PROD"].Id),
        ["STAFF-SWE"] = Job.Create("STAFF-SWE", "Staff Software Engineer", "Staff Software Engineer", grades["L6"].Id, families["ENG"].Id),
        ["SR-SWE"] = Job.Create("SR-SWE", "Senior Software Engineer", "Senior Software Engineer", grades["L5"].Id, families["ENG"].Id),
        ["SR-ML"] = Job.Create("SR-ML", "Senior ML Engineer", "Senior ML Engineer", grades["L5"].Id, families["AI"].Id),
        ["SR-PD"] = Job.Create("SR-PD", "Senior Product Designer", "Senior Product Designer", grades["L5"].Id, families["DES"].Id),
        ["ENT-AE"] = Job.Create("ENT-AE", "Enterprise Account Executive", "Enterprise Account Executive", grades["L5"].Id, families["GTM"].Id),
        ["SWE"] = Job.Create("SWE", "Software Engineer", "Software Engineer", grades["L4"].Id, families["ENG"].Id)
    };

    private static Dictionary<string, CostCenter> CreateCostCenters(
        LegalEntity usHq,
        LegalEntity sgApac,
        LegalEntity thEng,
        LegalEntity jpAi,
        DateTimeOffset now)
    {
        var validFrom = new DateTimeOffset(now.Year, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var usHoBranchId = usHq.Branches.First(b => b.IsHeadOffice).Id;
        var sgHoBranchId = sgApac.Branches.First(b => b.IsHeadOffice).Id;
        var thHoBranchId = thEng.Branches.First(b => b.IsHeadOffice).Id;
        var jpHoBranchId = jpAi.Branches.First(b => b.IsHeadOffice).Id;

        var usExec = CostCenter.Create(usHq.Id, "CC-US-EXEC", "Corporate HQ & Executive", "Corporate HQ & Executive", CostCenterFunctionType.Administrative, "Group executive & corporate governance.", defaultTaxBranchId: usHoBranchId, validFrom: validFrom);
        var usRnd = CostCenter.Create(usHq.Id, "CC-US-RND", "Core Platform R&D", "Core Platform R&D", CostCenterFunctionType.ResearchAndDevelopment, "US engineering & product R&D pool.", defaultTaxBranchId: usHoBranchId, validFrom: validFrom);
        var usCloud = CostCenter.Create(usHq.Id, "CC-US-CLOUD", "Cloud Infrastructure R&D", "Cloud Infrastructure R&D", CostCenterFunctionType.ResearchAndDevelopment, "Kubernetes, compute, and storage R&D.", parentCostCenterId: usRnd.Id, defaultTaxBranchId: usHoBranchId, validFrom: validFrom);
        var usSec = CostCenter.Create(usHq.Id, "CC-US-SEC", "Security & Trust Engineering", "Security & Trust Engineering", CostCenterFunctionType.ResearchAndDevelopment, "Platform security and compliance engineering.", parentCostCenterId: usRnd.Id, defaultTaxBranchId: usHoBranchId, validFrom: validFrom);

        var sgOps = CostCenter.Create(sgApac.Id, "CC-SG-OPS", "APAC Cloud Operations", "APAC Cloud Operations", CostCenterFunctionType.CostOfSales, "Production cloud hosting & SRE operations.", defaultTaxBranchId: sgHoBranchId, validFrom: validFrom);
        var sgGtm = CostCenter.Create(sgApac.Id, "CC-SG-GTM", "APAC Enterprise Sales", "APAC Enterprise Sales", CostCenterFunctionType.SellingAndDistribution, "Regional enterprise sales & customer success.", defaultTaxBranchId: sgHoBranchId, validFrom: validFrom);

        var thRnd = CostCenter.Create(thEng.Id, "CC-TH-RND", "Bangkok Engineering Center", "Bangkok Engineering Center", CostCenterFunctionType.ResearchAndDevelopment, "Core API, payments, and data platform development.", defaultTaxBranchId: thHoBranchId, validFrom: validFrom);
        var thShared = CostCenter.Create(thEng.Id, "CC-TH-SHARED", "APAC Shared Services", "APAC Shared Services", CostCenterFunctionType.SharedServices, "Cross-entity shared engineering & operations pool.", defaultTaxBranchId: thHoBranchId, validFrom: validFrom);

        var jpAiCc = CostCenter.Create(jpAi.Id, "CC-JP-AI", "Tokyo Foundation Model Lab", "Tokyo Foundation Model Lab", CostCenterFunctionType.ResearchAndDevelopment, "Foundation model training and inference research.", defaultTaxBranchId: jpHoBranchId, validFrom: validFrom);

        return new Dictionary<string, CostCenter>(StringComparer.Ordinal)
        {
            ["CC-US-EXEC"] = usExec,
            ["CC-US-RND"] = usRnd,
            ["CC-US-CLOUD"] = usCloud,
            ["CC-US-SEC"] = usSec,
            ["CC-SG-OPS"] = sgOps,
            ["CC-SG-GTM"] = sgGtm,
            ["CC-TH-RND"] = thRnd,
            ["CC-TH-SHARED"] = thShared,
            ["CC-JP-AI"] = jpAiCc
        };
    }

    private static Dictionary<string, OrganizationUnit> CreateOrganizationUnits(
        LegalEntity usHq,
        LegalEntity sgApac,
        LegalEntity thEng,
        LegalEntity jpAi,
        Dictionary<string, OrganizationUnitType> types,
        Dictionary<string, CostCenter> costCenters)
    {
        var grpNexa = OrganizationUnit.Create("GRP-NEXA", "NexaTech Group", "NexaTech Group", types["GROUP"].Id, "Global technology holding group.", usHq.Id, defaultCostCenterId: costCenters["CC-US-EXEC"].Id);

        var divCorp = OrganizationUnit.Create("DIV-CORP", "Corporate & Finance", "Corporate & Finance", types["DIVISION"].Id, "Global finance, legal, and corporate strategy.", usHq.Id, grpNexa, costCenters["CC-US-EXEC"].Id);
        var deptFin = OrganizationUnit.Create("DEPT-FIN", "Global Finance & Treasury", "Global Finance & Treasury", types["DEPARTMENT"].Id, "Consolidation, tax, and treasury.", usHq.Id, divCorp, costCenters["CC-US-EXEC"].Id);

        var divPlatform = OrganizationUnit.Create("DIV-PLATFORM", "Cloud & Core Platform", "Cloud & Core Platform", types["DIVISION"].Id, "Global cloud platform, infrastructure, and product.", usHq.Id, grpNexa, costCenters["CC-US-RND"].Id);
        var deptInfra = OrganizationUnit.Create("DEPT-INFRA", "Cloud Infrastructure", "Cloud Infrastructure", types["DEPARTMENT"].Id, "Compute, networking, and platform security.", usHq.Id, divPlatform, costCenters["CC-US-CLOUD"].Id);
        var teamK8s = OrganizationUnit.Create("TEAM-K8S", "Compute & Kubernetes", "Compute & Kubernetes", types["TEAM"].Id, "Managed container orchestration & runtime.", usHq.Id, deptInfra, costCenters["CC-US-CLOUD"].Id);
        var teamSec = OrganizationUnit.Create("TEAM-SEC", "Platform Security", "Platform Security", types["TEAM"].Id, "Zero-trust identity & cloud security.", usHq.Id, deptInfra, costCenters["CC-US-SEC"].Id);
        var deptProd = OrganizationUnit.Create("DEPT-PROD", "Product & Design", "Product & Design", types["DEPARTMENT"].Id, "Core platform product management & UX.", usHq.Id, divPlatform, costCenters["CC-US-RND"].Id);

        var subApac = OrganizationUnit.Create("SUB-APAC", "NexaTech APAC", "NexaTech APAC", types["SUBSIDIARY"].Id, "APAC commercial & engineering operations.", sgApac.Id, grpNexa, costCenters["CC-SG-OPS"].Id);
        var deptApacGtm = OrganizationUnit.Create("DEPT-APAC-GTM", "APAC Enterprise Sales", "APAC Enterprise Sales", types["DEPARTMENT"].Id, "Enterprise GTM across ASEAN and ANZ.", sgApac.Id, subApac, costCenters["CC-SG-GTM"].Id);

        var deptThEng = OrganizationUnit.Create("DEPT-TH-ENG", "Bangkok Engineering Center", "ศูนย์วิศวกรรมซอฟต์แวร์ กรุงเทพฯ", types["DEPARTMENT"].Id, "Regional product & platform engineering hub.", thEng.Id, subApac, costCenters["CC-TH-RND"].Id);
        var teamThCore = OrganizationUnit.Create("TEAM-TH-CORE", "Core API & Commerce", "Core API & Commerce", types["TEAM"].Id, "High-throughput commerce and billing APIs.", thEng.Id, deptThEng, costCenters["CC-TH-RND"].Id);
        var squadPay = OrganizationUnit.Create("SQUAD-PAY", "Payments & Billing Squad", "Payments & Billing Squad", types["SQUAD"].Id, "Multi-currency payments & settlement.", thEng.Id, teamThCore, costCenters["CC-TH-RND"].Id);
        var teamThData = OrganizationUnit.Create("TEAM-TH-DATA", "Data & Analytics Platform", "Data & Analytics Platform", types["TEAM"].Id, "Real-time telemetry and lakehouse pipelines.", thEng.Id, deptThEng, costCenters["CC-TH-SHARED"].Id);

        var subJpAi = OrganizationUnit.Create("SUB-JP-AI", "NexaTech AI Labs Japan", "NexaTech AI Labs Japan", types["SUBSIDIARY"].Id, "Applied AI & foundation model research.", jpAi.Id, grpNexa, costCenters["CC-JP-AI"].Id);
        var teamLlm = OrganizationUnit.Create("TEAM-LLM", "Foundation Models & Inference", "Foundation Models & Inference", types["TEAM"].Id, "Large-scale training & low-latency serving.", jpAi.Id, subJpAi, costCenters["CC-JP-AI"].Id);

        return new Dictionary<string, OrganizationUnit>(StringComparer.Ordinal)
        {
            ["GRP-NEXA"] = grpNexa,
            ["DIV-CORP"] = divCorp,
            ["DEPT-FIN"] = deptFin,
            ["DIV-PLATFORM"] = divPlatform,
            ["DEPT-INFRA"] = deptInfra,
            ["TEAM-K8S"] = teamK8s,
            ["TEAM-SEC"] = teamSec,
            ["DEPT-PROD"] = deptProd,
            ["SUB-APAC"] = subApac,
            ["DEPT-APAC-GTM"] = deptApacGtm,
            ["DEPT-TH-ENG"] = deptThEng,
            ["TEAM-TH-CORE"] = teamThCore,
            ["SQUAD-PAY"] = squadPay,
            ["TEAM-TH-DATA"] = teamThData,
            ["SUB-JP-AI"] = subJpAi,
            ["TEAM-LLM"] = teamLlm
        };
    }

    private static Dictionary<string, Position> CreatePositions(
        LegalEntity usHq,
        TaxBranch usSeattleBranch,
        LegalEntity sgApac,
        LegalEntity thEng,
        TaxBranch thChiangMaiBranch,
        LegalEntity jpAi,
        Dictionary<string, OrganizationUnit> ous,
        Dictionary<string, Job> jobs,
        Dictionary<string, JobGrade> grades,
        Dictionary<string, CostCenter> ccs,
        DateTimeOffset now)
    {
        var usHoBranchId = usHq.Branches.First(b => b.IsHeadOffice).Id;
        var sgHoBranchId = sgApac.Branches.First(b => b.IsHeadOffice).Id;
        var thHoBranchId = thEng.Branches.First(b => b.IsHeadOffice).Id;
        var jpHoBranchId = jpAi.Branches.First(b => b.IsHeadOffice).Id;
        var validFrom = new DateTimeOffset(now.Year, 1, 1, 0, 0, 0, TimeSpan.Zero);

        var ceo = Position.Create(
            usHq.Id, ous["GRP-NEXA"].Id, jobs["CEO"].Id, grades["C-SUITE"].Id,
            "POS-CEO", "Chief Executive Officer", "Chief Executive Officer",
            "Group Chief Executive Officer.", ccs["CC-US-EXEC"].Id, usHoBranchId,
            10_000_000m, "USD", headcountCapacity: 1, isPrimaryLeadership: true, validFrom: validFrom);

        var cfo = Position.Create(
            usHq.Id, ous["DIV-CORP"].Id, jobs["CFO"].Id, grades["C-SUITE"].Id,
            "POS-CFO", "Chief Financial Officer", "Chief Financial Officer",
            "Group Chief Financial Officer.", ccs["CC-US-EXEC"].Id, usHoBranchId,
            5_000_000m, "USD", reportsToPositionId: ceo.Id, headcountCapacity: 1, isPrimaryLeadership: true, validFrom: validFrom);

        var cto = Position.Create(
            usHq.Id, ous["DIV-PLATFORM"].Id, jobs["CTO"].Id, grades["C-SUITE"].Id,
            "POS-CTO", "Chief Technology Officer", "Chief Technology Officer",
            "Group Chief Technology Officer.", ccs["CC-US-RND"].Id, usHoBranchId,
            5_000_000m, "USD", reportsToPositionId: ceo.Id, headcountCapacity: 1, isPrimaryLeadership: true, validFrom: validFrom);

        var finCtrl = Position.Create(
            usHq.Id, ous["DEPT-FIN"].Id, jobs["FIN-CTRL"].Id, grades["M2"].Id,
            "POS-FIN-CTRL", "Global Finance Controller", "Global Finance Controller",
            "Oversees group consolidation and statutory reporting.", ccs["CC-US-EXEC"].Id, usHoBranchId,
            250_000m, "USD", reportsToPositionId: cfo.Id, headcountCapacity: 1, isPrimaryLeadership: true, validFrom: validFrom);

        var vpInfra = Position.Create(
            usHq.Id, ous["DEPT-INFRA"].Id, jobs["VP-ENG"].Id, grades["VP"].Id,
            "POS-VP-INFRA", "VP of Cloud Infrastructure", "VP of Cloud Infrastructure",
            "Leads global cloud compute, storage, and security.", ccs["CC-US-CLOUD"].Id, usSeattleBranch.Id,
            1_000_000m, "USD", reportsToPositionId: cto.Id, headcountCapacity: 1, isPrimaryLeadership: true, validFrom: validFrom);

        var emK8s = Position.Create(
            usHq.Id, ous["TEAM-K8S"].Id, jobs["ENG-MGR"].Id, grades["M1"].Id,
            "POS-EM-K8S", "Engineering Manager, Compute & K8s", "Engineering Manager, Compute & K8s",
            "Manages Kubernetes control plane and node autoscaling.", ccs["CC-US-CLOUD"].Id, usSeattleBranch.Id,
            100_000m, "USD", reportsToPositionId: vpInfra.Id, headcountCapacity: 1, isPrimaryLeadership: true, validFrom: validFrom);

        var staffK8s = Position.Create(
            usHq.Id, ous["TEAM-K8S"].Id, jobs["STAFF-SWE"].Id, grades["L6"].Id,
            "POS-STAFF-K8S", "Staff Cloud Infrastructure Engineer", "Staff Cloud Infrastructure Engineer",
            "Technical lead for multi-region Kubernetes clusters.", ccs["CC-US-CLOUD"].Id, usSeattleBranch.Id,
            reportsToPositionId: emK8s.Id, headcountCapacity: 3, isPrimaryLeadership: false, validFrom: validFrom);

        var secArch = Position.Create(
            usHq.Id, ous["TEAM-SEC"].Id, jobs["SEC-ARCH"].Id, grades["L7"].Id,
            "POS-SEC-ARCH", "Principal Security Architect", "Principal Security Architect",
            "Leads zero-trust architecture and security posture.", ccs["CC-US-SEC"].Id, usHoBranchId,
            150_000m, "USD", reportsToPositionId: vpInfra.Id, dottedLinePositionId: cto.Id,
            headcountCapacity: 1, isPrimaryLeadership: true, validFrom: validFrom);

        var pmLead = Position.Create(
            usHq.Id, ous["DEPT-PROD"].Id, jobs["PM-LEAD"].Id, grades["L7"].Id,
            "POS-PM-LEAD", "Principal Product Manager", "Principal Product Manager",
            "Owns core platform product roadmap.", ccs["CC-US-RND"].Id, usHoBranchId,
            100_000m, "USD", reportsToPositionId: cto.Id, headcountCapacity: 1, isPrimaryLeadership: true, validFrom: validFrom);

        var srPd = Position.Create(
            usHq.Id, ous["DEPT-PROD"].Id, jobs["SR-PD"].Id, grades["L5"].Id,
            "POS-SR-PD", "Senior Product Designer", "Senior Product Designer",
            "Design systems and developer console UX.", ccs["CC-US-RND"].Id, usHoBranchId,
            reportsToPositionId: pmLead.Id, headcountCapacity: 4, isPrimaryLeadership: false, validFrom: validFrom);

        var vpApac = Position.Create(
            sgApac.Id, ous["SUB-APAC"].Id, jobs["VP-ENG"].Id, grades["VP"].Id,
            "POS-VP-APAC", "VP & Managing Director, APAC", "VP & Managing Director, APAC",
            "Leads APAC commercial and regional engineering.", ccs["CC-SG-OPS"].Id, sgHoBranchId,
            2_000_000m, "SGD", reportsToPositionId: ceo.Id, headcountCapacity: 1, isPrimaryLeadership: true, validFrom: validFrom);

        var apacAe = Position.Create(
            sgApac.Id, ous["DEPT-APAC-GTM"].Id, jobs["ENT-AE"].Id, grades["L5"].Id,
            "POS-APAC-AE", "Enterprise Account Executive, APAC", "Enterprise Account Executive, APAC",
            "Enterprise cloud sales across ASEAN and ANZ.", ccs["CC-SG-GTM"].Id, sgHoBranchId,
            reportsToPositionId: vpApac.Id, headcountCapacity: 6, isPrimaryLeadership: false, validFrom: validFrom);

        var dirTh = Position.Create(
            thEng.Id, ous["DEPT-TH-ENG"].Id, jobs["DIR-ENG"].Id, grades["D1"].Id,
            "POS-DIR-TH", "Engineering Director, Thailand", "Engineering Director, Thailand",
            "Leads Bangkok and Chiang Mai engineering hubs.", ccs["CC-TH-RND"].Id, thHoBranchId,
            3_000_000m, "THB", reportsToPositionId: vpApac.Id, dottedLinePositionId: cto.Id,
            headcountCapacity: 1, isPrimaryLeadership: true, validFrom: validFrom);

        var emThCore = Position.Create(
            thEng.Id, ous["TEAM-TH-CORE"].Id, jobs["ENG-MGR"].Id, grades["M1"].Id,
            "POS-EM-TH-CORE", "Engineering Manager, Core API", "Engineering Manager, Core API",
            "Manages commerce, billing, and payments squads.", ccs["CC-TH-RND"].Id, thHoBranchId,
            500_000m, "THB", reportsToPositionId: dirTh.Id, headcountCapacity: 1, isPrimaryLeadership: true, validFrom: validFrom);

        var srSwePay = Position.Create(
            thEng.Id, ous["SQUAD-PAY"].Id, jobs["SR-SWE"].Id, grades["L5"].Id,
            "POS-SR-SWE-PAY", "Senior Software Engineer, Payments", "Senior Software Engineer, Payments",
            "Builds multi-currency payment & ledger services.", ccs["CC-TH-RND"].Id, thHoBranchId,
            reportsToPositionId: emThCore.Id, headcountCapacity: 5, isPrimaryLeadership: false, validFrom: validFrom);

        var sweData = Position.Create(
            thEng.Id, ous["TEAM-TH-DATA"].Id, jobs["SWE"].Id, grades["L4"].Id,
            "POS-SWE-DATA", "Software Engineer, Data Platform", "Software Engineer, Data Platform",
            "Builds streaming telemetry and analytics pipelines in Chiang Mai.", ccs["CC-TH-SHARED"].Id, thChiangMaiBranch.Id,
            reportsToPositionId: dirTh.Id, headcountCapacity: 6, isPrimaryLeadership: false, validFrom: validFrom);

        var vpAi = Position.Create(
            jpAi.Id, ous["SUB-JP-AI"].Id, jobs["VP-AI"].Id, grades["VP"].Id,
            "POS-VP-AI", "VP of AI Research, Tokyo", "VP of AI Research, Tokyo",
            "Leads foundation model research and inference optimization.", ccs["CC-JP-AI"].Id, jpHoBranchId,
            150_000_000m, "JPY", reportsToPositionId: cto.Id, headcountCapacity: 1, isPrimaryLeadership: true, validFrom: validFrom);

        var prinAi = Position.Create(
            jpAi.Id, ous["TEAM-LLM"].Id, jobs["PRIN-AI"].Id, grades["L7"].Id,
            "POS-PRIN-AI", "Principal AI Researcher", "Principal AI Researcher",
            "Leads foundation model architecture and alignment.", ccs["CC-JP-AI"].Id, jpHoBranchId,
            15_000_000m, "JPY", reportsToPositionId: vpAi.Id, headcountCapacity: 2, isPrimaryLeadership: true, validFrom: validFrom);

        var srMl = Position.Create(
            jpAi.Id, ous["TEAM-LLM"].Id, jobs["SR-ML"].Id, grades["L5"].Id,
            "POS-SR-ML", "Senior ML Engineer", "Senior ML Engineer",
            "Distributed training and GPU inference kernels.", ccs["CC-JP-AI"].Id, jpHoBranchId,
            reportsToPositionId: prinAi.Id, headcountCapacity: 8, isPrimaryLeadership: false, validFrom: validFrom);

        return new Dictionary<string, Position>(StringComparer.Ordinal)
        {
            ["POS-CEO"] = ceo,
            ["POS-CFO"] = cfo,
            ["POS-CTO"] = cto,
            ["POS-FIN-CTRL"] = finCtrl,
            ["POS-VP-INFRA"] = vpInfra,
            ["POS-EM-K8S"] = emK8s,
            ["POS-STAFF-K8S"] = staffK8s,
            ["POS-SEC-ARCH"] = secArch,
            ["POS-PM-LEAD"] = pmLead,
            ["POS-SR-PD"] = srPd,
            ["POS-VP-APAC"] = vpApac,
            ["POS-APAC-AE"] = apacAe,
            ["POS-DIR-TH"] = dirTh,
            ["POS-EM-TH-CORE"] = emThCore,
            ["POS-SR-SWE-PAY"] = srSwePay,
            ["POS-SWE-DATA"] = sweData,
            ["POS-VP-AI"] = vpAi,
            ["POS-PRIN-AI"] = prinAi,
            ["POS-SR-ML"] = srMl
        };
    }

    private static void AssignOrganizationUnitManagers(
        Dictionary<string, OrganizationUnit> ous,
        Dictionary<string, Position> positions)
    {
        ous["GRP-NEXA"].AssignManagerPosition(positions["POS-CEO"]);
        ous["DIV-CORP"].AssignManagerPosition(positions["POS-CFO"]);
        ous["DEPT-FIN"].AssignManagerPosition(positions["POS-FIN-CTRL"]);
        ous["DIV-PLATFORM"].AssignManagerPosition(positions["POS-CTO"]);
        ous["DEPT-INFRA"].AssignManagerPosition(positions["POS-VP-INFRA"]);
        ous["TEAM-K8S"].AssignManagerPosition(positions["POS-EM-K8S"]);
        ous["TEAM-SEC"].AssignManagerPosition(positions["POS-SEC-ARCH"]);
        ous["DEPT-PROD"].AssignManagerPosition(positions["POS-PM-LEAD"]);
        ous["SUB-APAC"].AssignManagerPosition(positions["POS-VP-APAC"]);
        ous["DEPT-TH-ENG"].AssignManagerPosition(positions["POS-DIR-TH"]);
        ous["TEAM-TH-CORE"].AssignManagerPosition(positions["POS-EM-TH-CORE"]);
        ous["SUB-JP-AI"].AssignManagerPosition(positions["POS-VP-AI"]);
        ous["TEAM-LLM"].AssignManagerPosition(positions["POS-PRIN-AI"]);
    }

    private static List<PositionDelegation> CreatePositionDelegations(
        LegalEntity usHq,
        LegalEntity thEng,
        Dictionary<string, Position> positions,
        DateTimeOffset now) =>
    [
        PositionDelegation.Create(
            legalEntityId: usHq.Id,
            delegatorPositionId: positions["POS-CTO"].Id,
            delegatePositionId: positions["POS-VP-INFRA"].Id,
            delegationType: DelegationType.ApprovalOnly,
            validFrom: now.AddDays(-2),
            validTo: now.AddDays(12),
            reason: "Executive summit travel — cloud infrastructure approvals delegated."),

        PositionDelegation.Create(
            legalEntityId: usHq.Id,
            delegatorPositionId: positions["POS-CFO"].Id,
            delegatePositionId: positions["POS-FIN-CTRL"].Id,
            delegationType: DelegationType.SpecificDocuments,
            validFrom: now.AddDays(-5),
            validTo: now.AddDays(25),
            documentTypeScope: "PR,PO,INVOICE",
            reason: "Standing quarterly procurement & invoice sign-off delegation."),

        PositionDelegation.Create(
            legalEntityId: thEng.Id,
            delegatorPositionId: positions["POS-DIR-TH"].Id,
            delegatePositionId: positions["POS-EM-TH-CORE"].Id,
            delegationType: DelegationType.ActingFull,
            validFrom: now.AddDays(-1),
            validTo: now.AddDays(6),
            reason: "Annual leave acting authority for Bangkok Engineering Center.")
    ];
}
