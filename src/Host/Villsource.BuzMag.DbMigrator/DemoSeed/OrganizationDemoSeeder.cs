using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Villsource.FSH.Modules.Organization.Contracts.Constants;
using Villsource.FSH.Modules.Organization.Data;
using Villsource.FSH.Modules.Organization.Domain.CostCenters;
using Villsource.FSH.Modules.Organization.Domain.JobGrades;
using Villsource.FSH.Modules.Organization.Domain.Jobs;
using Villsource.FSH.Modules.Organization.Domain.LegalEntities;
using Villsource.FSH.Modules.Organization.Domain.OrganizationUnits;
using Villsource.FSH.Modules.Organization.Domain.PositionDelegations;
using Villsource.ObjectValue;
using Position = Villsource.FSH.Modules.Organization.Domain.Positions.Position;

namespace Villsource.BuzMag.DbMigrator.DemoSeed;

internal sealed class OrganizationDemoSeeder(
    OrganizationDbContext dbContext,
    ILogger<OrganizationDemoSeeder> logger)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        const string rootLegalEntityCode = "VS-TH-HQ";
        if (await dbContext.LegalEntities
                .AnyAsync(x => x.Code == rootLegalEntityCode, cancellationToken)
                .ConfigureAwait(false))
        {
            logger.LogInformation("Organization enterprise demo data already exists. Skipping demo seeding.");
            return;
        }

        logger.LogInformation("Seeding Organization module enterprise demo data...");

        // =========================================================================
        // STEP 1: LegalEntity & TaxBranches (นิติบุคคลและสาขาทางภาษี 3 ภูมิภาค)
        // =========================================================================
        var hqAddress = new Address(
            houseNo: "99/9",
            subDistrict: "ห้วยขวาง",
            district: "ห้วยขวาง",
            province: "กรุงเทพมหานคร",
            postalCode: "10310",
            country: "Thailand",
            building: "อาคารวิลซอร์ส ทาวเวอร์ ชั้น 18-22",
            road: "พระราม 9");

        var cnxAddress = new Address(
            houseNo: "123/4",
            subDistrict: "สุเทพ",
            district: "เมืองเชียงใหม่",
            province: "เชียงใหม่",
            postalCode: "50200",
            country: "Thailand",
            building: "โครงการวัน นิมมาน อาคาร B ชั้น 4",
            road: "นิมมานเหมินท์");

        var rayongAddress = new Address(
            houseNo: "88/1",
            subDistrict: "มาบตาพุด",
            district: "เมืองระยอง",
            province: "ระยอง",
            postalCode: "21150",
            country: "Thailand",
            moo: "4",
            building: "นิคมอุตสาหกรรมอีสเทิร์นซีบอร์ด",
            road: "สุขุมวิท");

        var legalEntity = LegalEntity.Create(
            code: rootLegalEntityCode,
            countryCode: "TH",
            legalName: "บริษัท วิลซอร์ส คอร์ปอเรชั่น จำกัด (มหาชน)",
            internationalName: "Villsource Corporation Public Company Limited",
            taxId: "0107567000123",
            baseCurrency: "THB",
            registrationNumber: "0107567000123",
            profile: new CompanyProfile
            {
                TradeName = "Villsource Enterprise",
                Website = "https://www.villsource.co.th",
                ContactEmail = "corporate@villsource.co.th",
                ContactPhone = "+66-2-000-9999"
            },
            headOfficeAddress: hqAddress,
            headOfficeBranchCode: "00000",
            headOfficeName: "สำนักงานใหญ่ (พระราม 9)",
            headOfficeInternationalName: "Head Office (Rama IX)",
            headOfficeRegisteredDate: new DateTimeOffset(2018, 1, 15, 0, 0, 0, TimeSpan.Zero));

        var branchCnx = legalEntity.AddBranch(
            branchCode: "00001",
            name: "สาขาเชียงใหม่ (ศูนย์วิจัยและพัฒนาซอฟต์แวร์)",
            internationalName: "Chiang Mai Branch (Software R&D Hub)",
            registeredAddress: cnxAddress,
            isHeadOffice: false,
            registeredDate: new DateTimeOffset(2021, 6, 1, 0, 0, 0, TimeSpan.Zero));

        var branchRayong = legalEntity.AddBranch(
            branchCode: "00002",
            name: "สาขาระยอง (ศูนย์ปฏิบัติการและคลังสินค้าภาคตะวันออก)",
            internationalName: "Rayong Branch (Eastern Operations & Fulfillment Center)",
            registeredAddress: rayongAddress,
            isHeadOffice: false,
            registeredDate: new DateTimeOffset(2022, 9, 15, 0, 0, 0, TimeSpan.Zero));

        var branchHq = legalEntity.Branches.First(b => b.IsHeadOffice);

        await dbContext.LegalEntities.AddAsync(legalEntity, cancellationToken).ConfigureAwait(false);

        // =========================================================================
        // STEP 2: JobGrades (ระดับชั้นตำแหน่งงาน 8 ระดับ ตามโครงสร้างองค์กรใหญ่)
        // =========================================================================
        var jgCeo = JobGrade.Create("JG-10", "ประธานเจ้าหน้าที่บริหาร", "Chief Executive Officer (CEO)", 100, "Top Executive");
        var jgCLevel = JobGrade.Create("JG-09", "ผู้บริหารระดับสูง (C-Suite)", "C-Level Executive", 90, "Chief Officers (CTO, CFO, COO, CHRO, CCO)");
        var jgVp = JobGrade.Create("JG-08", "รองกรรมการผู้จัดการ / ผู้อำนวยการฝ่ายอาวุโส", "Vice President / Senior Director", 80, "Division Heads");
        var jgDir = JobGrade.Create("JG-07", "ผู้อำนวยการฝ่าย", "Director", 70, "Department Directors");
        var jgMgr = JobGrade.Create("JG-06", "ผู้จัดการแผนก", "Manager", 60, "Department / Section Managers");
        var jgLead = JobGrade.Create("JG-05", "หัวหน้าทีม / ผู้เชี่ยวชาญหลัก", "Team Lead / Principal Specialist", 50, "Team Leads & Principal Individual Contributors");
        var jgSenior = JobGrade.Create("JG-04", "เจ้าหน้าที่อาวุโส / วิศวกรอาวุโส", "Senior Specialist / Senior Engineer", 40, "Senior Level");
        var jgStaff = JobGrade.Create("JG-03", "เจ้าหน้าที่ปฏิบัติการ / วิศวกร", "Specialist / Engineer / Officer", 30, "Operational & Engineering Staff");

        await dbContext.JobGrades.AddRangeAsync(
            [jgCeo, jgCLevel, jgVp, jgDir, jgMgr, jgLead, jgSenior, jgStaff],
            cancellationToken).ConfigureAwait(false);

        // =========================================================================
        // STEP 3: Jobs (มาตรฐานตำแหน่งงานครอบคลุมทุกสายอาชีพ)
        // =========================================================================
        // Executive
        var jobCeo = Job.Create("JOB-CEO", "ประธานเจ้าหน้าที่บริหาร", "Chief Executive Officer", jgCeo.Id, "Executive Management");
        var jobCto = Job.Create("JOB-CTO", "ประธานเจ้าหน้าที่ฝ่ายเทคโนโลยี", "Chief Technology Officer", jgCLevel.Id, "Technology");
        var jobCfo = Job.Create("JOB-CFO", "ประธานเจ้าหน้าที่ฝ่ายการเงิน", "Chief Financial Officer", jgCLevel.Id, "Finance");
        var jobCoo = Job.Create("JOB-COO", "ประธานเจ้าหน้าที่ฝ่ายปฏิบัติการ", "Chief Operating Officer", jgCLevel.Id, "Operations");
        var jobChro = Job.Create("JOB-CHRO", "ประธานเจ้าหน้าที่ฝ่ายทรัพยากรบุคคล", "Chief Human Resources Officer", jgCLevel.Id, "Human Resources");

        // Technology & Product
        var jobVpEng = Job.Create("JOB-VP-ENG", "รองกรรมการผู้จัดการฝ่ายวิศวกรรมซอฟต์แวร์", "VP of Engineering", jgVp.Id, "Engineering");
        var jobDirProd = Job.Create("JOB-DIR-PROD", "ผู้อำนวยการฝ่ายบริหารผลิตภัณฑ์", "Director of Product Management", jgDir.Id, "Product");
        var jobEngMgr = Job.Create("JOB-ENG-MGR", "ผู้จัดการฝ่ายวิศวกรรมซอฟต์แวร์", "Engineering Manager", jgMgr.Id, "Engineering");
        var jobSecMgr = Job.Create("JOB-SEC-MGR", "ผู้จัดการฝ่ายความมั่นคงปลอดภัยไซเบอร์", "Information Security Manager", jgMgr.Id, "Security & Infrastructure");
        var jobPrincipalArch = Job.Create("JOB-PRN-ARCH", "สถาปนิกระบบซอฟต์แวร์หลัก", "Principal Software Architect", jgLead.Id, "Engineering");
        var jobTechLead = Job.Create("JOB-TECH-LEAD", "หัวหน้าทีมพัฒนาซอฟต์แวร์", "Software Engineering Tech Lead", jgLead.Id, "Engineering");
        var jobSrSwe = Job.Create("JOB-SR-SWE", "วิศวกรซอฟต์แวร์อาวุโส", "Senior Software Engineer", jgSenior.Id, "Engineering");
        var jobSwe = Job.Create("JOB-SWE", "วิศวกรซอฟต์แวร์", "Software Engineer", jgStaff.Id, "Engineering");
        var jobPm = Job.Create("JOB-PM", "ผู้จัดการผลิตภัณฑ์", "Product Manager", jgMgr.Id, "Product");

        // Finance, Accounting & Procurement
        var jobFinDir = Job.Create("JOB-DIR-FIN", "ผู้อำนวยการฝ่ายบัญชีและการเงิน", "Director of Finance & Accounting", jgDir.Id, "Finance");
        var jobAcctMgr = Job.Create("JOB-ACCT-MGR", "ผู้จัดการฝ่ายบัญชีและการรายงานทางการเงิน", "Accounting & Reporting Manager", jgMgr.Id, "Accounting");
        var jobProcMgr = Job.Create("JOB-PROC-MGR", "ผู้จัดการฝ่ายจัดซื้อจัดจ้างกลาง", "Procurement Manager", jgMgr.Id, "Procurement");
        var jobSrAcct = Job.Create("JOB-SR-ACCT", "นักบัญชีอาวุโส", "Senior Accountant", jgSenior.Id, "Accounting");

        // Commercial & Operations
        var jobVpSales = Job.Create("JOB-VP-SALES", "รองกรรมการผู้จัดการฝ่ายขายและการตลาด", "VP of Sales & Marketing", jgVp.Id, "Commercial");
        var jobSalesMgr = Job.Create("JOB-SALES-MGR", "ผู้จัดการฝ่ายขายองค์กร", "Enterprise Sales Manager", jgMgr.Id, "Sales");
        var jobOpsDir = Job.Create("JOB-DIR-OPS", "ผู้อำนวยการฝ่ายปฏิบัติการและโลจิสติกส์", "Director of Operations & Logistics", jgDir.Id, "Operations");
        var jobPlantMgr = Job.Create("JOB-PLANT-MGR", "ผู้จัดการศูนย์ปฏิบัติการระยอง", "Rayong Operations Center Manager", jgMgr.Id, "Operations");
        var jobOpsSpec = Job.Create("JOB-OPS-SPEC", "เจ้าหน้าที่ปฏิบัติการและคลังสินค้า", "Operations & Warehouse Specialist", jgStaff.Id, "Operations");

        // HR & Legal
        var jobHrMgr = Job.Create("JOB-HR-MGR", "ผู้จัดการฝ่ายทรัพยากรบุคคลและพัฒนาองค์กร", "HR & People Operations Manager", jgMgr.Id, "Human Resources");
        var jobLegalMgr = Job.Create("JOB-LEG-MGR", "ผู้จัดการฝ่ายกฎหมายและกำกับดูแลกิจการ", "Legal & Compliance Manager", jgMgr.Id, "Legal & Compliance");

        await dbContext.Jobs.AddRangeAsync(
            [
                jobCeo, jobCto, jobCfo, jobCoo, jobChro,
                jobVpEng, jobDirProd, jobEngMgr, jobSecMgr, jobPrincipalArch, jobTechLead, jobSrSwe, jobSwe, jobPm,
                jobFinDir, jobAcctMgr, jobProcMgr, jobSrAcct,
                jobVpSales, jobSalesMgr, jobOpsDir, jobPlantMgr, jobOpsSpec,
                jobHrMgr, jobLegalMgr
            ],
            cancellationToken).ConfigureAwait(false);

        // =========================================================================
        // STEP 4: OrganizationUnits (โครงสร้างองค์กร 4 ระดับ: Company -> Division -> Department -> Team)
        // =========================================================================
        // Level 0: Company Root
        var ouCorp = OrganizationUnit.Create(
            code: "OU-CORP",
            name: "บริษัท วิลซอร์ส คอร์ปอเรชั่น จำกัด (มหาชน)",
            internationalName: "Villsource Corporation HQ",
            type: "COMPANY",
            description: "สำนักงานใหญ่และศูนย์บริหารงานกลาง",
            legalEntityId: legalEntity.Id );

        // Level 1: Divisions (5 สายงานหลัก)
        var ouDivTech = OrganizationUnit.Create(
            code: "DIV-TECH",
            name: "สายงานเทคโนโลยีและนวัตกรรมดิจิทัล",
            internationalName: "Technology & Digital Innovation Division",
            type: "DIVISION",
            legalEntityId: legalEntity.Id,
            parent: ouCorp);

        var ouDivComm = OrganizationUnit.Create(
            code: "DIV-COMM",
            name: "สายงานการพาณิชย์และการเติบโตธุรกิจ",
            internationalName: "Commercial & Business Growth Division",
            type: "DIVISION",
            legalEntityId: legalEntity.Id,
            parent: ouCorp);

        var ouDivOps = OrganizationUnit.Create(
            code: "DIV-OPS",
            name: "สายงานปฏิบัติการและซัพพลายเชน",
            internationalName: "Operations & Supply Chain Division",
            type: "DIVISION",
            legalEntityId: legalEntity.Id,
            parent: ouCorp);

        var ouDivFin = OrganizationUnit.Create(
            code: "DIV-FIN",
            name: "สายงานบัญชี การเงิน และจัดซื้อกลาง",
            internationalName: "Finance, Accounting & Procurement Division",
            type: "DIVISION",
            legalEntityId: legalEntity.Id,
            parent: ouCorp);

        var ouDivPeople = OrganizationUnit.Create(
            code: "DIV-PPL",
            name: "สายงานทรัพยากรบุคคลและกฎหมายองค์กร",
            internationalName: "People, Culture & Corporate Legal Division",
            type: "DIVISION",
            legalEntityId: legalEntity.Id,
            parent: ouCorp);

        // Level 2: Departments (ฝ่ายงานภายใต้แต่ละ Division)
        var ouDeptEng = OrganizationUnit.Create(
            code: "DEPT-ENG",
            name: "ฝ่ายวิศวกรรมซอฟต์แวร์และแพลตฟอร์ม",
            internationalName: "Software & Platform Engineering Department",
            type: "DEPARTMENT",
            legalEntityId: legalEntity.Id,
            parent: ouDivTech );

        var ouDeptProd = OrganizationUnit.Create(
            code: "DEPT-PROD",
            name: "ฝ่ายบริหารผลิตภัณฑ์และประสบการณ์ผู้ใช้",
            internationalName: "Product Management & UX Department",
            type: "DEPARTMENT",
            legalEntityId: legalEntity.Id,
            parent: ouDivTech );

        var ouDeptSec = OrganizationUnit.Create(
            code: "DEPT-SEC",
            name: "ฝ่ายโครงสร้างพื้นฐานและความปลอดภัยไซเบอร์",
            internationalName: "Infrastructure & Cyber Security Department",
            type: "DEPARTMENT",
            legalEntityId: legalEntity.Id,
            parent: ouDivTech
            );

        var ouDeptSales = OrganizationUnit.Create(
            code: "DEPT-SALES",
            name: "ฝ่ายขายลูกค้าองค์กร",
            internationalName: "Enterprise Sales Department",
            type: "DEPARTMENT",
            legalEntityId: legalEntity.Id,
            parent: ouDivComm);

        var ouDeptRayongOps = OrganizationUnit.Create(
            code: "DEPT-RYG-OPS",
            name: "ฝ่ายปฏิบัติการและคลังสินค้าภาคตะวันออก (ระยอง)",
            internationalName: "Eastern Operations & Fulfillment Department (Rayong)",
            type: "DEPARTMENT",
            legalEntityId: legalEntity.Id,
            parent: ouDivOps);

        var ouDeptAcct = OrganizationUnit.Create(
            code: "DEPT-ACCT",
            name: "ฝ่ายบัญชีและภาษีอากร",
            internationalName: "Accounting & Taxation Department",
            type: "DEPARTMENT",
            legalEntityId: legalEntity.Id,
            parent: ouDivFin);

        var ouDeptProc = OrganizationUnit.Create(
            code: "DEPT-PROC",
            name: "ฝ่ายจัดซื้อจัดจ้างกลาง",
            internationalName: "Central Procurement Department",
            type: "DEPARTMENT",
            legalEntityId: legalEntity.Id,
            parent: ouDivFin);

        var ouDeptHr = OrganizationUnit.Create(
            code: "DEPT-HR",
            name: "ฝ่ายบริหารทรัพยากรบุคคลและเงินเดือน",
            internationalName: "Human Resources & Payroll Department",
            type: "DEPARTMENT",
            legalEntityId: legalEntity.Id,
            parent: ouDivPeople);

        var ouDeptLegal = OrganizationUnit.Create(
            code: "DEPT-LEGAL",
            name: "ฝ่ายกฎหมายสัญญาและกำกับดูแลกิจการ",
            internationalName: "Legal & Corporate Compliance Department",
            type: "DEPARTMENT",
            legalEntityId: legalEntity.Id,
            parent: ouDivPeople);

        // Level 3: Teams (ทีมปฏิบัติการเฉพาะทาง)
        var ouTeamCoreErp = OrganizationUnit.Create(
            code: "TEAM-ERP",
            name: "ทีมพัฒนา Core ERP & Finance Engine",
            internationalName: "Core ERP & Finance Engine Team",
            type: "TEAM",
            legalEntityId: legalEntity.Id,
            parent: ouDeptEng);

        var ouTeamCnxRnd = OrganizationUnit.Create(
            code: "TEAM-CNX-RND",
            name: "ทีมวิจัยและพัฒนา AI / Data (สาขาเชียงใหม่)",
            internationalName: "AI & Data R&D Team (Chiang Mai Hub)",
            type: "TEAM",
            legalEntityId: legalEntity.Id,
            parent: ouDeptEng);

        // Edge Case: ทีมที่ยังไม่มี ManagerPosition (Vacant Manager) เพื่อทดสอบ Skip-Level Approval Escalation
        var ouTeamDevOps = OrganizationUnit.Create(
            code: "TEAM-DEVOPS",
            name: "ทีม Cloud Platform & SRE (Vacant Manager Test)",
            internationalName: "Cloud Platform & SRE Team",
            type: "TEAM",
            legalEntityId: legalEntity.Id,
            parent: ouDeptSec );

        await dbContext.OrganizationUnits.AddRangeAsync(
            [
                ouCorp,
                ouDivTech, ouDivComm, ouDivOps, ouDivFin, ouDivPeople,
                ouDeptEng, ouDeptProd, ouDeptSec, ouDeptSales, ouDeptRayongOps, ouDeptAcct, ouDeptProc, ouDeptHr, ouDeptLegal,
                ouTeamCoreErp, ouTeamCnxRnd, ouTeamDevOps
            ],
            cancellationToken).ConfigureAwait(false);

        // =========================================================================
        // STEP 5: CostCenters (ศูนย์ต้นทุนครบทุกหมวด IAS 1 / TFRS 1)
        // =========================================================================
        var ccCorp = CostCenter.Create(
            legalEntityId: legalEntity.Id,
            code: "CC-1000",
            name: "ศูนย์ต้นทุนบริหารส่วนกลาง",
            internationalName: "Corporate Executive Cost Center",
            functionType: CostCenterFunctionType.Administrative,
            defaultTaxBranchId: branchHq.Id,
            orgUnitId: ouCorp.Id);

        var ccTech = CostCenter.Create(
            legalEntityId: legalEntity.Id,
            code: "CC-2000",
            name: "ศูนย์ต้นทุนสายงานเทคโนโลยี",
            internationalName: "Technology Division Cost Center",
            functionType: CostCenterFunctionType.ResearchAndDevelopment,
            parentCostCenterId: ccCorp.Id,
            defaultTaxBranchId: branchHq.Id,
            orgUnitId: ouDivTech.Id);

        var ccCoreErp = CostCenter.Create(
            legalEntityId: legalEntity.Id,
            code: "CC-2110",
            name: "ศูนย์ต้นทุนวิจัยพัฒนา Core ERP (กรุงเทพฯ)",
            internationalName: "Core ERP R&D Cost Center (BKK)",
            functionType: CostCenterFunctionType.ResearchAndDevelopment,
            parentCostCenterId: ccTech.Id,
            defaultTaxBranchId: branchHq.Id,
            orgUnitId: ouTeamCoreErp.Id);

        var ccCnxRnd = CostCenter.Create(
            legalEntityId: legalEntity.Id,
            code: "CC-2120",
            name: "ศูนย์ต้นทุนวิจัยพัฒนา AI & Data (เชียงใหม่)",
            internationalName: "AI & Data R&D Cost Center (CNX)",
            functionType: CostCenterFunctionType.ResearchAndDevelopment,
            parentCostCenterId: ccTech.Id,
            defaultTaxBranchId: branchCnx.Id,
            orgUnitId: ouTeamCnxRnd.Id);

        var ccComm = CostCenter.Create(
            legalEntityId: legalEntity.Id,
            code: "CC-3000",
            name: "ศูนย์ต้นทุนการขายและการตลาด",
            internationalName: "Sales & Marketing Cost Center",
            functionType: CostCenterFunctionType.SellingAndDistribution,
            parentCostCenterId: ccCorp.Id,
            defaultTaxBranchId: branchHq.Id,
            orgUnitId: ouDivComm.Id);

        var ccRayongOps = CostCenter.Create(
            legalEntityId: legalEntity.Id,
            code: "CC-4100",
            name: "ศูนย์ต้นทุนปฏิบัติการและคลังสินค้า (ระยอง)",
            internationalName: "Eastern Operations & Fulfillment Cost Center (Rayong)",
            functionType: CostCenterFunctionType.CostOfSales,
            parentCostCenterId: ccCorp.Id,
            defaultTaxBranchId: branchRayong.Id,
            orgUnitId: ouDeptRayongOps.Id);

        var ccSharedServices = CostCenter.Create(
            legalEntityId: legalEntity.Id,
            code: "CC-6000",
            name: "ศูนย์ต้นทุนบริการส่วนกลางและ IT/HR Pool",
            internationalName: "Shared Services Allocation Pool",
            functionType: CostCenterFunctionType.SharedServices,
            parentCostCenterId: ccCorp.Id,
            defaultTaxBranchId: branchHq.Id,
            orgUnitId: ouDivPeople.Id);

        await dbContext.CostCenters.AddRangeAsync(
            [ccCorp, ccTech, ccCoreErp, ccCnxRnd, ccComm, ccRayongOps, ccSharedServices],
            cancellationToken).ConfigureAwait(false);

        // =========================================================================
        // STEP 6: Positions (ตำแหน่งงานทุกสายงาน + Reporting Lines + Headcount Capacity)
        // =========================================================================
        // C-Suite
        var posCeo = Position.Create(
            legalEntityId: legalEntity.Id, orgUnitId: ouCorp.Id, jobId: jobCeo.Id, jobGradeId: jgCeo.Id,
            code: "POS-CEO-001", title: "ประธานเจ้าหน้าที่บริหาร", internationalTitle: "Chief Executive Officer",
            costCenterId: ccCorp.Id, headcountCapacity: 1, isPrimaryLeadership: true);

        var posCto = Position.Create(
            legalEntityId: legalEntity.Id, orgUnitId: ouDivTech.Id, jobId: jobCto.Id, jobGradeId: jgCLevel.Id,
            code: "POS-CTO-001", title: "ประธานเจ้าหน้าที่ฝ่ายเทคโนโลยี", internationalTitle: "Chief Technology Officer",
            costCenterId: ccTech.Id, reportsToPositionId: posCeo.Id, headcountCapacity: 1, isPrimaryLeadership: true);

        var posCfo = Position.Create(
            legalEntityId: legalEntity.Id, orgUnitId: ouDivFin.Id, jobId: jobCfo.Id, jobGradeId: jgCLevel.Id,
            code: "POS-CFO-001", title: "ประธานเจ้าหน้าที่ฝ่ายการเงิน", internationalTitle: "Chief Financial Officer",
            costCenterId: ccCorp.Id, reportsToPositionId: posCeo.Id, headcountCapacity: 1, isPrimaryLeadership: true);

        var posCoo = Position.Create(
            legalEntityId: legalEntity.Id, orgUnitId: ouDivOps.Id, jobId: jobCoo.Id, jobGradeId: jgCLevel.Id,
            code: "POS-COO-001", title: "ประธานเจ้าหน้าที่ฝ่ายปฏิบัติการ", internationalTitle: "Chief Operating Officer",
            costCenterId: ccRayongOps.Id, reportsToPositionId: posCeo.Id, headcountCapacity: 1, isPrimaryLeadership: true);

        var posChro = Position.Create(
            legalEntityId: legalEntity.Id, orgUnitId: ouDivPeople.Id, jobId: jobChro.Id, jobGradeId: jgCLevel.Id,
            code: "POS-CHRO-001", title: "ประธานเจ้าหน้าที่ฝ่ายทรัพยากรบุคคล", internationalTitle: "Chief Human Resources Officer",
            costCenterId: ccSharedServices.Id, reportsToPositionId: posCeo.Id, headcountCapacity: 1, isPrimaryLeadership: true);

        // Technology & Product Leadership + Staff
        var posVpEng = Position.Create(
            legalEntityId: legalEntity.Id, orgUnitId: ouDeptEng.Id, jobId: jobVpEng.Id, jobGradeId: jgVp.Id,
            code: "POS-VP-ENG-001", title: "รองกรรมการผู้จัดการฝ่ายวิศวกรรมซอฟต์แวร์", internationalTitle: "VP of Engineering",
            costCenterId: ccTech.Id, reportsToPositionId: posCto.Id, headcountCapacity: 1, isPrimaryLeadership: true);

        var posDirProd = Position.Create(
            legalEntityId: legalEntity.Id, orgUnitId: ouDeptProd.Id, jobId: jobDirProd.Id, jobGradeId: jgDir.Id,
            code: "POS-DIR-PROD-001", title: "ผู้อำนวยการฝ่ายบริหารผลิตภัณฑ์", internationalTitle: "Director of Product Management",
            costCenterId: ccTech.Id, reportsToPositionId: posCto.Id, headcountCapacity: 1, isPrimaryLeadership: true);

        var posSecMgr = Position.Create(
            legalEntityId: legalEntity.Id, orgUnitId: ouDeptSec.Id, jobId: jobSecMgr.Id, jobGradeId: jgMgr.Id,
            code: "POS-SEC-MGR-001", title: "ผู้จัดการฝ่ายความมั่นคงปลอดภัยไซเบอร์", internationalTitle: "Information Security Manager",
            costCenterId: ccTech.Id, reportsToPositionId: posCto.Id, headcountCapacity: 1, isPrimaryLeadership: true);

        var posPrincipalArch = Position.Create(
            legalEntityId: legalEntity.Id, orgUnitId: ouDeptEng.Id, jobId: jobPrincipalArch.Id, jobGradeId: jgLead.Id,
            code: "POS-PRN-ARCH-001", title: "สถาปนิกระบบซอฟต์แวร์หลัก", internationalTitle: "Principal Software Architect",
            costCenterId: ccTech.Id, reportsToPositionId: posVpEng.Id, dottedLinePositionId: posCto.Id,
            headcountCapacity: 2, isPrimaryLeadership: false);

        var posErpLead = Position.Create(
            legalEntityId: legalEntity.Id, orgUnitId: ouTeamCoreErp.Id, jobId: jobTechLead.Id, jobGradeId: jgLead.Id,
            code: "POS-ERP-LEAD-001", title: "หัวหน้าทีมพัฒนา Core ERP", internationalTitle: "Core ERP Tech Lead",
            costCenterId: ccCoreErp.Id, reportsToPositionId: posVpEng.Id, headcountCapacity: 1, isPrimaryLeadership: true);

        var posErpSwePool = Position.Create(
            legalEntityId: legalEntity.Id, orgUnitId: ouTeamCoreErp.Id, jobId: jobSwe.Id, jobGradeId: jgStaff.Id,
            code: "POS-ERP-SWE-001", title: "วิศวกรซอฟต์แวร์ (Core ERP)", internationalTitle: "Software Engineer (Core ERP)",
            costCenterId: ccCoreErp.Id, reportsToPositionId: posErpLead.Id, headcountCapacity: 8, isPrimaryLeadership: false);

        var posCnxMgr = Position.Create(
            legalEntityId: legalEntity.Id, orgUnitId: ouTeamCnxRnd.Id, jobId: jobEngMgr.Id, jobGradeId: jgMgr.Id,
            code: "POS-CNX-MGR-001", title: "ผู้จัดการศูนย์วิจัยและพัฒนา เชียงใหม่", internationalTitle: "Chiang Mai R&D Center Manager",
            costCenterId: ccCnxRnd.Id, reportsToPositionId: posVpEng.Id, headcountCapacity: 1, isPrimaryLeadership: true);

        var posCnxSrSwe = Position.Create(
            legalEntityId: legalEntity.Id, orgUnitId: ouTeamCnxRnd.Id, jobId: jobSrSwe.Id, jobGradeId: jgSenior.Id,
            code: "POS-CNX-SR-001", title: "วิศวกรซอฟต์แวร์อาวุโส AI/Data (เชียงใหม่)", internationalTitle: "Senior AI/Data Engineer (CNX)",
            costCenterId: ccCnxRnd.Id, reportsToPositionId: posCnxMgr.Id, headcountCapacity: 4, isPrimaryLeadership: false);

        // Commercial & Operations
        var posVpSales = Position.Create(
            legalEntityId: legalEntity.Id, orgUnitId: ouDivComm.Id, jobId: jobVpSales.Id, jobGradeId: jgVp.Id,
            code: "POS-VP-SALES-001", title: "รองกรรมการผู้จัดการฝ่ายขายและการตลาด", internationalTitle: "VP of Sales & Marketing",
            costCenterId: ccComm.Id, reportsToPositionId: posCeo.Id, headcountCapacity: 1, isPrimaryLeadership: true);

        var posSalesMgr = Position.Create(
            legalEntityId: legalEntity.Id, orgUnitId: ouDeptSales.Id, jobId: jobSalesMgr.Id, jobGradeId: jgMgr.Id,
            code: "POS-SALES-MGR-001", title: "ผู้จัดการฝ่ายขายลูกค้าองค์กร", internationalTitle: "Enterprise Sales Manager",
            costCenterId: ccComm.Id, reportsToPositionId: posVpSales.Id, headcountCapacity: 1, isPrimaryLeadership: true);

        var posPlantMgr = Position.Create(
            legalEntityId: legalEntity.Id, orgUnitId: ouDeptRayongOps.Id, jobId: jobPlantMgr.Id, jobGradeId: jgMgr.Id,
            code: "POS-RYG-MGR-001", title: "ผู้จัดการศูนย์ปฏิบัติการและคลังสินค้าระยอง", internationalTitle: "Rayong Operations Manager",
            costCenterId: ccRayongOps.Id, reportsToPositionId: posCoo.Id, headcountCapacity: 1, isPrimaryLeadership: true);

        var posRayongOpsPool = Position.Create(
            legalEntityId: legalEntity.Id, orgUnitId: ouDeptRayongOps.Id, jobId: jobOpsSpec.Id, jobGradeId: jgStaff.Id,
            code: "POS-RYG-OPS-001", title: "เจ้าหน้าที่ปฏิบัติการและคลังสินค้า (ระยอง)", internationalTitle: "Operations & Warehouse Specialist (Rayong)",
            costCenterId: ccRayongOps.Id, reportsToPositionId: posPlantMgr.Id, headcountCapacity: 15, isPrimaryLeadership: false);

        // Finance, Accounting & Procurement
        var posAcctMgr = Position.Create(
            legalEntityId: legalEntity.Id, orgUnitId: ouDeptAcct.Id, jobId: jobAcctMgr.Id, jobGradeId: jgMgr.Id,
            code: "POS-ACCT-MGR-001", title: "ผู้จัดการฝ่ายบัญชีและภาษีอากร", internationalTitle: "Accounting & Tax Manager",
            costCenterId: ccCorp.Id, reportsToPositionId: posCfo.Id, headcountCapacity: 1, isPrimaryLeadership: true);

        var posSrAcct = Position.Create(
            legalEntityId: legalEntity.Id, orgUnitId: ouDeptAcct.Id, jobId: jobSrAcct.Id, jobGradeId: jgSenior.Id,
            code: "POS-SR-ACCT-001", title: "นักบัญชีอาวุโส", internationalTitle: "Senior Accountant",
            costCenterId: ccCorp.Id, reportsToPositionId: posAcctMgr.Id, headcountCapacity: 3, isPrimaryLeadership: false);

        var posProcMgr = Position.Create(
            legalEntityId: legalEntity.Id, orgUnitId: ouDeptProc.Id, jobId: jobProcMgr.Id, jobGradeId: jgMgr.Id,
            code: "POS-PROC-MGR-001", title: "ผู้จัดการฝ่ายจัดซื้อจัดจ้างกลาง", internationalTitle: "Central Procurement Manager",
            costCenterId: ccCorp.Id, reportsToPositionId: posCfo.Id, headcountCapacity: 1, isPrimaryLeadership: true);

        // People & Legal
        var posHrMgr = Position.Create(
            legalEntityId: legalEntity.Id, orgUnitId: ouDeptHr.Id, jobId: jobHrMgr.Id, jobGradeId: jgMgr.Id,
            code: "POS-HR-MGR-001", title: "ผู้จัดการฝ่ายบริหารทรัพยากรบุคคล", internationalTitle: "HR & People Operations Manager",
            costCenterId: ccSharedServices.Id, reportsToPositionId: posChro.Id, headcountCapacity: 1, isPrimaryLeadership: true);

        var posLegalMgr = Position.Create(
            legalEntityId: legalEntity.Id, orgUnitId: ouDeptLegal.Id, jobId: jobLegalMgr.Id, jobGradeId: jgMgr.Id,
            code: "POS-LEG-MGR-001", title: "ผู้จัดการฝ่ายกฎหมายและกำกับดูแลกิจการ", internationalTitle: "Legal & Compliance Manager",
            costCenterId: ccSharedServices.Id, reportsToPositionId: posChro.Id, headcountCapacity: 1, isPrimaryLeadership: true);

        await dbContext.Positions.AddRangeAsync(
            [
                posCeo, posCto, posCfo, posCoo, posChro,
                posVpEng, posDirProd, posSecMgr, posPrincipalArch, posErpLead, posErpSwePool, posCnxMgr, posCnxSrSwe,
                posVpSales, posSalesMgr, posPlantMgr, posRayongOpsPool,
                posAcctMgr, posSrAcct, posProcMgr,
                posHrMgr, posLegalMgr
            ],
            cancellationToken).ConfigureAwait(false);

        // =========================================================================
        // STEP 7: Assign ManagerPosition กลับเข้าทุก OrganizationUnit
        // =========================================================================
        ouCorp.AssignManagerPosition(posCeo);
        ouDivTech.AssignManagerPosition(posCto);
        ouDivComm.AssignManagerPosition(posVpSales);
        ouDivOps.AssignManagerPosition(posCoo);
        ouDivFin.AssignManagerPosition(posCfo);
        ouDivPeople.AssignManagerPosition(posChro);

        ouDeptEng.AssignManagerPosition(posVpEng);
        ouDeptProd.AssignManagerPosition(posDirProd);
        ouDeptSec.AssignManagerPosition(posSecMgr);
        ouDeptSales.AssignManagerPosition(posSalesMgr);
        ouDeptRayongOps.AssignManagerPosition(posPlantMgr);
        ouDeptAcct.AssignManagerPosition(posAcctMgr);
        ouDeptProc.AssignManagerPosition(posProcMgr);
        ouDeptHr.AssignManagerPosition(posHrMgr);
        ouDeptLegal.AssignManagerPosition(posLegalMgr);

        ouTeamCoreErp.AssignManagerPosition(posErpLead);
        ouTeamCnxRnd.AssignManagerPosition(posCnxMgr);
        // หมายเหตุ: ouTeamDevOps จงใจเว้น ManagerPositionId = null เพื่อทดสอบ Skip-Level Escalation ขึ้นไปหา posSecMgr (DEPT-SEC)

        // =========================================================================
        // STEP 8: PositionDelegations (จำลองสถานการณ์รักษาการและมอบอำนาจครบ 3 รูปแบบ)
        // =========================================================================
        var now = DateTimeOffset.UtcNow;

        // 1. ACTING_FULL: CTO มอบอำนาจเต็มรูปแบบให้ VP of Engineering รักษาการแทนช่วงเดินทางต่างประเทศ
        var delCtoActing = PositionDelegation.Create(
            legalEntityId: legalEntity.Id,
            delegatorPositionId: posCto.Id,
            delegatePositionId: posVpEng.Id,
            delegationType: DelegationType.ActingFull,
            validFrom: now.AddDays(-1),
            validTo: now.AddDays(7),
            reason: "CTO attending Global Cloud & AI Summit");

        // 2. APPROVAL_ONLY: COO มอบอำนาจอนุมัติเอกสารทั้งหมดให้ Rayong Operations Manager
        var delCooApproval = PositionDelegation.Create(
            legalEntityId: legalEntity.Id,
            delegatorPositionId: posCoo.Id,
            delegatePositionId: posPlantMgr.Id,
            delegationType: DelegationType.ApprovalOnly,
            validFrom: now,
            validTo: now.AddDays(5),
            reason: "COO on annual leave");

        // 3. SPECIFIC_DOCUMENTS: CFO มอบอำนาจอนุมัติเฉพาะ PR, PO, EXP ให้ Procurement Manager
        var delCfoSpecific = PositionDelegation.Create(
            legalEntityId: legalEntity.Id,
            delegatorPositionId: posCfo.Id,
            delegatePositionId: posProcMgr.Id,
            delegationType: DelegationType.SpecificDocuments,
            validFrom: now.AddDays(-2),
            validTo: now.AddDays(14),
            documentTypeScope: "PR, PO, EXP",
            reason: "Delegate routine procurement approvals during Q4 budget closing");

        await dbContext.PositionDelegations.AddRangeAsync(
            [delCtoActing, delCooApproval, delCfoSpecific],
            cancellationToken).ConfigureAwait(false);

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        logger.LogInformation(
            "Completed seeding Organization enterprise demo data: 1 LegalEntity, 3 TaxBranches, 8 JobGrades, 25 Jobs, 18 OUs, 7 CostCenters, 22 Positions, 3 Delegations.");
    }
}