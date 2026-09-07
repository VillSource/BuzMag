namespace Villsource.Modules.HumanResource.Contracts.Dtos;

public sealed class EmploymentDto : AuditableDto
{
    public string Ref { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset EffectiveFrom { get; set; }
    public DateTimeOffset? EffectiveTo { get; set; }
    public string? Note { get; set; }
}