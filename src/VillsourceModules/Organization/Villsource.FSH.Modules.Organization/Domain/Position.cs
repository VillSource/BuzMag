using FSH.Framework.Core.Domain;
using Thinktecture;
using Villsource.FSH.Modules.Organization.Contracts.Constants;
using Villsource.FSH.Modules.Organization.Domain.Events;
using Villsource.Tool.UniqueKey;

namespace Villsource.FSH.Modules.Organization.Domain;

public sealed class Position : BaseEntity<Guid>, IAuditableEntity, ISoftDeletable
{
    public string ReferenceId { get; } = VillsourceId.Key;
    public string Name { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public IReadOnlyCollection<PositionTier> PositionTiers { get; private set; } = [];
    public DateTimeOffset CreatedOnUtc { get; private init; }
    public string? CreatedBy { get; private init; }
    public DateTimeOffset? LastModifiedOnUtc { get; private set; }
    public string? LastModifiedBy { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTimeOffset? DeletedOnUtc { get; private set; }
    public string? DeletedBy { get; private set; }

    public Position() { }

    public static Position Create(string code, string name, string? description = null, string? createBy = null)
    {
        ArgumentNullException.ThrowIfNull(code);
        
        var model = new Position
        {
            Code = code.ToUpperInvariant(),
            Name = name,
            Description =  description,
            Id =  Guid.CreateVersion7(),
            CreatedOnUtc =  DateTimeOffset.UtcNow,
            CreatedBy = createBy,
        };
        model.AddDomainEvent(DomainEvent.Create((id,ts)=>
            new OrganizationCreatedDomainEvent(id, ts)));
        return model;
    }

    public void Modify(string code, string name, string? description = null, string? modifiedBy = null)
    {
        Code = code;
        Name = name;
        Description =  description;
        LastModifiedOnUtc = DateTimeOffset.UtcNow;
        LastModifiedBy = modifiedBy;
    }

    public void SetTier(ISet<PositionTier> positionTier)
    {
        PositionTiers = positionTier.ToList();
    }

    public void Delete(string? deletedBy = null)
    {
        DeletedOnUtc = DateTimeOffset.UtcNow;
        DeletedBy = deletedBy;
        IsDeleted = true;
    }
}