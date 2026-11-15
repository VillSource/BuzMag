using Villsource.FSH.Modules.Organization.Contracts.Constants;
using Villsource.FSH.Modules.Organization.Domain.CostCenters;
using Villsource.FSH.Modules.Organization.Domain.LegalEntities;

namespace Villsource.BuzMag.DbMigrator.DemoSeed;

internal static class CostCenterSeedData
{
    public static IReadOnlyList<CostCenter> Build(
        OrganizationTemplateType templateType,
        IReadOnlyList<LegalEntity> legalEntities) =>
        templateType switch
        {
            OrganizationTemplateType.TechCompany => BuildTechCompany(legalEntities),
            OrganizationTemplateType.Restaurant => BuildRestaurantChain(legalEntities),
            OrganizationTemplateType.CarDealership => BuildCarDealership(legalEntities),
            _ => throw new ArgumentOutOfRangeException(nameof(templateType), templateType, null),
        };

    // =========================================================================
    // 1. TECH COMPANY (ACME-TH + ACME-SG)
    // =========================================================================
    private static List<CostCenter> BuildTechCompany(IReadOnlyList<LegalEntity> legalEntities)
    {
        var results = new List<CostCenter>();

        var thEntity = FindLegalEntity(legalEntities, "ACME-TH");
        if (thEntity is not null)
        {
            var hoBranchId = FindBranchId(thEntity, "00000");
            var cmBranchId = FindBranchId(thEntity, "00001");
            var thStart = new DateTimeOffset(2021, 3, 15, 0, 0, 0, TimeSpan.Zero);

            // Root: Shared Services (Head Office)
            var corporateRoot = CostCenter.Create(
                legalEntityId: thEntity.Id,
                code: "CC-1000",
                name: "ส่วนกลางและบริหารองค์กร",
                internationalName: "Corporate & Shared Services",
                functionType: CostCenterFunctionType.SharedServices,
                description: "ศูนย์ต้นทุนกลางสำหรับโครงสร้างพื้นฐานและการบริหารจัดการองค์กร",
                defaultTaxBranchId: hoBranchId,
                validFrom: thStart);

            var adminChild = CostCenter.Create(
                legalEntityId: thEntity.Id,
                code: "CC-1100",
                name: "ทรัพยากรบุคคล กฎหมาย และการเงิน",
                internationalName: "People, Legal & Finance",
                functionType: CostCenterFunctionType.Administrative,
                description: "ค่าใช้จ่ายบริหารงานบุคคล บัญชี การเงิน และกฎหมาย",
                parentCostCenterId: corporateRoot.Id,
                defaultTaxBranchId: hoBranchId,
                validFrom: thStart);

            var cloudInfraChild = CostCenter.Create(
                legalEntityId: thEntity.Id,
                code: "CC-1200",
                name: "โครงสร้างพื้นฐานคลาวด์และปฏิบัติการไอที",
                internationalName: "Cloud Infrastructure & IT Ops",
                functionType: CostCenterFunctionType.SharedServices,
                description: "ค่าใช้จ่าย Cloud Server, SaaS License และระบบความปลอดภัยภายใน",
                parentCostCenterId: corporateRoot.Id,
                defaultTaxBranchId: hoBranchId,
                validFrom: thStart);

            // Root: R&D (Chiang Mai Branch)
            var rndRoot = CostCenter.Create(
                legalEntityId: thEntity.Id,
                code: "CC-2000",
                name: "วิศวกรรมผลิตภัณฑ์และวิจัยพัฒนา",
                internationalName: "Product Engineering & R&D",
                functionType: CostCenterFunctionType.ResearchAndDevelopment,
                description: "ศูนย์วิจัยและพัฒนาซอฟต์แวร์หลัก สาขาเชียงใหม่",
                defaultTaxBranchId: cmBranchId ?? hoBranchId,
                validFrom: new DateTimeOffset(2023, 6, 1, 0, 0, 0, TimeSpan.Zero));

            var aiLabsChild = CostCenter.Create(
                legalEntityId: thEntity.Id,
                code: "CC-2100",
                name: "พัฒนาแพลตฟอร์มหลักและห้องปฏิบัติการ AI",
                internationalName: "Core Platform & AI Labs",
                functionType: CostCenterFunctionType.ResearchAndDevelopment,
                description: "ทีมวิจัยโมเดล AI และสถาปัตยกรรมแพลตฟอร์ม",
                parentCostCenterId: rndRoot.Id,
                defaultTaxBranchId: cmBranchId ?? hoBranchId,
                validFrom: new DateTimeOffset(2023, 6, 1, 0, 0, 0, TimeSpan.Zero));

            // Direct Cost of Sales & Selling
            var delivery = CostCenter.Create(
                legalEntityId: thEntity.Id,
                code: "CC-3000",
                name: "ส่งมอบโครงการและบริการลูกค้าองค์กร",
                internationalName: "Enterprise Delivery & Customer Success",
                functionType: CostCenterFunctionType.CostOfSales,
                description: "ต้นทุนการ Implement ระบบและดูแลหลังการขายสำหรับลูกค้าองค์กร",
                defaultTaxBranchId: hoBranchId,
                validFrom: thStart);

            var salesMarketing = CostCenter.Create(
                legalEntityId: thEntity.Id,
                code: "CC-4000",
                name: "การขายและการตลาดองค์กร",
                internationalName: "B2B Growth & Marketing",
                functionType: CostCenterFunctionType.SellingAndDistribution,
                description: "งบประมาณการตลาด งานอีเวนต์ และทีมขาย B2B",
                defaultTaxBranchId: hoBranchId,
                validFrom: thStart);

            // Legacy / Inactive Cost Center (for testing filters & lifecycle states)
            var legacyOps = CostCenter.Create(
                legalEntityId: thEntity.Id,
                code: "CC-9900",
                name: "ศูนย์ข้อมูลเดิม (ปิดใช้งานแล้ว)",
                internationalName: "Legacy On-Premise Data Center",
                functionType: CostCenterFunctionType.Administrative,
                description: "หน่วยงานดูแลเซิร์ฟเวอร์เดิมก่อนย้ายขึ้น Cloud ทั้งหมด",
                defaultTaxBranchId: hoBranchId,
                validFrom: thStart,
                validTo: new DateTimeOffset(2024, 12, 31, 23, 59, 59, TimeSpan.Zero),
                isActive: false,
                isPostingBlocked: true);

            results.AddRange([
                corporateRoot,
                adminChild,
                cloudInfraChild,
                rndRoot,
                aiLabsChild,
                delivery,
                salesMarketing,
                legacyOps
            ]);
        }

        var sgEntity = FindLegalEntity(legalEntities, "ACME-SG");
        if (sgEntity is not null)
        {
            var sgHqBranchId = FindBranchId(sgEntity, "HQ");
            var sgStart = new DateTimeOffset(2022, 8, 10, 0, 0, 0, TimeSpan.Zero);

            results.AddRange([
                CostCenter.Create(
                    legalEntityId: sgEntity.Id,
                    code: "SG-1000",
                    name: "APAC Regional Management",
                    internationalName: "APAC Regional Management",
                    functionType: CostCenterFunctionType.Administrative,
                    description: "Singapore regional holding & executive administration",
                    defaultTaxBranchId: sgHqBranchId,
                    validFrom: sgStart),
                CostCenter.Create(
                    legalEntityId: sgEntity.Id,
                    code: "SG-2000",
                    name: "APAC Enterprise Sales",
                    internationalName: "APAC Enterprise Sales",
                    functionType: CostCenterFunctionType.SellingAndDistribution,
                    description: "Regional commercial sales and partner channels",
                    defaultTaxBranchId: sgHqBranchId,
                    validFrom: sgStart)
            ]);
        }

        return results;
    }

