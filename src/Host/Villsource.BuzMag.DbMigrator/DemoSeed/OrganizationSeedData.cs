using Villsource.FSH.Modules.Organization.Contracts.Constants;
using Villsource.FSH.Modules.Organization.Domain;

namespace Villsource.BuzMag.DbMigrator.DemoSeed;

internal enum OrganizationTemplateType
{
    TechCompany,
    Restaurant,
    CarDealership,
}

internal static class OrganizationSeedData
{
    private const string Actor = "DemoSeeder";

    public static (IReadOnlyCollection<Position> Positions, OrganizationUnit RootUnit) Build(
        OrganizationTemplateType templateType,
        string companyName) =>
        templateType switch
        {
            OrganizationTemplateType.TechCompany => BuildTechCompany(companyName),
            OrganizationTemplateType.Restaurant => BuildRestaurantChain(companyName),
            OrganizationTemplateType.CarDealership => BuildCarDealership(companyName),
            _ => throw new ArgumentOutOfRangeException(nameof(templateType), templateType, null),
        };

    private static Position CreatePos(string code, string name, string description, HashSet<PositionTier> tiers)
    {
        var position = Position.Create(code, name, description, Actor);
        position.SetTier(tiers);
        return position;
    }

    // =========================================================================
    // 1. TECH COMPANY TEMPLATE
    // =========================================================================
    private static (IReadOnlyCollection<Position>, OrganizationUnit) BuildTechCompany(string companyName)
    {
        var pos = new List<Position>
        {
            // Executives & Management
            CreatePos("CEO", "Chief Executive Officer", "ผู้บริหารสูงสุดขององค์กร", [PositionTier.Executive]),
            CreatePos("CTO", "Chief Technology Officer", "ผู้บริหารสูงสุดสายงานเทคโนโลยี", [PositionTier.Executive]),
            CreatePos("CPO", "Chief Product Officer", "ผู้บริหารสูงสุดสายงานผลิตภัณฑ์", [PositionTier.Executive]),
            CreatePos("CFO", "Chief Financial Officer", "ผู้บริหารสูงสุดสายงานการเงิน", [PositionTier.Executive]),
            CreatePos("VP-ENG", "VP of Engineering", "รองประธานฝ่ายวิศวกรรมซอฟต์แวร์", [PositionTier.Director, PositionTier.Executive]),
            CreatePos("ENG-MGR", "Engineering Manager", "ผู้จัดการทีมวิศวกรรมซอฟต์แวร์", [PositionTier.Manager, PositionTier.Director]),
            CreatePos("PROD-MGR", "Group Product Manager", "ผู้จัดการกลุ่มผลิตภัณฑ์", [PositionTier.Manager, PositionTier.Director]),
            CreatePos("DES-MGR", "Design Manager", "ผู้จัดการทีมออกแบบประสบการณ์ผู้ใช้", [PositionTier.Manager]),
            CreatePos("GTM-DIR", "Commercial Director", "ผู้อำนวยการฝ่ายธุรกิจและการตลาด", [PositionTier.Director]),
            CreatePos("HR-MGR", "People & Culture Manager", "ผู้จัดการฝ่ายทรัพยากรบุคคล", [PositionTier.Manager]),

            // Engineering & Data
            CreatePos("SWE-BE", "Backend Software Engineer", "วิศวกรซอฟต์แวร์ฝั่งระบบหลังบ้านและ API",
                [PositionTier.Trainee, PositionTier.Junior, PositionTier.MidTier, PositionTier.Senior, PositionTier.Staff, PositionTier.Principal, PositionTier.Distinguished]),
            CreatePos("SWE-FE", "Frontend Software Engineer", "วิศวกรซอฟต์แวร์ฝั่งเว็บแอปพลิเคชัน",
                [PositionTier.Trainee, PositionTier.Junior, PositionTier.MidTier, PositionTier.Senior, PositionTier.Staff, PositionTier.Principal]),
            CreatePos("SWE-MOB", "Mobile Software Engineer", "วิศวกรพัฒนาแอปพลิเคชันมือถือ (iOS/Android)",
                [PositionTier.Junior, PositionTier.MidTier, PositionTier.Senior, PositionTier.Lead]),
            CreatePos("DEVOPS", "DevOps / SRE Engineer", "วิศวกรโครงสร้างพื้นฐานคลาวด์และความเสถียรระบบ",
                [PositionTier.MidTier, PositionTier.Senior, PositionTier.Staff, PositionTier.Lead]),
            CreatePos("QA-ENG", "QA & Automation Engineer", "วิศวกรทดสอบและประกันคุณภาพซอฟต์แวร์",
                [PositionTier.Trainee, PositionTier.Junior, PositionTier.MidTier, PositionTier.Senior, PositionTier.Lead]),
            CreatePos("SEC-ENG", "Application Security Engineer", "วิศวกรความปลอดภัยสารสนเทศและซอฟต์แวร์",
                [PositionTier.MidTier, PositionTier.Senior, PositionTier.Principal]),
            CreatePos("DATA-ENG", "Data Engineer", "วิศวกรระบบท่อส่งข้อมูลและคลังข้อมูล",
                [PositionTier.Junior, PositionTier.MidTier, PositionTier.Senior, PositionTier.Lead]),
            CreatePos("AI-ENG", "AI / Machine Learning Engineer", "วิศวกรปัญญาประดิษฐ์และโมเดลการเรียนรู้ของเครื่อง",
                [PositionTier.MidTier, PositionTier.Senior, PositionTier.Staff, PositionTier.Principal]),

            // Product, GTM & Corporate
            CreatePos("PM", "Product Manager", "ผู้ดูแลทิศทางและฟีเจอร์ของผลิตภัณฑ์",
                [PositionTier.Junior, PositionTier.MidTier, PositionTier.Senior, PositionTier.Lead, PositionTier.Principal]),
            CreatePos("UX-UI", "Product Designer (UX/UI)", "นักออกแบบประสบการณ์และส่วนติดต่อผู้ใช้",
                [PositionTier.Trainee, PositionTier.Junior, PositionTier.MidTier, PositionTier.Senior, PositionTier.Lead]),
            CreatePos("AE", "Account Executive", "ผู้ดูแลการขายโซลูชันและลูกค้าองค์กร",
                [PositionTier.Junior, PositionTier.MidTier, PositionTier.Senior, PositionTier.Lead]),
            CreatePos("MKT-SPEC", "Growth & Digital Marketing Specialist", "ผู้เชี่ยวชาญด้านการตลาดดิจิทัลและการเติบโต",
                [PositionTier.Junior, PositionTier.MidTier, PositionTier.Senior]),
            CreatePos("CS-SPEC", "Customer Success Specialist", "ผู้เชี่ยวชาญด้านการดูแลและสนับสนุนลูกค้า",
                [PositionTier.Junior, PositionTier.MidTier, PositionTier.Senior, PositionTier.Supervisor]),
            CreatePos("HR-BP", "People Partner & Talent Acquisition", "สรรหาบุคลากรและดูแลพนักงาน",
                [PositionTier.Junior, PositionTier.MidTier, PositionTier.Senior, PositionTier.Lead]),
            CreatePos("FIN-ACC", "Accountant & Financial Analyst", "นักบัญชีและวิเคราะห์การเงินองค์กร",
                [PositionTier.Junior, PositionTier.MidTier, PositionTier.Senior]),
            CreatePos("IT-SUP", "Corporate IT & Security Specialist", "ดูแลระบบไอทีและอุปกรณ์ภายในองค์กร",
                [PositionTier.Junior, PositionTier.MidTier, PositionTier.Senior]),
        }.ToDictionary(p => p.Code, StringComparer.OrdinalIgnoreCase);

        var root = OrganizationUnit.Create("COMP", companyName, "สำนักงานใหญ่และโครงสร้างองค์กรหลัก", Actor);

        var exec = root.CreateChild("EXEC", "Executive Leadership", "คณะผู้บริหารระดับสูง", Actor);
        exec.AllocatePosition(pos["CEO"], 1, Actor);
        exec.AllocatePosition(pos["CTO"], 1, Actor);
        exec.AllocatePosition(pos["CPO"], 1, Actor);
        exec.AllocatePosition(pos["CFO"], 1, Actor);

        var eng = root.CreateChild("ENG", "Engineering & Technology", "สายงานวิศวกรรมซอฟต์แวร์และโครงสร้างพื้นฐาน", Actor);
        eng.AllocatePosition(pos["VP-ENG"], 1, Actor);

        var engCore = eng.CreateChild("ENG-CORE", "Core Product Engineering", "ฝ่ายพัฒนาผลิตภัณฑ์หลัก", Actor);
        engCore.AllocatePosition(pos["ENG-MGR"], 2, Actor);
        engCore.AllocatePosition(pos["SWE-BE"], null, Actor); // Internship / Trainee pool
        engCore.AllocatePosition(pos["SWE-FE"], null, Actor);

        var engBe = engCore.CreateChild("ENG-BE", "Backend Engineering", "ทีมพัฒนา API, Microservices และฐานข้อมูล", Actor);
        engBe.AllocatePosition(pos["SWE-BE"], 12, Actor);

        var engFe = engCore.CreateChild("ENG-FE", "Frontend & Web Engineering", "ทีมพัฒนา Web Portal และ Dashboard", Actor);
        engFe.AllocatePosition(pos["SWE-FE"], 8, Actor);

        var engMob = engCore.CreateChild("ENG-MOB", "Mobile Engineering", "ทีมพัฒนาแอปพลิเคชัน iOS และ Android", Actor);
        engMob.AllocatePosition(pos["SWE-MOB"], 5, Actor);

        var engPlat = eng.CreateChild("ENG-PLAT", "Platform & Infrastructure", "ฝ่ายคลาวด์ DevOps และ SRE", Actor);
        engPlat.AllocatePosition(pos["ENG-MGR"], 1, Actor);
        engPlat.AllocatePosition(pos["DEVOPS"], 5, Actor);

        var engData = eng.CreateChild("ENG-DATA", "Data & AI Engineering", "ฝ่ายวิศวกรรมข้อมูลและปัญญาประดิษฐ์", Actor);
        engData.AllocatePosition(pos["ENG-MGR"], 1, Actor);
        engData.AllocatePosition(pos["DATA-ENG"], 4, Actor);
        engData.AllocatePosition(pos["AI-ENG"], 4, Actor);

        var engQaSec = eng.CreateChild("ENG-QASEC", "Quality Assurance & Security", "ฝ่ายประกันคุณภาพซอฟต์แวร์และความปลอดภัย", Actor);
        engQaSec.AllocatePosition(pos["QA-ENG"], 6, Actor);
        engQaSec.AllocatePosition(pos["SEC-ENG"], 2, Actor);

        var prod = root.CreateChild("PROD", "Product & Design", "สายงานบริหารผลิตภัณฑ์และการออกแบบ", Actor);
        var prodPm = prod.CreateChild("PROD-PM", "Product Management", "ฝ่ายวางกลยุทธ์และดูแลผลิตภัณฑ์", Actor);
        prodPm.AllocatePosition(pos["PROD-MGR"], 1, Actor);
        prodPm.AllocatePosition(pos["PM"], 6, Actor);

        var prodUx = prod.CreateChild("PROD-UX", "Product Design (UX/UI)", "ฝ่ายวิจัยผู้ใช้และออกแบบผลิตภัณฑ์", Actor);
        prodUx.AllocatePosition(pos["DES-MGR"], 1, Actor);
        prodUx.AllocatePosition(pos["UX-UI"], 5, Actor);

        var gtm = root.CreateChild("GTM", "Go-To-Market & Commercial", "สายงานการขาย การตลาด และดูแลลูกค้า", Actor);
        gtm.AllocatePosition(pos["GTM-DIR"], 1, Actor);

        var gtmSales = gtm.CreateChild("GTM-SALES", "Enterprise Sales & BD", "ฝ่ายขายลูกค้าองค์กรและพัฒนาธุรกิจ", Actor);
        gtmSales.AllocatePosition(pos["AE"], 6, Actor);

        var gtmMkt = gtm.CreateChild("GTM-MKT", "Marketing & Growth", "ฝ่ายการตลาดดิจิทัลและสร้างการเติบโต", Actor);
        gtmMkt.AllocatePosition(pos["MKT-SPEC"], 4, Actor);

        var gtmCs = gtm.CreateChild("GTM-CS", "Customer Success & Support", "ฝ่ายบริการหลังการขายและสนับสนุนลูกค้า", Actor);
        gtmCs.AllocatePosition(pos["CS-SPEC"], 6, Actor);

        var corp = root.CreateChild("CORP", "Corporate & Operations", "สายงานสนับสนุนและปฏิบัติการองค์กร", Actor);
        var corpHr = corp.CreateChild("CORP-HR", "People & Talent", "ฝ่ายทรัพยากรบุคคลและสรรหาบุคลากร", Actor);
        corpHr.AllocatePosition(pos["HR-MGR"], 1, Actor);
        corpHr.AllocatePosition(pos["HR-BP"], 3, Actor);

        var corpFin = corp.CreateChild("CORP-FIN", "Finance & Legal", "ฝ่ายบัญชี การเงิน และกฎหมาย", Actor);
        corpFin.AllocatePosition(pos["FIN-ACC"], 3, Actor);

        var corpIt = corp.CreateChild("CORP-IT", "Workplace & Internal IT", "ฝ่ายดูแลระบบไอทีภายในองค์กร", Actor);
        corpIt.AllocatePosition(pos["IT-SUP"], 2, Actor);

        return (pos.Values, root);
    }

