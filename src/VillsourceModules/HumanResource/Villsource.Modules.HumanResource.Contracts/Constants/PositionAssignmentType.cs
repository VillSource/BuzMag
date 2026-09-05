using Thinktecture;

namespace Villsource.Modules.HumanResource.Contracts.Constants;

[SmartEnum<string>]
public sealed partial class PositionAssignmentType
{
    public static readonly PositionAssignmentType None = new("NONE");
    public static readonly PositionAssignmentType Initial = new("INITIAL");       // บรรจุครั้งแรก
    public static readonly PositionAssignmentType Promotion = new("PROMOTION");   // เลื่อนขั้น
    public static readonly PositionAssignmentType Demotion = new("DEMOTION");     // ลดขั้น
    public static readonly PositionAssignmentType Transfer = new("TRANSFER");     // โยกย้ายแผนก/สาขา
    public static readonly PositionAssignmentType Acting = new("ACTING");         // รักษาการ (ชั่วคราว)
    public static readonly PositionAssignmentType Secondment = new("SECONDMENT"); // ยืมตัว (ไปช่วยงานแผนกอื่นชั่วคราว) 
}