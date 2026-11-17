using FSH.Framework.Core.Domain;
using Villsource.FSH.Modules.Organization.Domain.OrganizationUnitTypes.Events;
using Villsource.Tool.UniqueKey;

namespace Villsource.FSH.Modules.Organization.Domain.OrganizationUnitTypes;

public sealed partial class OrganizationUnitType : BaseEntity<Guid>, IAuditableEntity, ISoftDeletable
{
    private OrganizationUnitType() { }

    public string ReferenceId { get; } = VillsourceId.Key;
    public string Code { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string InternationalName { get; private set; } = null!;
    public int HierarchyLevel { get; private set; }
    public string? Description { get; private set; }
    public bool IsActive { get; private set; } = true;

    public static OrganizationUnitType Create(
        string code,
        string name,
        string internationalName,
        int hierarchyLevel,
        string? description = null,
        bool isActive = true)
    {
        var (normalizedCode, normalizedName, normalizedIntlName, level, normalizedDesc) =
            OrganizationUnitTypeRules.NormalizeAndValidate(code, name, internationalName, hierarchyLevel, description);

        var entity = new OrganizationUnitType
        {
            Id = Guid.CreateVersion7(),
            Code = normalizedCode,
            Name = normalizedName,
            InternationalName = normalizedIntlName,
            HierarchyLevel = level,
            Description = normalizedDesc,
            IsActive = isActive
        };

        entity.AddDomainEvent(DomainEvent.Create((id, ts) =>
            new OrganizationUnitTypeCreatedDomainEvent(
                entity.Id,
                entity.Code,
                entity.Name,
                entity.HierarchyLevel,
                id,
                ts)));

        return entity;
    }

    public bool Update(
        string code,
        string name,
        string internationalName,
        int hierarchyLevel,
        string? description = null)
    {
        EnsureNotDeleted();

        var (normalizedCode, normalizedName, normalizedIntlName, level, normalizedDesc) =
            OrganizationUnitTypeRules.NormalizeAndValidate(code, name, internationalName, hierarchyLevel, description);

        if (string.Equals(Code, normalizedCode, StringComparison.Ordinal) &&
            string.Equals(Name, normalizedName, StringComparison.Ordinal) &&
            string.Equals(InternationalName, normalizedIntlName, StringComparison.Ordinal) &&
            HierarchyLevel == level &&
            string.Equals(Description, normalizedDesc, StringComparison.Ordinal))
        {
            return false;
        }

        Code = normalizedCode;
        Name = normalizedName;
        InternationalName = normalizedIntlName;
        HierarchyLevel = level;
        Description = normalizedDesc;

        AddDomainEvent(DomainEvent.Create((id, ts) =>
            new OrganizationUnitTypeUpdatedDomainEvent(Id, Code, Name, HierarchyLevel, id, ts)));

        return true;
    }

    public void Activate()
    {
        EnsureNotDeleted();
        if (IsActive) return;

        IsActive = true;
        AddDomainEvent(DomainEvent.Create((id, ts) =>
            new OrganizationUnitTypeActivatedDomainEvent(Id, Code, id, ts)));
    }

    public void Deactivate()
    {
        EnsureNotDeleted();
        if (!IsActive) return;

        IsActive = false;
        AddDomainEvent(DomainEvent.Create((id, ts) =>
            new OrganizationUnitTypeDeactivatedDomainEvent(Id, Code, id, ts)));
    }

    public void Delete(string? deletedBy = null)
    {
        if (IsDeleted) return;

        DeletedOnUtc = DateTimeOffset.UtcNow;
        DeletedBy = deletedBy;
        IsDeleted = true;
        IsActive = false;

        AddDomainEvent(DomainEvent.Create((id, ts) =>
            new OrganizationUnitTypeDeletedDomainEvent(Id, Code, id, ts)));
    }

    private void EnsureNotDeleted()
    {
        if (IsDeleted)
        {
            throw new InvalidOperationException($"Organization unit type '{Id}' has already been deleted.");
        }
    }
}
