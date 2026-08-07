using FSH.Framework.Core.Domain;
using Microsoft.EntityFrameworkCore;
using NanoidDotNet;
using System.Collections.Frozen;
using Villsource.FSH.Modules.Organization.Data;
using Villsource.FSH.Modules.Organization.Domain.Events;
using Villsource.Tool.UniqueKey;
using static System.Text.RegularExpressions.Regex;

namespace Villsource.FSH.Modules.Organization.Domain;

public sealed class OrganizationUnit : AggregateRoot<Guid>, IAuditableEntity, ISoftDeletable
{
    public Guid OrganizationId { get; private set; } = Guid.Empty;
    public Guid? ParenId { get; private set; }
    public string ReferenceId { get; } = VillsourceId.Key;
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

    public ICollection<OrganizationUnit> Children { get; private set; } = [];

    public OrganizationUnit() { }

    public static OrganizationUnit Create(string code, string name, string? description = null,
        Organization? organization = null, string? createBy = null)
    {
        ArgumentNullException.ThrowIfNull(organization);
        ArgumentNullException.ThrowIfNull(code);
        ArgumentNullException.ThrowIfNull(name);

        var model = new OrganizationUnit
        {
            OrganizationId = organization.Id,
            ParenId = null,
            Code = code.ToUpperInvariant(),
            Name = name,
            Description = description,
            Id = Guid.CreateVersion7(),
            CreatedOnUtc = DateTimeOffset.UtcNow,
            CreatedBy = createBy,
        };

        model.Path = $"/{model.ReferenceId}";
        model.AddDomainEvent(DomainEvent.Create((id, ts) =>
            new OrganizationCreatedDomainEvent(id, ts)));
        return model;
    }

    public OrganizationUnit CreateChild(string code, string name, string? description = null, string? createBy = null)
    {
        ArgumentNullException.ThrowIfNull(code);
        ArgumentNullException.ThrowIfNull(name);

        if (Path.Length < 1)
            throw new ArgumentException("Parent must have a valid Path.");

        var model = new OrganizationUnit
        {
            OrganizationId = OrganizationId,
            ParenId = Id,
            Code = code.ToUpperInvariant(),
            Name = name,
            Description = description,
            Id = Guid.CreateVersion7(),
            CreatedOnUtc = DateTimeOffset.UtcNow,
            CreatedBy = createBy,
        };

        model.Path = string.Concat(Path.TrimEnd('/'), "/", model.ReferenceId);
        model.AddDomainEvent(DomainEvent.Create((id, ts) =>
            new OrganizationCreatedDomainEvent(id, ts)));
        
        Children.Add(model);
        return model;
    }

    public void Update(string code, string name, string? description, string? modifiedBy = null)
    {
        ArgumentNullException.ThrowIfNull(code);
        ArgumentNullException.ThrowIfNull(Name);

        Code = code.ToUpperInvariant();
        Name = name;
        Description = description;
        LastModifiedOnUtc = DateTimeOffset.UtcNow;
        LastModifiedBy = modifiedBy;
    }

    private void ThrowIfInvalidPath()
    {
        if (Path.Length < 11)
            throw new InvalidOperationException("The organization unit has an invalid path.");
    }

    public void Move(OrganizationUnit? parent)
    {
        if (parent is null)
        {
            ParenId = null;
            Path = $"/{ReferenceId}";
            return;
        }
        parent.ThrowIfInvalidPath();
        OrganizationId = parent.OrganizationId;
        ParenId = parent.Id;
        Path = string.Concat(parent.Path.TrimEnd('/'), "/", ReferenceId);
    }

    public void Move(Organization organization, OrganizationUnit? parent = null)
    {
        ArgumentNullException.ThrowIfNull(organization);
        
        if (parent is not null && parent.OrganizationId != organization.Id)
            throw new ArgumentException("The parent organization unit must belong to the same organization.",
                nameof(parent));
        
        Move(parent);
        OrganizationId = organization.Id;
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

        if (Path.Length < 11)
            throw new InvalidOperationException("Organization must have a valid Path.");

        var descendants = dbContext.OrganizationUnits
            .Where(ou => ou.OrganizationId == OrganizationId)
            .Where(ou => ou.Path.StartsWith(Path))
            .OrderBy(ou => ou.Path);

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


    public void MoveChildren(IList<OrganizationUnit> descendants)
    {
        ArgumentNullException.ThrowIfNull(descendants);
        foreach (var descendant in descendants)
        {
            descendant.OrganizationId = OrganizationId;
            descendant.Path = Replace(descendant.Path, $"^.*{ReferenceId}", Path);
        }
    }
}