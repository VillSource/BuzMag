using FSH.Framework.Core.Domain;
using Villsource.FSH.Modules.Organization.Domain.Events;

namespace Villsource.FSH.Modules.Organization.Domain;

public sealed class Organization : AggregateRoot<Guid>, IAuditableEntity, ISoftDeletable 
{
    public DateTimeOffset CreatedOnUtc { get; private init; }
    public string? CreatedBy { get; private init; }
    public DateTimeOffset? LastModifiedOnUtc { get; private set; }
    public string? LastModifiedBy { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTimeOffset? DeletedOnUtc { get; private set; }
    public string? DeletedBy { get; private set; }
    public bool IsDefault { get; }
    
    public ICollection<OrganizationUnit> Units { get; private set; } = [];
    public Organization() { }

    public static Organization Create(string? createBy = null) => Create(Guid.CreateVersion7(), createBy);
    public static Organization Create(Guid id, string? createBy = null)
    {
        var model = new Organization
        {
            Id =  id,
            CreatedOnUtc =  DateTimeOffset.UtcNow,
            CreatedBy = createBy,
        };
        model.AddDomainEvent(DomainEvent.Create((id,ts)=>
            new OrganizationCreatedDomainEvent(id, ts)));
        return model;
    }

    public void Modify(string modifiedBy)
    {
        LastModifiedOnUtc = DateTimeOffset.UtcNow;
        LastModifiedBy = modifiedBy;
    }

    public void Delete(string deletedBy)
    {
        DeletedOnUtc = DateTimeOffset.UtcNow;
        DeletedBy = deletedBy;
        IsDeleted = true;
    }
}