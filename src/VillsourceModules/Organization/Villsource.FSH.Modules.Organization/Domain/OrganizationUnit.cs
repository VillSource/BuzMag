using FSH.Framework.Core.Domain;
using Microsoft.EntityFrameworkCore;
using Villsource.FSH.Modules.Organization.Data;
using Villsource.FSH.Modules.Organization.Domain.Events;

namespace Villsource.FSH.Modules.Organization.Domain;


public sealed class OrganizationUnit : AggregateRoot<Guid>, IAuditableEntity, ISoftDeletable
{
    public Guid? ParenId { get; private set; }
    public string Path { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public DateTimeOffset CreatedOnUtc { get; private init; }
    public string? CreatedBy { get; private init; }
    public DateTimeOffset? LastModifiedOnUtc { get; private set; }
    public string? LastModifiedBy { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTimeOffset? DeletedOnUtc { get; private set; }
    public string? DeletedBy { get; private set; }

    public ICollection<Position> Positions { get; private set; } = [];
    public ICollection<OrganizationUnit> Children { get; private set; } = [];
    
    public OrganizationUnit() { }

    public static OrganizationUnit Create(string code, string name, string? description = null, OrganizationUnit? parent = null, string? createBy = null)
    {
        if (parent is { Path.Length: < 1 })
            throw new ArgumentException("Parent must have a valid Path.");
        
        var model = new OrganizationUnit
        {
            Path = parent == null ? "/" : string.Concat(parent.Path.TrimEnd('/'), "/", parent.Id.ToString("N")),
            ParenId =  parent?.Id,
            Code = code,
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

    public void Move(OrganizationUnit? parent)
    {
        if (parent is { Path.Length: < 1 })
            throw new ArgumentException("Parent must have a valid Path.");
        
        ParenId = parent?.Id;
        Path = parent == null ? "/" : string.Concat(parent.Path.TrimEnd('/'), "/", parent.Id.ToString("N"));
    }

    public void Delete(string? deletedBy = null)
    {
        DeletedOnUtc = DateTimeOffset.UtcNow;
        DeletedBy = deletedBy;
        IsDeleted = true;
    }
    
    public async Task<List<OrganizationUnit>> GetDescendantsAsync(OrganizationDbContext dbContext,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        if (string.IsNullOrEmpty(Path))
            throw new InvalidOperationException("Current OU must have a valid Path.");

        var rootPath = $"{Path.TrimEnd('/')}/{Id:N}";

        var descendants = dbContext.OrganizationUnits.AsNoTracking()
            .Where(ou => ou.Path.StartsWith(rootPath));

        return await descendants.ToListAsync(ct).ConfigureAwait(false);
    }

    public static ICollection<OrganizationUnit> BuildTree(IReadOnlyCollection<OrganizationUnit> flatList)
    {
        var lookup = flatList.ToLookup(u => u.ParenId);

        return BuildChildren();

        List<OrganizationUnit> BuildChildren(Guid? parentId = null)
        {
            var tree = lookup[parentId].Select(u =>
            {
                u.Children = BuildChildren(u.Id);
                return u;
            });
            return tree.ToList();
        }
    }
}