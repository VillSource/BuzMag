using Thinktecture;

namespace Villsource.Modules.HumanResource.Contracts.Constants;

[SmartEnum<string>]
public sealed partial class EmployeeStatus
{
    public static readonly EmployeeStatus None = new("NONE");
    public static readonly EmployeeStatus Active = new("ACTIVE");
    public static readonly EmployeeStatus Inactive = new("INACTIVE");
    public static readonly EmployeeStatus Onboarding = new("ONBOARDING");
    public static readonly EmployeeStatus Offboarding = new("OFFBOARDING");
}