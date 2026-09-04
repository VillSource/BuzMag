using FSH.Framework.Core.Domain;
using Microsoft.EntityFrameworkCore;
using System.Collections.Frozen;
using Villsource.FSH.Modules.Organization.Data;
using Villsource.FSH.Modules.Organization.Domain.Events;
using Villsource.Tool.UniqueKey;
using static System.Text.RegularExpressions.Regex;

namespace Villsource.FSH.Modules.Organization.Domain;

public sealed class OrganizationUnit : AggregateRoot<Guid>, IAuditableEntity, ISoftDeletable
{
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
    public ICollection<OrganizationUnitPositionAllocation> PositionAllocations { get; private set; } = [];

    public OrganizationUnit() { }

    public void AllocatePosition(Position position, int? headCount, string? createBy = null)
    {
        ArgumentNullException.ThrowIfNull(position);

        var po = OrganizationUnitPositionAllocation.Create(
            ouId: Id,
            positionId: position.Id,
            headCount: headCount,
            effectiveDate: DateTimeOffset.UtcNow,
            createBy: createBy);
        PositionAllocations.Add(po);
    }

    public static OrganizationUnit Create(string code, string name, string? description = null, string? createBy = null)
    {
        ArgumentNullException.ThrowIfNull(code);
        ArgumentNullException.ThrowIfNull(name);

        var model = new OrganizationUnit
        {
            ParenId = null,
            Code = code.ToUpperInvariant(),
            Name = name,
            Description = description,
            Id = Guid.CreateVersion7(),
            CreatedOnUtc = DateTimeOffset.UtcNow,
            CreatedBy = createBy,
        };

        model.Path = $"/{model.ReferenceId}";
        model.AddOrganizationUnitCreatedDomainEvent();
        return model;
    }

    private void AddOrganizationUnitCreatedDomainEvent() => AddDomainEvent(DomainEvent.Create((id, ts) =>
        new OrganizationUnitCreatedDomainEvent(
            Id: Id,
            ReferenceId: ReferenceId,
            Code: Code,
            Name: Name,
            EventId: id,
            OccurredOnUtc: ts)));

    public OrganizationUnit CreateChild(string code, string name, string? description = null, string? createBy = null)
    {
        ThrowIfInvalidPath();

        var model = Create(code, name, description, createBy);
        model.ParenId = Id;
        model.Path = string.Concat(Path.TrimEnd('/'), "/", model.ReferenceId);
        model.ClearDomainEvents();
        model.AddOrganizationUnitCreatedDomainEvent();

        Children.Add(model);
        return model;
    }

    private void AddOrganizationUnitUpdatedDomainEvent() => AddDomainEvent(DomainEvent.Create((id, ts) =>
        new OrganizationUnitUpdatedDomainEvent(
            OrganizationUnitId: Id,
            EventId: id,
            OccurredOnUtc: ts)));

    public void Update(string code, string name, string? description, string? modifiedBy = null)
    {
        ArgumentNullException.ThrowIfNull(code);
        ArgumentNullException.ThrowIfNull(Name);

        Code = code.ToUpperInvariant();
        Name = name;
        Description = description;
        LastModifiedOnUtc = DateTimeOffset.UtcNow;
        LastModifiedBy = modifiedBy;

        AddOrganizationUnitUpdatedDomainEvent();
    }

    private void ThrowIfInvalidPath()
    {
        if (Path.Length <= VillsourceId.KeySizes)
            throw new InvalidOperationException("The organization unit has an invalid path.");
    }

    public void MoveTo(OrganizationUnit? parent)
    {
        if (parent is null)
        {
            ParenId = null;
            Path = $"/{ReferenceId}";
            return;
        }

        parent.ThrowIfInvalidPath();
        ParenId = parent.Id;
        Path = string.Concat(parent.Path.TrimEnd('/'), "/", ReferenceId);
    }

    public void Delete(string? deletedBy = null)
    {
        DeletedOnUtc = DateTimeOffset.UtcNow;
        DeletedBy = deletedBy;
        IsDeleted = true;

        AddOrganizationUnitDeletedDomainEvent();
    }

    private void AddOrganizationUnitDeletedDomainEvent() => AddDomainEvent(DomainEvent.Create((id, ts) =>
        new OrganizationUnitDeletedDomainEvent(
            OrganizationUnitId: Id,
            EventId: id,
            OccurredOnUtc: ts)));

    public void RebaseDescendants(IList<OrganizationUnit> descendants)
    {
        ArgumentNullException.ThrowIfNull(descendants);
        string searchToken = $"/{ReferenceId}/"; 
        string endToken = $"/{ReferenceId}";

        foreach (var descendant in descendants)
        {
            int index = descendant.Path.IndexOf(searchToken, StringComparison.Ordinal);

            if (index >= 0)
            {
                int remainderStartIndex = index + endToken.Length; 
                descendant.Path = string.Concat(Path, descendant.Path.AsSpan(remainderStartIndex));
                continue;
            }
            if (descendant.Path.EndsWith(endToken, StringComparison.Ordinal))
            {
                descendant.Path = Path;
                continue;
            }
            descendant.Path = $"{Path}/{descendant.ReferenceId}";
        }

    }
}