    // =========================================================================
    // 2. RESTAURANT / F&B MULTI-BRANCH TEMPLATE
    // =========================================================================
    private static (IReadOnlyCollection<Position>, OrganizationUnit) BuildRestaurantChain(string companyName)
    {
        var pos = new List<Position>
        {
            // HQ & Management
            CreatePos("CEO", "Chief Executive Officer", "ประธานเจ้าหน้าที่บริหาร", [PositionTier.Executive]),
            CreatePos("COO", "Chief Operating Officer", "ประธานเจ้าหน้าที่ฝ่ายปฏิบัติการร้านอาหาร", [PositionTier.Executive]),
            CreatePos("OPS-DIR", "Director of Restaurant Operations", "ผู้อำนวยการฝ่ายปฏิบัติการสาขา", [PositionTier.Director]),
            CreatePos("AREA-MGR", "Area Manager", "ผู้จัดการเขตดูแลหลายสาขา", [PositionTier.Manager]),
            CreatePos("BR-MGR", "Restaurant General Manager", "ผู้จัดการสาขา", [PositionTier.Supervisor, PositionTier.Manager]),
            CreatePos("FOH-SUP", "Floor / Service Supervisor", "หัวหน้าพนักงานบริการหน้าร้าน", [PositionTier.Lead, PositionTier.Supervisor]),

            // Culinary & Central Kitchen
            CreatePos("EXEC-CHEF", "Executive Chef", "หัวหน้าเชฟใหญ่บริหารเมนูและมาตรฐานครัว", [PositionTier.Director, PositionTier.Executive]),
            CreatePos("RD-CHEF", "R&D Chef", "เชฟวิจัยและพัฒนาสูตรอาหาร", [PositionTier.Senior, PositionTier.Lead]),
            CreatePos("HEAD-CHEF", "Head Chef (Branch)", "หัวหน้าพ่อครัวประจำสาขา", [PositionTier.Supervisor, PositionTier.Manager]),
            CreatePos("SOUS-CHEF", "Sous Chef", "รองหัวหน้าพ่อครัว", [PositionTier.Senior, PositionTier.Lead]),
            CreatePos("LINE-COOK", "Line Cook / Station Chef", "กุ๊กประจำสเตชัน (ผัด/ย่าง/เตรียมวัตถุดิบ)",
                [PositionTier.Trainee, PositionTier.Junior, PositionTier.Staff, PositionTier.Senior]),
            CreatePos("QC-FOOD", "Food Safety & QA Specialist", "เจ้าหน้าที่ควบคุมคุณภาพและความปลอดภัยอาหาร",
                [PositionTier.Staff, PositionTier.Senior, PositionTier.Lead]),

            // Front of House & Bar
            CreatePos("HOST", "Host / Receptionist", "พนักงานต้อนรับและจัดคิวลูกค้า", [PositionTier.Junior, PositionTier.Staff]),
            CreatePos("SERVER", "Service Crew / Waitstaff", "พนักงานบริการหน้าร้าน",
                [PositionTier.Trainee, PositionTier.Junior, PositionTier.Staff, PositionTier.Senior]),
            CreatePos("BARISTA", "Barista & Beverage Specialist", "พนักงานชงเครื่องดื่มและกาแฟ",
                [PositionTier.Junior, PositionTier.Staff, PositionTier.Senior, PositionTier.Lead]),
            CreatePos("BARTENDER", "Bartender / Mixologist", "พนักงานผสมเครื่องดื่มและบาร์",
                [PositionTier.Staff, PositionTier.Senior, PositionTier.Lead]),
            CreatePos("CASHIER", "Cashier & POS Operator", "พนักงานแคชเชียร์และรับออเดอร์", [PositionTier.Junior, PositionTier.Staff]),
            CreatePos("STEWARD", "Kitchen Steward", "พนักงานล้างจานและดูแลความสะอาดครัว", [PositionTier.Junior, PositionTier.Staff]),

            // Supply Chain & Back Office
            CreatePos("PROCURE", "Purchasing & Supply Chain Officer", "เจ้าหน้าที่จัดซื้อวัตถุดิบและซัพพลายเชน",
                [PositionTier.Staff, PositionTier.Senior, PositionTier.Manager]),
            CreatePos("MKT-FB", "F&B Brand & Digital Marketing", "การตลาดแบรนด์ร้านอาหารและเดลิเวอรี",
                [PositionTier.Staff, PositionTier.Senior, PositionTier.Manager]),
            CreatePos("HR-SPEC", "HR & Shift Scheduling Officer", "ฝ่ายบุคคลและบริหารตารางกะพนักงาน",
                [PositionTier.Staff, PositionTier.Senior, PositionTier.Manager]),
            CreatePos("FIN-ACC", "Cost Controller & Accountant", "นักบัญชีและควบคุมต้นทุนอาหาร (Food Cost)",
                [PositionTier.Staff, PositionTier.Senior, PositionTier.Manager]),
        }.ToDictionary(p => p.Code, StringComparer.OrdinalIgnoreCase);

        var root = OrganizationUnit.Create("COMP", companyName, "กลุ่มธุรกิจร้านอาหารและเครื่องดื่ม", Actor);

        // Head Office
        var hq = root.CreateChild("HQ", "Head Office & Central Kitchen", "สำนักงานใหญ่และครัวกลาง", Actor);
        hq.AllocatePosition(pos["CEO"], 1, Actor);
        hq.AllocatePosition(pos["COO"], 1, Actor);

        var culinary = hq.CreateChild("HQ-CK", "Central Kitchen & Culinary R&D", "ครัวกลาง วิจัยเมนู และประกันคุณภาพอาหาร", Actor);
        culinary.AllocatePosition(pos["EXEC-CHEF"], 1, Actor);
        culinary.AllocatePosition(pos["RD-CHEF"], 2, Actor);
        culinary.AllocatePosition(pos["QC-FOOD"], 2, Actor);
        culinary.AllocatePosition(pos["LINE-COOK"], 8, Actor);

        var hqOps = hq.CreateChild("HQ-OPS", "Operations, Supply Chain & Support", "ฝ่ายปฏิบัติการสาขา จัดซื้อ การตลาด และบัญชี", Actor);
        hqOps.AllocatePosition(pos["OPS-DIR"], 1, Actor);
        hqOps.AllocatePosition(pos["AREA-MGR"], 2, Actor);
        hqOps.AllocatePosition(pos["PROCURE"], 3, Actor);
        hqOps.AllocatePosition(pos["MKT-FB"], 3, Actor);
        hqOps.AllocatePosition(pos["HR-SPEC"], 2, Actor);
        hqOps.AllocatePosition(pos["FIN-ACC"], 3, Actor);

        // Branch 1: Siam Square (Flagship)
        var brSiam = root.CreateChild("BR-SIAM", "Siam Square Flagship Branch", "สาขาสยามสแควร์ (Flagship)", Actor);
        brSiam.AllocatePosition(pos["BR-MGR"], 1, Actor);

        var siamFoh = brSiam.CreateChild("SIAM-FOH", "Front of House (Siam)", "ฝ่ายบริการหน้าร้าน สาขาสยามสแควร์", Actor);
        siamFoh.AllocatePosition(pos["FOH-SUP"], 2, Actor);
        siamFoh.AllocatePosition(pos["HOST"], 2, Actor);
        siamFoh.AllocatePosition(pos["SERVER"], 12, Actor);
        siamFoh.AllocatePosition(pos["SERVER"], null, Actor); // Part-time / Weekend shift pool
        siamFoh.AllocatePosition(pos["BARISTA"], 3, Actor);
        siamFoh.AllocatePosition(pos["CASHIER"], 2, Actor);

        var siamBoh = brSiam.CreateChild("SIAM-BOH", "Back of House / Kitchen (Siam)", "ฝ่ายครัว สาขาสยามสแควร์", Actor);
        siamBoh.AllocatePosition(pos["HEAD-CHEF"], 1, Actor);
        siamBoh.AllocatePosition(pos["SOUS-CHEF"], 2, Actor);
        siamBoh.AllocatePosition(pos["LINE-COOK"], 8, Actor);
        siamBoh.AllocatePosition(pos["STEWARD"], 3, Actor);

        // Branch 2: Thonglor (Dining & Bar)
        var brThonglor = root.CreateChild("BR-TL", "Thonglor Bistro & Bar", "สาขาทองหล่อ", Actor);
        brThonglor.AllocatePosition(pos["BR-MGR"], 1, Actor);

        var tlFoh = brThonglor.CreateChild("TL-FOH", "Front of House & Bar (Thonglor)", "ฝ่ายบริการหน้าร้านและบาร์ สาขาทองหล่อ", Actor);
        tlFoh.AllocatePosition(pos["FOH-SUP"], 2, Actor);
        tlFoh.AllocatePosition(pos["SERVER"], 10, Actor);
        tlFoh.AllocatePosition(pos["BARTENDER"], 3, Actor);
        tlFoh.AllocatePosition(pos["CASHIER"], 2, Actor);

        var tlBoh = brThonglor.CreateChild("TL-BOH", "Back of House / Kitchen (Thonglor)", "ฝ่ายครัว สาขาทองหล่อ", Actor);
        tlBoh.AllocatePosition(pos["HEAD-CHEF"], 1, Actor);
        tlBoh.AllocatePosition(pos["SOUS-CHEF"], 1, Actor);
        tlBoh.AllocatePosition(pos["LINE-COOK"], 7, Actor);
        tlBoh.AllocatePosition(pos["STEWARD"], 3, Actor);

        // Branch 3: Bang Khae (Mall Branch)
        var brBangKhae = root.CreateChild("BR-BK", "Bang Khae Mall Branch", "สาขาบางแค", Actor);
        brBangKhae.AllocatePosition(pos["BR-MGR"], 1, Actor);
        brBangKhae.AllocatePosition(pos["FOH-SUP"], 1, Actor);
        brBangKhae.AllocatePosition(pos["SERVER"], 8, Actor);
        brBangKhae.AllocatePosition(pos["BARISTA"], 2, Actor);
        brBangKhae.AllocatePosition(pos["CASHIER"], 2, Actor);
        brBangKhae.AllocatePosition(pos["HEAD-CHEF"], 1, Actor);
        brBangKhae.AllocatePosition(pos["LINE-COOK"], 5, Actor);
        brBangKhae.AllocatePosition(pos["STEWARD"], 2, Actor);

        return (pos.Values, root);
    }

