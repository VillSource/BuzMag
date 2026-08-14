using FSH.Framework.Core.Domain;
using Villsource.FSH.Modules.Organization.Domain.Events;
using Villsource.Tool.UniqueKey;

namespace Villsource.FSH.Modules.Organization.Domain;

public sealed class OrganizationUnitPositionAllocation : BaseEntity<Guid>, IAuditableEntity, ISoftDeletable
{
    public Guid OrganizationUnitId { get; private set; } = Guid.Empty;
    public Guid PositionId { get; private set; } = Guid.Empty;
    public int? HeadCount { get; private set; }
    public DateTimeOffset EffectiveFrom { get; private set; }
    public DateTimeOffset? EffectiveTo { get; private set; }
    public DateTimeOffset CreatedOnUtc { get; private init; }
    public string? CreatedBy { get; private init; }
    public DateTimeOffset? LastModifiedOnUtc { get; private set; }
    public string? LastModifiedBy { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTimeOffset? DeletedOnUtc { get; private set; }
    public string? DeletedBy { get; private set; }

    public OrganizationUnit Unit { get; } = new ();
    public Position Position { get; } = new();

    public OrganizationUnitPositionAllocation() { }

    public static OrganizationUnitPositionAllocation Create(
        Guid ouId,
        Guid positionId,
        int? headCount,
        DateTimeOffset effectiveDate,
        string? createBy = null)
    {
        var model = new OrganizationUnitPositionAllocation
        {
            Id = Guid.CreateVersion7(),
            OrganizationUnitId = ouId,
            PositionId =  positionId,
            HeadCount = headCount,
            EffectiveFrom = effectiveDate,
            EffectiveTo = null,
            CreatedOnUtc = DateTimeOffset.UtcNow,
            CreatedBy = createBy,
        };
        model.AddDomainEvent(DomainEvent.Create((id, ts) =>
            new OrganizationUnitPositionAllocatedDomainEvent(id, ts)));
        return model;
    }

    public void EndEffective(string? modifiedBy = null)
    {
        EffectiveTo = DateTimeOffset.UtcNow;
        LastModifiedOnUtc = DateTimeOffset.UtcNow;
        LastModifiedBy = modifiedBy;
    }

    public void Delete(string? deletedBy = null)
    {
        DeletedOnUtc = DateTimeOffset.UtcNow;
        DeletedBy = deletedBy;
        IsDeleted = true;
    }
}