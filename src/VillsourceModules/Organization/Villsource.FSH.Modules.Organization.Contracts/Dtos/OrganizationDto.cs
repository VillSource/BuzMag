namespace Villsource.FSH.Modules.Organization.Contracts.Dtos;

public sealed class OrganizationDto
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedOnUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset? LastModifiedOnUtc { get; set; }
    public string? LastModifiedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedOnUtc { get; set; }
    public string? DeletedBy { get; set; }
    
    
    public IReadOnlyCollection<OrganizationUnitDto> Units { get; set; } = [];
}