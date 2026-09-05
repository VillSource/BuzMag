using Thinktecture;

namespace Villsource.Modules.HumanResource.Contracts.Constants;

[SmartEnum<string>]
public sealed partial class EmploymentStatus
{
    public static readonly EmploymentStatus None = new("NONE");
    
    // กลุ่ม Active
    public static readonly EmploymentStatus NewHire = new("NEW-HIRE"); // เพิ่งรับเข้า
    public static readonly EmploymentStatus Regular = new("REGULAR");  // พนักงานประจำ (ผ่านโปรแล้ว)
    
    // กลุ่ม Inactive (ออกจากการเป็นพนักงาน)
    public static readonly EmploymentStatus Resigned = new("RESIGNED");       // ลาออกเอง
    public static readonly EmploymentStatus Terminated = new("TERMINATED");   // เลิกจ้าง / ไล่ออก
    public static readonly EmploymentStatus Retired = new("RETIRED");         // เกษียณอายุ
    public static readonly EmploymentStatus Deceased = new("DECEASED");       // เสียชีวิต (สำคัญมากเรื่องกฎหมาย/สวัสดิการ)
    
    // กลุ่มพักงาน 
    public static readonly EmploymentStatus Suspended = new("SUSPENDED");     // พักงาน (ระหว่างสอบสวน)
}