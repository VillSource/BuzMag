namespace Villsource.FSH.Modules.Organization.Contracts.Dtos;

public sealed class OrganizationUnitDto
{
    public Guid Id { get; set; }
    public Guid? ParenId { get; set; }
    public string Path { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTimeOffset CreatedOnUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset? LastModifiedOnUtc { get; set; }
    public string? LastModifiedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedOnUtc { get; set; }
    public string? DeletedBy { get; set; }

    public IReadOnlyCollection<OrganizationUnitDto> Children { get; set; } = [];
    public IReadOnlyCollection<OrganizationUnitDto> Descendants { get; set; } = [];
    public IReadOnlyCollection<PositionDto> Positions { get; set; } = [];
}