using Villsource.ObjectValue;

namespace Villsource.Modules.HumanResource.Contracts.Dtos;

public sealed class EmployeeDto : AuditableDto
{
    public string Ref { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string LastName { get; set; } = string.Empty;
    public string TitleEn { get; set; } = string.Empty;
    public string FirstNameEn { get; set; } = string.Empty;
    public string? MiddleNameEn { get; set; }
    public string LastNameEn { get; set; } = string.Empty;
    public Address Address { get; set; } = Address.Empty();
    public string? Email { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset? LastHireDate { get; set; }
    public DateTimeOffset? FirstHireDate { get; set; }
    public string? SnapshotManagerRef { get; set; } 
    public string? SnapshotOuRef { get; set; }
    public string? SnapshotPositionRef { get; set; }
    public string? SnapshotTier { get; set; }
}