    // =========================================================================
    // 2. RESTAURANT / F&B CHAIN (SAVORA-TH)
    // =========================================================================
    private static IReadOnlyList<CostCenter> BuildRestaurantChain(IReadOnlyList<LegalEntity> legalEntities)
    {
        var entity = FindLegalEntity(legalEntities, "SAVORA-TH");
        if (entity is null) return [];

        var hoBranchId = FindBranchId(entity, "00000");
        var siamBranchId = FindBranchId(entity, "00001");
        var thonglorBranchId = FindBranchId(entity, "00002");
        var bangKhaeBranchId = FindBranchId(entity, "00003");
        var start = new DateTimeOffset(2019, 1, 20, 0, 0, 0, TimeSpan.Zero);

        var hqAdmin = CostCenter.Create(
            legalEntityId: entity.Id,
            code: "CC-000",
            name: "สำนักงานใหญ่และบริหารกลาง",
            internationalName: "Head Office Administration",
            functionType: CostCenterFunctionType.Administrative,
            description: "ฝ่ายบริหาร บัญชี ทรัพยากรบุคคล และพัฒนาเมนู",
            defaultTaxBranchId: hoBranchId,
            validFrom: start);

        var brandMarketing = CostCenter.Create(
            legalEntityId: entity.Id,
            code: "CC-010",
            name: "การตลาดแบรนด์และช่องทางเดลิเวอรี",
            internationalName: "Brand Marketing & Delivery Channels",
            functionType: CostCenterFunctionType.SellingAndDistribution,
            description: "แคมเปญโปรโมชัน สื่อโฆษณา และค่าธรรมเนียมแพลตฟอร์มเดลิเวอรี",
            parentCostCenterId: hqAdmin.Id,
            defaultTaxBranchId: hoBranchId,
            validFrom: start);

        var centralKitchen = CostCenter.Create(
            legalEntityId: entity.Id,
            code: "CC-100",
            name: "ครัวกลางและจัดซื้อวัตถุดิบ",
            internationalName: "Central Kitchen & Commissary",
            functionType: CostCenterFunctionType.CostOfSales,
            description: "ต้นทุนผลิตวัตถุดิบกึ่งสำเร็จรูปและจัดส่งให้สาขา",
            defaultTaxBranchId: hoBranchId,
            validFrom: start);

        var siamBranch = CostCenter.Create(
            legalEntityId: entity.Id,
            code: "CC-201",
            name: "ปฏิบัติการสาขาสยามสแควร์ (Flagship)",
            internationalName: "Siam Square Flagship Store Operations",
            functionType: CostCenterFunctionType.CostOfSales,
            description: "ต้นทุนดำเนินงานหน้าร้านและครัวสาขาสยามสแควร์",
            defaultTaxBranchId: siamBranchId ?? hoBranchId,
            validFrom: new DateTimeOffset(2019, 5, 1, 0, 0, 0, TimeSpan.Zero));

        var thonglorBranch = CostCenter.Create(
            legalEntityId: entity.Id,
            code: "CC-202",
            name: "ปฏิบัติการสาขาทองหล่อ",
            internationalName: "Thonglor Bistro & Bar Operations",
            functionType: CostCenterFunctionType.CostOfSales,
            description: "ต้นทุนดำเนินงานหน้าร้านและบาร์สาขาทองหล่อ",
            defaultTaxBranchId: thonglorBranchId ?? hoBranchId,
            validFrom: new DateTimeOffset(2020, 11, 15, 0, 0, 0, TimeSpan.Zero));

        var bangKhaeBranch = CostCenter.Create(
            legalEntityId: entity.Id,
            code: "CC-203",
            name: "ปฏิบัติการสาขาบางแค",
            internationalName: "Bang Khae Mall Store Operations",
            functionType: CostCenterFunctionType.CostOfSales,
            description: "ต้นทุนดำเนินงานหน้าร้านสาขาเดอะมอลล์บางแค",
            defaultTaxBranchId: bangKhaeBranchId ?? hoBranchId,
            validFrom: new DateTimeOffset(2022, 4, 10, 0, 0, 0, TimeSpan.Zero));

        return
        [
            hqAdmin,
            brandMarketing,
            centralKitchen,
            siamBranch,
            thonglorBranch,
            bangKhaeBranch
        ];
    }

