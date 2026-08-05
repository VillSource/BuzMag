using FSH.Framework.Core.Domain;
using Microsoft.EntityFrameworkCore;
using System.Collections.Frozen;
using Villsource.FSH.Modules.Organization.Data;
using Villsource.FSH.Modules.Organization.Domain.Events;

namespace Villsource.FSH.Modules.Organization.Domain;

public sealed class OrganizationUnit : AggregateRoot<Guid>, IAuditableEntity, ISoftDeletable
{
    public Guid OrganizationId { get; private set; } = Guid.Empty;
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

    public static OrganizationUnit Create(string code, string name, string? description = null,
        Organization? organization = null, string? createBy = null)
    {
        ArgumentNullException.ThrowIfNull(organization);

        var model = new OrganizationUnit
        {
            OrganizationId = organization.Id,
            Path = "/",
            ParenId = null,
            Code = code,
            Name = name,
            Description = description,
            Id = Guid.CreateVersion7(),
            CreatedOnUtc = DateTimeOffset.UtcNow,
            CreatedBy = createBy,
        };
        model.AddDomainEvent(DomainEvent.Create((id, ts) =>
            new OrganizationCreatedDomainEvent(id, ts)));
        return model;
    }

    public OrganizationUnit CreateChild(string code, string name, string? description = null, string? createBy = null)
    {
        if (Path.Length < 1)
            throw new ArgumentException("Parent must have a valid Path.");

        var model = new OrganizationUnit
        {
            OrganizationId = OrganizationId,
            Path = string.Concat(Path.TrimEnd('/'), "/", Id.ToString("N")),
            ParenId = Id,
            Code = code,
            Name = name,
            Description = description,
            Id = Guid.CreateVersion7(),
            CreatedOnUtc = DateTimeOffset.UtcNow,
            CreatedBy = createBy,
        };
        model.AddDomainEvent(DomainEvent.Create((id, ts) =>
            new OrganizationCreatedDomainEvent(id, ts)));
        return model;
    }

    public void Modify(string code, string name, string? description = null, string? modifiedBy = null)
    {
        Code = code;
        Name = name;
        Description = description;
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
        
        AddDomainEvent(DomainEvent.Create((id, ts) =>
            new OrganizationUnitDeletedDomainEvent(
                OrganizationId: OrganizationId,
                OrganizationUnitId: Id,
                EventId: id,
                OccurredOnUtc: ts)));
    }

    public async Task<List<OrganizationUnit>> GetDescendantsAsync(OrganizationDbContext dbContext,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        if (Path.Length < 1)
            throw new InvalidOperationException("Organization must have a valid Path.");

        var rootPath = $"{Path.TrimEnd('/')}/{Id:N}";

        var descendants = dbContext.OrganizationUnits
            .Where(ou => ou.OrganizationId == OrganizationId)
            .Where(ou => ou.Path.StartsWith(rootPath));

        return await descendants.ToListAsync(ct).ConfigureAwait(false);
    }

    public static ICollection<OrganizationUnit> BuildTree(IReadOnlyCollection<OrganizationUnit> flatList)
    {
        var parentLookup = flatList.ToLookup(u => u.ParenId);
        var idSet = flatList.Select(u => u.Id).ToFrozenSet();
        
        var rootNodes = parentLookup 
            .Where(l => !l.Key.HasValue || !idSet.Contains(l.Key.Value))
            .Select(l => l.Key);

        return [.. rootNodes.SelectMany(BuildChildren)];

        List<OrganizationUnit> BuildChildren(Guid? parentId = null) =>
        [
            .. parentLookup[parentId].Select(u =>
            {
                u.Children = BuildChildren(u.Id);
                return u;
            })
        ];
    }
}