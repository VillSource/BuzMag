using System.Text.Json.Serialization;

namespace Villsource.FSH.Modules.Organization.Contracts.Dtos;

public sealed class OrganizationDto
{
    [JsonIgnore]
    public Guid Id { get; set; }
    public string ReferenceId { get; set; } = string.Empty;
    public DateTimeOffset CreatedOnUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset? LastModifiedOnUtc { get; set; }
    public string? LastModifiedBy { get; set; }
    public bool IsDeleted { get; set; }
    public bool IsDefault { get; set; }
    public DateTimeOffset? DeletedOnUtc { get; set; }
    public string? DeletedBy { get; set; }
    
    
    public IReadOnlyCollection<OrganizationUnitDto> Units { get; set; } = [];
}