    // =========================================================================
    // 3. CAR DEALERSHIP & SERVICE CENTER TEMPLATE
    // =========================================================================
    private static (IReadOnlyCollection<Position>, OrganizationUnit) BuildCarDealership(string companyName)
    {
        var pos = new List<Position>
        {
            // Executive & Branch Management
            CreatePos("MD", "Managing Director", "กรรมการผู้จัดการกลุ่มตัวแทนจำหน่าย", [PositionTier.Executive]),
            CreatePos("GM", "General Manager (Dealership)", "ผู้จัดการทั่วไปประจำสาขา", [PositionTier.Director, PositionTier.Executive]),
            CreatePos("CFO", "Chief Financial Officer", "ผู้อำนวยการฝ่ายบัญชีและการเงิน", [PositionTier.Executive]),

            // New & Used Car Sales + F&I
            CreatePos("SALES-MGR", "Sales Manager", "ผู้จัดการฝ่ายขายรถยนต์", [PositionTier.Manager]),
            CreatePos("SALES-SUP", "Sales Team Supervisor", "หัวหน้าทีมที่ปรึกษาการขาย", [PositionTier.Lead, PositionTier.Supervisor]),
            CreatePos("SALES-CONS", "Sales Consultant", "ที่ปรึกษาการขายรถยนต์",
                [PositionTier.Trainee, PositionTier.Junior, PositionTier.Staff, PositionTier.Senior]),
            CreatePos("PROD-GENIUS", "Product & EV Specialist", "ผู้เชี่ยวชาญผลิตภัณฑ์และระบบรถยนต์ไฟฟ้า",
                [PositionTier.Junior, PositionTier.Staff, PositionTier.Senior]),
            CreatePos("FI-MGR", "Finance & Insurance (F&I) Manager", "ผู้จัดการฝ่ายสินเชื่อและประกันภัยรถยนต์", [PositionTier.Manager]),
            CreatePos("FI-SPEC", "F&I & Registration Specialist", "เจ้าหน้าที่สินเชื่อ ประกันภัย และทะเบียนรถ",
                [PositionTier.Staff, PositionTier.Senior]),
            CreatePos("UCAR-MGR", "Certified Pre-Owned Manager", "ผู้จัดการฝ่ายรถยนต์มือสอง", [PositionTier.Manager]),
            CreatePos("APPRAISER", "Used Car Appraiser", "เจ้าหน้าที่ประเมินราคารถยนต์มือสอง",
                [PositionTier.Staff, PositionTier.Senior, PositionTier.Lead]),

            // After-Sales Service & Workshop
            CreatePos("SVC-MGR", "After-Sales Service Manager", "ผู้จัดการศูนย์บริการหลังการขาย", [PositionTier.Manager]),
            CreatePos("SA", "Service Advisor", "พนักงานรับรถและให้คำปรึกษาซ่อมบำรุง",
                [PositionTier.Junior, PositionTier.Staff, PositionTier.Senior, PositionTier.Lead]),
            CreatePos("FOREMAN", "Workshop Foreman", "หัวหน้าช่างควบคุมงานซ่อม", [PositionTier.Supervisor, PositionTier.Lead]),
            CreatePos("TECH-MECH", "Automotive Mechanic Technician", "ช่างเทคนิคซ่อมเครื่องยนต์และช่วงล่าง",
                [PositionTier.Trainee, PositionTier.Junior, PositionTier.Staff, PositionTier.Senior]),
            CreatePos("TECH-EV", "EV & High-Voltage Technician", "ช่างเทคนิคระบบไฟฟ้าแรงดันสูงและแบตเตอรี่ EV",
                [PositionTier.Staff, PositionTier.Senior, PositionTier.Lead]),
            CreatePos("QC-INSP", "Final Quality Inspector", "เจ้าหน้าที่ตรวจสอบคุณภาพงานซ่อมก่อนส่งมอบ",
                [PositionTier.Senior, PositionTier.Lead]),

            // Body & Paint + Spare Parts
            CreatePos("BP-SUP", "Body & Paint Supervisor", "หัวหน้าศูนย์ซ่อมตัวถังและสี", [PositionTier.Supervisor]),
            CreatePos("TECH-BP", "Body & Paint Technician", "ช่างเคาะพ่นสีตัวถังรถยนต์",
                [PositionTier.Junior, PositionTier.Staff, PositionTier.Senior]),
            CreatePos("PARTS-MGR", "Spare Parts Manager", "ผู้จัดการฝ่ายอะไหล่และคลังสินค้า", [PositionTier.Manager]),
            CreatePos("PARTS-SPEC", "Spare Parts & Accessories Officer", "เจ้าหน้าที่เบิกจ่ายอะไหล่และอุปกรณ์ตกแต่ง",
                [PositionTier.Junior, PositionTier.Staff, PositionTier.Senior]),

            // Corporate & CRM
            CreatePos("CRM-MGR", "CRM & Marketing Manager", "ผู้จัดการฝ่ายลูกค้าสัมพันธ์และการตลาด", [PositionTier.Manager]),
            CreatePos("CRM-SPEC", "Customer Relations & Tele-Service", "เจ้าหน้าที่ลูกค้าสัมพันธ์และนัดหมายเข้าศูนย์",
                [PositionTier.Junior, PositionTier.Staff, PositionTier.Senior]),
            CreatePos("HR-SPEC", "HR & Training Specialist", "ฝ่ายทรัพยากรบุคคลและฝึกอบรม",
                [PositionTier.Staff, PositionTier.Senior, PositionTier.Manager]),
            CreatePos("FIN-ACC", "Dealership Accountant & Cashier", "พนักงานบัญชีและการเงินโชว์รูม",
                [PositionTier.Staff, PositionTier.Senior]),
        }.ToDictionary(p => p.Code, StringComparer.OrdinalIgnoreCase);

        var root = OrganizationUnit.Create("COMP", companyName, "กลุ่มตัวแทนจำหน่ายรถยนต์และศูนย์บริการครบวงจร", Actor);

        // Head Office
        var hq = root.CreateChild("HQ", "Dealer Group Headquarters", "สำนักงานใหญ่กลุ่มธุรกิจยานยนต์", Actor);
        hq.AllocatePosition(pos["MD"], 1, Actor);
        hq.AllocatePosition(pos["CFO"], 1, Actor);

        var hqCorp = hq.CreateChild("HQ-CORP", "Corporate Finance, HR & CRM", "ฝ่ายบัญชีกลาง บุคคล และการตลาด", Actor);
        hqCorp.AllocatePosition(pos["CRM-MGR"], 1, Actor);
        hqCorp.AllocatePosition(pos["CRM-SPEC"], 4, Actor);
        hqCorp.AllocatePosition(pos["HR-SPEC"], 3, Actor);
        hqCorp.AllocatePosition(pos["FIN-ACC"], 4, Actor);

        // Branch 1: Rama 9 (Flagship Showroom & Body/Paint Center)
        var brRama9 = root.CreateChild("BR-RAMA9", "Rama 9 Flagship Showroom & Service", "โชว์รูมและศูนย์บริการ สาขาพระราม 9", Actor);
        brRama9.AllocatePosition(pos["GM"], 1, Actor);

        var r9Sales = brRama9.CreateChild("R9-SALES", "New Vehicle Sales (Rama 9)", "ฝ่ายขายรถยนต์ใหม่ สาขาพระราม 9", Actor);
        r9Sales.AllocatePosition(pos["SALES-MGR"], 1, Actor);
        r9Sales.AllocatePosition(pos["SALES-SUP"], 2, Actor);
        r9Sales.AllocatePosition(pos["SALES-CONS"], 12, Actor);
        r9Sales.AllocatePosition(pos["PROD-GENIUS"], 2, Actor);

        var r9Fi = brRama9.CreateChild("R9-FI", "Finance, Insurance & Registration (Rama 9)", "ฝ่ายสินเชื่อ ประกันภัย และทะเบียน", Actor);
        r9Fi.AllocatePosition(pos["FI-MGR"], 1, Actor);
        r9Fi.AllocatePosition(pos["FI-SPEC"], 3, Actor);

        var r9Svc = brRama9.CreateChild("R9-SVC", "After-Sales Service & EV Workshop (Rama 9)", "ศูนย์บริการซ่อมบำรุงและ EV", Actor);
        r9Svc.AllocatePosition(pos["SVC-MGR"], 1, Actor);
        r9Svc.AllocatePosition(pos["SA"], 5, Actor);
        r9Svc.AllocatePosition(pos["FOREMAN"], 2, Actor);
        r9Svc.AllocatePosition(pos["TECH-MECH"], 10, Actor);
        r9Svc.AllocatePosition(pos["TECH-EV"], 4, Actor);
        r9Svc.AllocatePosition(pos["QC-INSP"], 2, Actor);

        var r9Bp = brRama9.CreateChild("R9-BP", "Body & Paint Center (Rama 9)", "ศูนย์ซ่อมตัวถังและสีมาตรฐาน", Actor);
        r9Bp.AllocatePosition(pos["BP-SUP"], 1, Actor);
        r9Bp.AllocatePosition(pos["TECH-BP"], 8, Actor);

        var r9Parts = brRama9.CreateChild("R9-PARTS", "Spare Parts & Accessories (Rama 9)", "คลังอะไหล่และอุปกรณ์ตกแต่ง", Actor);
        r9Parts.AllocatePosition(pos["PARTS-MGR"], 1, Actor);
        r9Parts.AllocatePosition(pos["PARTS-SPEC"], 4, Actor);

        // Branch 2: Bangna (Showroom & Quick Service)
        var brBangna = root.CreateChild("BR-BN", "Bangna Showroom & Service", "โชว์รูมและศูนย์บริการ สาขาบางนา", Actor);
        brBangna.AllocatePosition(pos["GM"], 1, Actor);

        var bnSales = brBangna.CreateChild("BN-SALES", "New Vehicle Sales & F&I (Bangna)", "ฝ่ายขายและสินเชื่อ สาขาบางนา", Actor);
        bnSales.AllocatePosition(pos["SALES-MGR"], 1, Actor);
        bnSales.AllocatePosition(pos["SALES-CONS"], 8, Actor);
        bnSales.AllocatePosition(pos["FI-SPEC"], 2, Actor);

        var bnSvc = brBangna.CreateChild("BN-SVC", "Service & Parts (Bangna)", "ศูนย์บริการและอะไหล่ สาขาบางนา", Actor);
        bnSvc.AllocatePosition(pos["SVC-MGR"], 1, Actor);
        bnSvc.AllocatePosition(pos["SA"], 3, Actor);
        bnSvc.AllocatePosition(pos["FOREMAN"], 1, Actor);
        bnSvc.AllocatePosition(pos["TECH-MECH"], 6, Actor);
        bnSvc.AllocatePosition(pos["TECH-EV"], 2, Actor);
        bnSvc.AllocatePosition(pos["PARTS-SPEC"], 2, Actor);

        // Certified Pre-Owned Unit
        var ucar = root.CreateChild("UCAR", "Certified Pre-Owned Division", "ฝ่ายรถยนต์มือสองรับรองคุณภาพ", Actor);
        ucar.AllocatePosition(pos["UCAR-MGR"], 1, Actor);
        ucar.AllocatePosition(pos["APPRAISER"], 3, Actor);
        ucar.AllocatePosition(pos["SALES-CONS"], 4, Actor);

        return (pos.Values, root);
    }
}