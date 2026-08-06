using System.Collections.Frozen;

namespace Villsource.FSH.Modules.Organization.Contracts.Dtos;

public sealed class OrganizationUnitDto
{
    public Guid Id { get; set; }
    public string ReferenceId { get; set; } = string.Empty;
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
    
    public static ICollection<OrganizationUnitDto> BuildTree(IReadOnlyCollection<OrganizationUnitDto> flatList)
    {
        var parentLookup = flatList.ToLookup(u => u.ParenId);
        var idSet = flatList.Select(u => u.Id).ToFrozenSet();

        var rootNodes = parentLookup 
            .Where(l => !l.Key.HasValue || !idSet.Contains(l.Key.Value))
            .Select(l => l.Key);

        return [.. rootNodes.SelectMany(BuildChildren)];

        List<OrganizationUnitDto> BuildChildren(Guid? parentId = null) =>
        [
            .. parentLookup[parentId].Select(u =>
            {
                u.Children = BuildChildren(u.Id);
                return u;
            })
        ];
    }
}