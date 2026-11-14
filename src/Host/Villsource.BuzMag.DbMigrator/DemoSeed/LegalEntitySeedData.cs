using Villsource.FSH.Modules.Organization.Domain.LegalEntities;
using Villsource.ObjectValue;

namespace Villsource.BuzMag.DbMigrator.DemoSeed;

internal static class LegalEntitySeedData
{
    public static IReadOnlyList<LegalEntity> Build(
        OrganizationTemplateType templateType,
        string companyName) =>
        templateType switch
        {
            OrganizationTemplateType.TechCompany => BuildTechCompany(companyName),
            OrganizationTemplateType.Restaurant => BuildRestaurantChain(companyName),
            OrganizationTemplateType.CarDealership => BuildCarDealership(companyName),
            _ => throw new ArgumentOutOfRangeException(nameof(templateType), templateType, null),
        };

    // =========================================================================
    // 1. TECH COMPANY (e.g., Acme Corp — TH Head Entity + SG Regional Entity)
    // =========================================================================
    private static IReadOnlyList<LegalEntity> BuildTechCompany(string companyName)
    {
        var thEntity = LegalEntity.Create(
            code: "ACME-TH",
            countryCode: "TH",
            legalName: $"บริษัท {companyName} (ประเทศไทย) จำกัด",
            internationalName: $"{companyName} (Thailand) Co., Ltd.",
            taxId: "0105564012345",
            baseCurrency: "THB",
            registrationNumber: "0105564012345",
            profile: new CompanyProfile
            {
                TradeName = companyName,
                Website = "https://acme.example.com",
                ContactEmail = "legal-th@acme.com",
                ContactPhone = "+66-2-000-1000"
            },
            headOfficeAddress: CreateAddress(
                line1: "999/9 อาคารสำนักงานใหญ่ ชั้น 25 ถนนพระราม 9",
                subDistrict: "ห้วยขวาง",
                district: "ห้วยขวาง",
                province: "กรุงเทพมหานคร",
                postalCode: "10310",
                countryCode: "TH"),
            headOfficeBranchCode: "00000",
            headOfficeName: "สำนักงานใหญ่ (พระราม 9)",
            headOfficeInternationalName: "Head Office (Rama 9)",
            headOfficeRegisteredDate: new DateTimeOffset(2021, 3, 15, 0, 0, 0, TimeSpan.Zero));

        thEntity.AddBranch(
            branchCode: "00001",
            name: "สาขาเชียงใหม่ (ศูนย์พัฒนาซอฟต์แวร์)",
            internationalName: "Chiang Mai R&D Center",
            registeredAddress: CreateAddress(
                line1: "123 ถนนนิมมานเหมินท์",
                subDistrict: "สุเทพ",
                district: "เมืองเชียงใหม่",
                province: "เชียงใหม่",
                postalCode: "50200",
                countryCode: "TH"),
            registeredDate: new DateTimeOffset(2023, 6, 1, 0, 0, 0, TimeSpan.Zero));

        var sgEntity = LegalEntity.Create(
            code: "ACME-SG",
            countryCode: "SG",
            legalName: $"{companyName} APAC Pte. Ltd.",
            internationalName: $"{companyName} APAC Pte. Ltd.",
            taxId: "202212345K",
            baseCurrency: "SGD",
            registrationNumber: "202212345K",
            profile: new CompanyProfile
            {
                TradeName = $"{companyName} APAC",
                Website = "https://acme.example.com",
                ContactEmail = "apac@acme.com",
                ContactPhone = "+65-6000-2000"
            },
            headOfficeAddress: CreateAddress(
                line1: "1 Raffles Place, #20-01 One Raffles Place",
                subDistrict: "Downtown Core",
                district: "Central Region",
                province: "Singapore",
                postalCode: "048616",
                countryCode: "SG"),
            headOfficeBranchCode: "HQ",
            headOfficeName: "Singapore Regional HQ",
            headOfficeInternationalName: "Singapore Regional HQ",
            headOfficeRegisteredDate: new DateTimeOffset(2022, 8, 10, 0, 0, 0, TimeSpan.Zero));

        return [thEntity, sgEntity];
    }

