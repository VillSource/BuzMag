namespace Villsource.Modules.HumanResource.Contracts.Dtos;

public sealed class PositionAssignmentDto : AuditableDto
{
    public string Ref { get; set; } = string.Empty;
    public string? ManagerRef { get; set; }
    public string OrganizationUnitRef { get; set; } = string.Empty;
    public string PositionRef { get; set; } = string.Empty;
    public string PositionTier { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public DateTimeOffset EffectiveFrom { get; set; }
    public DateTimeOffset? EffectiveTo { get; set; }
}