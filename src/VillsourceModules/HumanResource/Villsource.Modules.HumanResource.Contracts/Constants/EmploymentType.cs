using Thinktecture;

namespace Villsource.Modules.HumanResource.Contracts.Constants;

[SmartEnum<string>]
public sealed partial class EmploymentType
{
    public static readonly EmploymentType None = new("NONE");
    public static readonly EmploymentType Internship = new("INTERNSHIP");
    public static readonly EmploymentType Probation = new("PROBATION");
    public static readonly EmploymentType PartTime = new("PART-TIME");
    public static readonly EmploymentType FullTime = new("FULL-TIME");
    public static readonly EmploymentType Contract = new("CONTRACT");
}