    // =========================================================================
    // 2. RESTAURANT / F&B CHAIN (e.g., Savora Dining Group)
    // =========================================================================
    private static IReadOnlyList<LegalEntity> BuildRestaurantChain(string companyName)
    {
        var entity = LegalEntity.Create(
            code: "SAVORA-TH",
            countryCode: "TH",
            legalName: $"บริษัท {companyName} จำกัด",
            internationalName: $"{companyName} Co., Ltd.",
            taxId: "0105562098765",
            baseCurrency: "THB",
            registrationNumber: "0105562098765",
            profile: new CompanyProfile
            {
                TradeName = companyName,
                Website = "https://savora.example.com",
                ContactEmail = "contact@savora.com",
                ContactPhone = "+66-2-000-3000"
            },
            headOfficeAddress: CreateAddress(
                line1: "88 ซอยศูนย์วิจัย ถนนเพชรบุรีตัดใหม่",
                subDistrict: "บางกะปิ",
                district: "ห้วยขวาง",
                province: "กรุงเทพมหานคร",
                postalCode: "10310",
                countryCode: "TH"),
            headOfficeBranchCode: "00000",
            headOfficeName: "สำนักงานใหญ่และครัวกลาง",
            headOfficeInternationalName: "Head Office & Central Kitchen",
            headOfficeRegisteredDate: new DateTimeOffset(2019, 1, 20, 0, 0, 0, TimeSpan.Zero));

        entity.AddBranch(
            branchCode: "00001",
            name: "สาขาสยามสแควร์ (Flagship)",
            internationalName: "Siam Square Flagship Branch",
            registeredAddress: CreateAddress(
                line1: "388 ถนนพระรามที่ 1",
                subDistrict: "ปทุมวัน",
                district: "ปทุมวัน",
                province: "กรุงเทพมหานคร",
                postalCode: "10330",
                countryCode: "TH"),
            registeredDate: new DateTimeOffset(2019, 5, 1, 0, 0, 0, TimeSpan.Zero));

        entity.AddBranch(
            branchCode: "00002",
            name: "สาขาทองหล่อ",
            internationalName: "Thonglor Bistro & Bar",
            registeredAddress: CreateAddress(
                line1: "250 ซอยสุขุมวิท 55 (ทองหล่อ)",
                subDistrict: "คลองตันเหนือ",
                district: "วัฒนา",
                province: "กรุงเทพมหานคร",
                postalCode: "10110",
                countryCode: "TH"),
            registeredDate: new DateTimeOffset(2020, 11, 15, 0, 0, 0, TimeSpan.Zero));

        entity.AddBranch(
            branchCode: "00003",
            name: "สาขาบางแค",
            internationalName: "Bang Khae Mall Branch",
            registeredAddress: CreateAddress(
                line1: "275 ถนนเพชรเกษม",
                subDistrict: "บางแคเหนือ",
                district: "บางแค",
                province: "กรุงเทพมหานคร",
                postalCode: "10160",
                countryCode: "TH"),
            registeredDate: new DateTimeOffset(2022, 4, 10, 0, 0, 0, TimeSpan.Zero));

        return [entity];
    }

    // =========================================================================
    // 3. CAR DEALERSHIP & SERVICE CENTER (e.g., Globex)
    // =========================================================================
    private static IReadOnlyList<LegalEntity> BuildCarDealership(string companyName)
    {
        var entity = LegalEntity.Create(
            code: "GLOBEX-TH",
            countryCode: "TH",
            legalName: $"บริษัท {companyName} ออโตโมบิล จำกัด",
            internationalName: $"{companyName} Automobile Co., Ltd.",
            taxId: "0105558054321",
            baseCurrency: "THB",
            registrationNumber: "0105558054321",
            profile: new CompanyProfile
            {
                TradeName = $"{companyName} Motors",
                Website = "https://globex.example.com",
                ContactEmail = "info@globex.com",
                ContactPhone = "+66-2-000-4000"
            },
            headOfficeAddress: CreateAddress(
                line1: "555 ถนนวิภาวดีรังสิต",
                subDistrict: "จตุจักร",
                district: "จตุจักร",
                province: "กรุงเทพมหานคร",
                postalCode: "10900",
                countryCode: "TH"),
            headOfficeBranchCode: "00000",
            headOfficeName: "สำนักงานใหญ่กลุ่มธุรกิจยานยนต์",
            headOfficeInternationalName: "Dealer Group Headquarters",
            headOfficeRegisteredDate: new DateTimeOffset(2015, 7, 1, 0, 0, 0, TimeSpan.Zero));

        entity.AddBranch(
            branchCode: "00001",
            name: "โชว์รูมและศูนย์บริการ สาขาพระราม 9",
            internationalName: "Rama 9 Flagship Showroom & Service",
            registeredAddress: CreateAddress(
                line1: "789 ถนนพระราม 9",
                subDistrict: "บางกะปิ",
                district: "ห้วยขวาง",
                province: "กรุงเทพมหานคร",
                postalCode: "10310",
                countryCode: "TH"),
            registeredDate: new DateTimeOffset(2017, 2, 14, 0, 0, 0, TimeSpan.Zero));

        entity.AddBranch(
            branchCode: "00002",
            name: "โชว์รูมและศูนย์บริการ สาขาบางนา",
            internationalName: "Bangna Showroom & Service",
            registeredAddress: CreateAddress(
                line1: "456 ถนนเทพรัตน (บางนา-ตราด กม.4)",
                subDistrict: "บางนาใต้",
                district: "บางนา",
                province: "กรุงเทพมหานคร",
                postalCode: "10260",
                countryCode: "TH"),
            registeredDate: new DateTimeOffset(2020, 9, 1, 0, 0, 0, TimeSpan.Zero));

        return [entity];
    }

    // Adjust property/constructor mapping if Villsource.ObjectValue.Address uses different member names
    private static Address CreateAddress(
        string line1,
        string subDistrict,
        string district,
        string province,
        string postalCode,
        string countryCode) =>
        new()
        {
            SubDistrict = subDistrict,
            District = district,
            Province = province,
            PostalCode = postalCode,
            Building = line1,
            Country = countryCode,
        };
}