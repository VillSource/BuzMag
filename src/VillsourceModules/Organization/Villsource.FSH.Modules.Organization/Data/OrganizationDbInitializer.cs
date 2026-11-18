using FSH.Framework.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Villsource.FSH.Modules.Organization.Contracts.Constants;
using Villsource.FSH.Modules.Organization.Domain.JobFamilies;
using Villsource.FSH.Modules.Organization.Domain.JobGrades;
using Villsource.FSH.Modules.Organization.Domain.OrganizationUnitTypes;

namespace Villsource.FSH.Modules.Organization.Data;

public sealed partial class OrganizationDbInitializer(
    OrganizationDbContext dbContext,
    ILogger<OrganizationDbInitializer> logger) : IDbInitializer
{
    public async Task MigrateAsync(CancellationToken cancellationToken)
    {
        if ((await dbContext.Database.GetPendingMigrationsAsync(cancellationToken).ConfigureAwait(false)).Any())
        {
            await dbContext.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);
            logger.LogInformation("[Organization] applied migrations");
        }
    }

    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("[Organization] seeded default organization");

        if (!await dbContext.JobFamilies.AnyAsync(cancellationToken).ConfigureAwait(false))
        {
            var jf = CreateJobFamilies();
            await dbContext.JobFamilies.AddRangeAsync(jf.Values, cancellationToken).ConfigureAwait(false);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            LogSeedJfSeccess(jf.Count);
        }
        
        if (!await dbContext.JobGrades.AnyAsync(cancellationToken).ConfigureAwait(false))
        {
            var jg = CreateJobGrades();
            await dbContext.JobGrades.AddRangeAsync(jg.Values, cancellationToken).ConfigureAwait(false);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            LogSeedJgSeccess(jg.Count);
        }
        
        if (!await dbContext.OrganizationUnitTypes.AnyAsync(cancellationToken).ConfigureAwait(false))
        {
            var ouTypes = CreateOrganizationUnitTypes();
            await dbContext.OrganizationUnitTypes.AddRangeAsync(ouTypes.Values, cancellationToken).ConfigureAwait(false);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            LogSeedOuTypeSeccess(ouTypes.Count);
        }
    }
    
    private static Dictionary<string, JobFamily> CreateJobFamilies() => new(StringComparer.Ordinal)
    {
        // Executive & Strategy
        ["EXEC"] = JobFamily.Create("EXEC", "ผู้บริหารและจัดการทั่วไป", "Executive & General Management", "C-suite leadership, managing directors, general managers, and business unit heads."),
        ["STRAT"] = JobFamily.Create("STRAT", "กลยุทธ์องค์กร", "Corporate Strategy", "Corporate strategy, mergers and acquisitions, strategic planning, and enterprise PMO."),

        // Technology, Engineering & Data
        ["ENG"] = JobFamily.Create("ENG", "วิศวกรรมซอฟต์แวร์", "Software & Platform Engineering", "Backend, frontend, mobile, full-stack development, and core platform architecture."),
        ["INFRA"] = JobFamily.Create("INFRA", "วิศวกรรมคลาวด์และระบบ", "Cloud & Infrastructure", "Cloud engineering, site reliability, CI/CD automation, and production platform operations."),
        ["AI"] = JobFamily.Create("AI", "ปัญญาประดิษฐ์", "AI & Machine Learning", "Foundation models, applied machine learning, NLP, computer vision, and AI research."),
        ["DATA"] = JobFamily.Create("DATA", "ข้อมูลและการวิเคราะห์", "Data & Analytics", "Data pipelines, data warehousing, business intelligence, and decision science."),
        ["QA"] = JobFamily.Create("QA", "ประกันคุณภาพซอฟต์แวร์", "Quality Assurance", "Test automation, quality engineering, performance testing, and release reliability."),
        ["ITOPS"] = JobFamily.Create("ITOPS", "เทคโนโลยีสารสนเทศ", "Enterprise IT", "Internal IT infrastructure, corporate networks, endpoint support, and enterprise SaaS administration."),
        ["SEC"] = JobFamily.Create("SEC", "ความปลอดภัยไซเบอร์", "Cybersecurity & Trust", "Application security, cloud security, security operations, governance, risk, and data privacy."),

        // Product & Design
        ["PROD"] = JobFamily.Create("PROD", "บริหารผลิตภัณฑ์", "Product Management", "Product strategy, roadmaps, lifecycle management, and technical product ownership."),
        ["DES"] = JobFamily.Create("DES", "ออกแบบผลิตภัณฑ์และ UX", "Product Design & UX", "User experience, user interface design, design systems, and user research."),
        ["BRAND"] = JobFamily.Create("BRAND", "แบรนด์และงานสร้างสรรค์", "Brand & Creative", "Visual identity, graphic design, multimedia production, and brand storytelling."),

        // Commercial, Sales & Revenue
        ["SALES"] = JobFamily.Create("SALES", "การขาย", "Commercial Sales", "Account executives, inside sales, regional sales, and direct revenue generation."),
        ["BDEV"] = JobFamily.Create("BDEV", "พัฒนาธุรกิจและพันธมิตร", "Business Development", "Strategic alliances, channel partnerships, reseller ecosystems, and new market expansion."),
        ["SOL"] = JobFamily.Create("SOL", "โซลูชันและพรีเซลส์", "Solutions Architecture", "Technical pre-sales, solutions engineering, proof-of-concept delivery, and RFP architecture."),
        ["REVOPS"] = JobFamily.Create("REVOPS", "ปฏิบัติการฝ่ายขาย", "Revenue Operations", "Sales operations, CRM governance, pipeline forecasting, and commercial enablement."),

        // Marketing & Communications
        ["MKT"] = JobFamily.Create("MKT", "การตลาด", "Growth Marketing", "Demand generation, digital acquisition, SEO/SEM, lifecycle campaigns, and marketing analytics."),
        ["PMKT"] = JobFamily.Create("PMKT", "การตลาดผลิตภัณฑ์", "Product Marketing", "Product positioning, go-to-market launches, competitive intelligence, and industry events."),
        ["COMMS"] = JobFamily.Create("COMMS", "สื่อสารองค์กร", "Corporate Communications", "Public relations, media outreach, investor relations, and internal corporate communications."),

        // Customer Experience & Delivery
        ["CS"] = JobFamily.Create("CS", "บริหารความสำเร็จลูกค้า", "Customer Success", "Customer onboarding, adoption, retention, renewals, and executive account advocacy."),
        ["SUPP"] = JobFamily.Create("SUPP", "บริการและซัพพอร์ตลูกค้า", "Customer Support", "Tiered technical support, customer service operations, incident triage, and helpdesk."),
        ["IMPL"] = JobFamily.Create("IMPL", "ติดตั้งระบบและที่ปรึกษา", "Professional Services", "Enterprise solution deployment, custom integration, technical consulting, and client training."),

        // Corporate, People, Finance & Legal
        ["HR"] = JobFamily.Create("HR", "ทรัพยากรบุคคล", "Human Resources", "Talent acquisition, HR business partnering, total rewards, learning and development, and people operations."),
        ["FIN"] = JobFamily.Create("FIN", "การเงินและบัญชี", "Finance & Accounting", "Financial planning and analysis, accounting, controllership, tax, audit, and treasury."),
        ["LEGAL"] = JobFamily.Create("LEGAL", "กฎหมายและกำกับดูแล", "Legal & Compliance", "Corporate counsel, commercial contracts, regulatory compliance, intellectual property, and corporate secretary."),
        ["PROC"] = JobFamily.Create("PROC", "จัดซื้อและบริหารคู่ค้า", "Procurement", "Strategic sourcing, purchasing operations, supplier negotiations, and third-party vendor governance."),
        ["FAC"] = JobFamily.Create("FAC", "บริหารอาคารและสถานที่", "Facilities & Workplace", "Corporate real estate, office administration, physical security, and workplace health and safety."),

        // Supply Chain, Manufacturing & Physical Operations
        ["SCM"] = JobFamily.Create("SCM", "ซัพพลายเชน", "Supply Chain", "Sales and operations planning, inventory optimization, demand forecasting, and supply network design."),
        ["LOG"] = JobFamily.Create("LOG", "โลจิสติกส์และคลังสินค้า", "Logistics & Warehousing", "Warehouse operations, fleet and freight management, order fulfillment, and distribution."),
        ["MFG"] = JobFamily.Create("MFG", "การผลิต", "Manufacturing", "Plant operations, production control, assembly lines, maintenance, and lean manufacturing."),
        ["ENGHW"] = JobFamily.Create("ENGHW", "วิศวกรรมฮาร์ดแวร์และเครื่องกล", "Hardware & Mechanical Engineering", "Hardware R&D, electrical and mechanical engineering, industrial design, and prototyping."),
        ["QM"] = JobFamily.Create("QM", "บริหารคุณภาพและมาตรฐาน", "Quality Management", "Quality management systems (ISO/GMP), process audits, and product regulatory certifications."),

        // Research & Specialized Domains
        ["RND"] = JobFamily.Create("RND", "วิจัยและพัฒนา", "Research & Development", "Applied scientific research, laboratory operations, materials science, and long-horizon innovation."),
        ["CLIN"] = JobFamily.Create("CLIN", "การแพทย์และสาธารณสุข", "Clinical & Healthcare", "Medical affairs, clinical specialists, healthcare service delivery, and clinical compliance.")
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
    
    private static Dictionary<string, OrganizationUnitType> CreateOrganizationUnitTypes() => new(StringComparer.Ordinal)
    {
        ["GROUP"] = OrganizationUnitType.Create("GROUP", "Corporate Group", "Corporate Group", 0, "Top-level holding group."),
        ["SUBSIDIARY"] = OrganizationUnitType.Create("SUBSIDIARY", "Operating Subsidiary", "Operating Subsidiary", 1, "Regional or legal operating subsidiary."),
        ["DIVISION"] = OrganizationUnitType.Create("DIVISION", "Business Division", "Business Division", 2, "Core technology or corporate business pillar."),
        ["DEPARTMENT"] = OrganizationUnitType.Create("DEPARTMENT", "Department", "Department", 3, "Functional engineering, product, or corporate department."),
        ["TEAM"] = OrganizationUnitType.Create("TEAM", "Team", "Team", 4, "Specialized functional or platform team."),
        ["SQUAD"] = OrganizationUnitType.Create("SQUAD", "Delivery Squad", "Delivery Squad", 5, "Cross-functional agile delivery squad.")
    };
    
    
    
    [LoggerMessage(LogLevel.Information, "[Organization] Seeded {JobFamiliesCount} JobFamilies successfully")]
    partial void LogSeedJfSeccess(int jobFamiliesCount);
    
    [LoggerMessage(LogLevel.Information, "[Organization] Seeded {JobGradesCount} JobGrades successfully")]
    partial void LogSeedJgSeccess(int jobGradesCount);
    
    [LoggerMessage(LogLevel.Information, "[Organization] Seeded {OrganizationTypeCount} OrganizationTypes successfully")]
    partial void LogSeedOuTypeSeccess(int organizationTypeCount);
}
