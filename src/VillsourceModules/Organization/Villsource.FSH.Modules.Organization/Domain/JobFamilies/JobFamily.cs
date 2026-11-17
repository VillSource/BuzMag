using FSH.Framework.Core.Domain;
using Villsource.FSH.Modules.Organization.Domain.JobFamilies.Events;
using Villsource.Tool.UniqueKey;

namespace Villsource.FSH.Modules.Organization.Domain.JobFamilies;

public sealed partial class JobFamily : BaseEntity<Guid>, IAuditableEntity, ISoftDeletable
{
    private JobFamily() { }

    public string ReferenceId { get; } = VillsourceId.Key;
    public string Code { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string InternationalName { get; private set; } = null!;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; } = true;

    public static JobFamily Create(
        string code,
        string name,
        string internationalName,
        string? description = null,
        bool isActive = true)
    {
        var (normalizedCode, normalizedName, normalizedIntlName, normalizedDesc) =
            JobFamilyRules.NormalizeAndValidate(code, name, internationalName, description);

        var entity = new JobFamily
        {
            Id = Guid.CreateVersion7(),
            Code = normalizedCode,
            Name = normalizedName,
            InternationalName = normalizedIntlName,
            Description = normalizedDesc,
            IsActive = isActive
        };

        entity.AddDomainEvent(DomainEvent.Create((id, ts) =>
            new JobFamilyCreatedDomainEvent(entity.Id, entity.Code, entity.Name, id, ts)));

        return entity;
    }

    public bool Update(
        string code,
        string name,
        string internationalName,
        string? description = null)
    {
        EnsureNotDeleted();

        var (normalizedCode, normalizedName, normalizedIntlName, normalizedDesc) =
            JobFamilyRules.NormalizeAndValidate(code, name, internationalName, description);

        if (string.Equals(Code, normalizedCode, StringComparison.Ordinal) &&
            string.Equals(Name, normalizedName, StringComparison.Ordinal) &&
            string.Equals(InternationalName, normalizedIntlName, StringComparison.Ordinal) &&
            string.Equals(Description, normalizedDesc, StringComparison.Ordinal))
        {
            return false;
        }

        Code = normalizedCode;
        Name = normalizedName;
        InternationalName = normalizedIntlName;
        Description = normalizedDesc;

        AddDomainEvent(DomainEvent.Create((id, ts) =>
            new JobFamilyUpdatedDomainEvent(Id, Code, Name, id, ts)));

        return true;
    }

    public void Activate()
    {
        EnsureNotDeleted();
        if (IsActive) return;

        IsActive = true;
        AddDomainEvent(DomainEvent.Create((id, ts) =>
            new JobFamilyActivatedDomainEvent(Id, Code, id, ts)));
    }

    public void Deactivate()
    {
        EnsureNotDeleted();
        if (!IsActive) return;

        IsActive = false;
        AddDomainEvent(DomainEvent.Create((id, ts) =>
            new JobFamilyDeactivatedDomainEvent(Id, Code, id, ts)));
    }

    public void Delete(string? deletedBy = null)
    {
        if (IsDeleted) return;

        DeletedOnUtc = DateTimeOffset.UtcNow;
        DeletedBy = deletedBy;
        IsDeleted = true;
        IsActive = false;

        AddDomainEvent(DomainEvent.Create((id, ts) =>
            new JobFamilyDeletedDomainEvent(Id, Code, id, ts)));
    }

    private void EnsureNotDeleted()
    {
        if (IsDeleted)
        {
            throw new InvalidOperationException($"Job family '{Id}' has already been deleted.");
        }
    }
}