    // =========================================================================
    // 3. CAR DEALERSHIP & SERVICE CENTER (GLOBEX-TH)
    // =========================================================================
    private static IReadOnlyList<CostCenter> BuildCarDealership(IReadOnlyList<LegalEntity> legalEntities)
    {
        var entity = FindLegalEntity(legalEntities, "GLOBEX-TH");
        if (entity is null) return [];

        var hoBranchId = FindBranchId(entity, "00000");
        var rama9BranchId = FindBranchId(entity, "00001");
        var bangnaBranchId = FindBranchId(entity, "00002");

        var hqAdmin = CostCenter.Create(
            legalEntityId: entity.Id,
            code: "CC-HQ",
            name: "บริหารจัดการกลุ่มธุรกิจยานยนต์",
            internationalName: "Dealer Group Headquarters",
            functionType: CostCenterFunctionType.Administrative,
            description: "สำนักงานใหญ่ฝ่ายบริหาร การเงิน สินเชื่อเช่าซื้อ และทะเบียน",
            defaultTaxBranchId: hoBranchId,
            validFrom: new DateTimeOffset(2015, 7, 1, 0, 0, 0, TimeSpan.Zero));

        // Rama 9 Branch Hierarchy
        var rama9Start = new DateTimeOffset(2017, 2, 14, 0, 0, 0, TimeSpan.Zero);
        var rama9Complex = CostCenter.Create(
            legalEntityId: entity.Id,
            code: "CC-RMA9",
            name: "ปฏิบัติการส่วนกลาง สาขาพระราม 9",
            internationalName: "Rama 9 Complex Shared Operations",
            functionType: CostCenterFunctionType.SharedServices,
            description: "ค่าสาธารณูปโภคและบริหารอาคารโชว์รูมสาขาพระราม 9",
            defaultTaxBranchId: rama9BranchId ?? hoBranchId,
            validFrom: rama9Start);

        var rama9Sales = CostCenter.Create(
            legalEntityId: entity.Id,
            code: "CC-RMA9-SLS",
            name: "ฝ่ายขายรถยนต์ สาขาพระราม 9",
            internationalName: "Rama 9 Vehicle Sales Department",
            functionType: CostCenterFunctionType.SellingAndDistribution,
            description: "ฝ่ายขายรถยนต์ใหม่และรถมือสองรับรองคุณภาพ สาขาพระราม 9",
            parentCostCenterId: rama9Complex.Id,
            defaultTaxBranchId: rama9BranchId ?? hoBranchId,
            validFrom: rama9Start);

        var rama9Service = CostCenter.Create(
            legalEntityId: entity.Id,
            code: "CC-RMA9-SVC",
            name: "ศูนย์บริการซ่อมบำรุงและอะไหล่ สาขาพระราม 9",
            internationalName: "Rama 9 After-Sales Service & Parts",
            functionType: CostCenterFunctionType.CostOfSales,
            description: "ศูนย์ซ่อมเช็กระยะ ซ่อมสีและตัวถัง และคลังอะไหล่ สาขาพระราม 9",
            parentCostCenterId: rama9Complex.Id,
            defaultTaxBranchId: rama9BranchId ?? hoBranchId,
            validFrom: rama9Start);

        // Bangna Branch Hierarchy
        var bangnaStart = new DateTimeOffset(2020, 9, 1, 0, 0, 0, TimeSpan.Zero);
        var bangnaComplex = CostCenter.Create(
            legalEntityId: entity.Id,
            code: "CC-BNA",
            name: "ปฏิบัติการส่วนกลาง สาขาบางนา",
            internationalName: "Bangna Complex Shared Operations",
            functionType: CostCenterFunctionType.SharedServices,
            description: "ค่าสาธารณูปโภคและบริหารอาคารโชว์รูมสาขาบางนา",
            defaultTaxBranchId: bangnaBranchId ?? hoBranchId,
            validFrom: bangnaStart);

        var bangnaSales = CostCenter.Create(
            legalEntityId: entity.Id,
            code: "CC-BNA-SLS",
            name: "ฝ่ายขายรถยนต์ สาขาบางนา",
            internationalName: "Bangna Vehicle Sales Department",
            functionType: CostCenterFunctionType.SellingAndDistribution,
            description: "ฝ่ายขายรถยนต์ใหม่และรถทดลองขับ สาขาบางนา",
            parentCostCenterId: bangnaComplex.Id,
            defaultTaxBranchId: bangnaBranchId ?? hoBranchId,
            validFrom: bangnaStart);

        var bangnaService = CostCenter.Create(
            legalEntityId: entity.Id,
            code: "CC-BNA-SVC",
            name: "ศูนย์บริการซ่อมบำรุงและอะไหล่ สาขาบางนา",
            internationalName: "Bangna After-Sales Service & Parts",
            functionType: CostCenterFunctionType.CostOfSales,
            description: "ศูนย์ซ่อมบำรุงและจำหน่ายอะไหล่ สาขาบางนา",
            parentCostCenterId: bangnaComplex.Id,
            defaultTaxBranchId: bangnaBranchId ?? hoBranchId,
            validFrom: bangnaStart);

        return
        [
            hqAdmin,
            rama9Complex,
            rama9Sales,
            rama9Service,
            bangnaComplex,
            bangnaSales,
            bangnaService
        ];
    }

    private static LegalEntity? FindLegalEntity(IReadOnlyList<LegalEntity> legalEntities, string code) =>
        legalEntities.FirstOrDefault(e => string.Equals(e.Code, code, StringComparison.OrdinalIgnoreCase));

    private static Guid? FindBranchId(LegalEntity legalEntity, string branchCode) =>
        legalEntity.Branches
            .FirstOrDefault(b => !b.IsDeleted && string.Equals(b.BranchCode, branchCode, StringComparison.OrdinalIgnoreCase))
            ?.Id;
}