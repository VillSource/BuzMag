namespace Villsource.FSH.Modules.Organization.Contracts.Dtos;

public sealed class OrganizationUnitPositionAllocationDto
{
    public Guid Id { get; set; }
    public string OrganizationUnitReferenceId { get; set; } = string.Empty;
    public PositionDto Position { get; set; } = new();
    public int? HeadCount { get; set; }
    public DateTimeOffset EffectiveFrom { get; set; }
    public DateTimeOffset? EffectiveTo { get; set; }
}
