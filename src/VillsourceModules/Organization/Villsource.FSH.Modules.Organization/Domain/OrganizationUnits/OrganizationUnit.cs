using FSH.Framework.Core.Domain;
using Villsource.FSH.Modules.Organization.Domain.OrganizationUnits.Events;
using Villsource.FSH.Modules.Organization.Features.v1.Positions;
using Villsource.Tool.UniqueKey;
using Position = Villsource.FSH.Modules.Organization.Domain.Positions.Position;

namespace Villsource.FSH.Modules.Organization.Domain.OrganizationUnits;

public sealed partial class OrganizationUnit : BaseEntity<Guid>, IAuditableEntity, ISoftDeletable
{
    private readonly List<OrganizationUnit> _children = [];

    private OrganizationUnit() { }

    public string ReferenceId { get; } = VillsourceId.Key;
    public string Code { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string InternationalName { get; private set; } = null!;
    public string Type { get; private set; } = null!;
    public string? Description { get; private set; }

    public Guid? LegalEntityId { get; private set; }
    public string? LegalEntityRefId { get; private set; }

    public Guid? ParentId { get; private set; }
    public OrganizationUnit? Parent { get; private set; } = null!;

    public int Level { get; private set; }
    public string HierarchyPath { get; private set; } = null!;

    public string? CostCenterCode { get; private set; }

    public Guid? ManagerPositionId { get; private set; }
    public Position? ManagerPosition { get; private set; }

    public bool IsActive { get; private set; } = true;

    public IReadOnlyCollection<OrganizationUnit> Children => _children.AsReadOnly();

    public static OrganizationUnit Create(
        string code,
        string name,
        string internationalName,
        string type,
        string? description = null,
        Guid? legalEntityId = null,
        string? legalEntityRefId = null,
        OrganizationUnit? parent = null,
        string? costCenterCode = null,
        Position? managerPosition = null,
        bool allowExternalManagerPosition = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(internationalName);
        ArgumentException.ThrowIfNullOrWhiteSpace(type);

        if (parent is not null && parent.IsDeleted)
        {
            throw new InvalidOperationException($"Cannot attach to deleted parent OrganizationUnit '{parent.ReferenceId}'.");
        }

        var entity = new OrganizationUnit
        {
            Id = Guid.CreateVersion7(),
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            InternationalName = internationalName.Trim(),
            Type = type.Trim().ToUpperInvariant(),
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            LegalEntityId = legalEntityId ?? parent?.LegalEntityId,
            LegalEntityRefId = string.IsNullOrWhiteSpace(legalEntityRefId) ? parent?.LegalEntityRefId : legalEntityRefId.Trim(),
            ParentId = parent?.Id,
            Level = parent is null ? 0 : parent.Level + 1,
            CostCenterCode = string.IsNullOrWhiteSpace(costCenterCode) ? null : costCenterCode.Trim().ToUpperInvariant(),
            IsActive = true
        };

        entity.HierarchyPath = parent is null
            ? $"/{entity.ReferenceId}/"
            : $"{parent.HierarchyPath}{entity.ReferenceId}/";

        if (managerPosition is not null)
        {
            ValidateManagerPositionCandidate(
                targetOuId: entity.Id,
                targetOuRefId: entity.ReferenceId,
                candidate: managerPosition,
                allowExternalManagerPosition: allowExternalManagerPosition);

            entity.ManagerPositionId = managerPosition.Id;
            entity.ManagerPosition = managerPosition;
        }

        entity.AddDomainEvent(DomainEvent.Create((id, ts) =>
            new OrganizationUnitCreatedDomainEvent(
                OrganizationUnitId: entity.Id,
                Code: entity.Code,
                Name: entity.Name,
                InternationalName: entity.InternationalName,
                Type: entity.Type,
                LegalEntityId: entity.LegalEntityId,
                ParentId: entity.ParentId,
                ManagerPositionId: entity.ManagerPositionId,
                EventId: id,
                OccurredOnUtc: ts)));

        return entity;
    }

    public void Update(
        string code,
        string name,
        string internationalName,
        string type,
        string? description = null,
        string? costCenterCode = null)
    {
        EnsureNotDeleted();
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(internationalName);
        ArgumentException.ThrowIfNullOrWhiteSpace(type);

        Code = code.Trim().ToUpperInvariant();
        Name = name.Trim();
        InternationalName = internationalName.Trim();
        Type = type.Trim().ToUpperInvariant();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        CostCenterCode = string.IsNullOrWhiteSpace(costCenterCode) ? null : costCenterCode.Trim().ToUpperInvariant();

        AddDomainEvent(DomainEvent.Create((id, ts) =>
            new OrganizationUnitUpdatedDomainEvent(
                OrganizationUnitId: Id,
                Code: Code,
                Name: Name,
                InternationalName: InternationalName,
                Type: Type,
                EventId: id,
                OccurredOnUtc: ts)));
    }

    public void AssignManagerPosition(Position? managerPosition, bool allowExternalManagerPosition = false)
    {
        EnsureNotDeleted();

        var newManagerPositionId = managerPosition?.Id;
        if (ManagerPositionId == newManagerPositionId)
        {
            return;
        }

        if (managerPosition is not null)
        {
            ValidateManagerPositionCandidate(
                targetOuId: Id,
                targetOuRefId: ReferenceId,
                candidate: managerPosition,
                allowExternalManagerPosition: allowExternalManagerPosition);
        }

        var previousManagerPositionId = ManagerPositionId;
        ManagerPositionId = newManagerPositionId;
        ManagerPosition = managerPosition;

        AddDomainEvent(DomainEvent.Create((id, ts) =>
            new OrganizationUnitManagerPositionChangedDomainEvent(
                OrganizationUnitId: Id,
                PreviousManagerPositionId: previousManagerPositionId,
                NewManagerPositionId: newManagerPositionId,
                EventId: id,
                OccurredOnUtc: ts)));
    }

    public void MoveTo(OrganizationUnit? newParent, IReadOnlyCollection<OrganizationUnit> descendants)
    {
        EnsureNotDeleted();

        if (newParent is not null)
        {
            if (newParent.IsDeleted)
            {
                throw new InvalidOperationException($"Target parent OrganizationUnit '{newParent.ReferenceId}' is deleted.");
            }

            if (newParent.Id == Id || newParent.HierarchyPath.StartsWith(HierarchyPath, StringComparison.Ordinal))
            {
                throw new InvalidOperationException("Cannot move an OrganizationUnit under itself or one of its descendants.");
            }
        }

        var oldParentId = ParentId;
        var oldPath = HierarchyPath;
        var oldLevel = Level;

        ParentId = newParent?.Id;
        Level = newParent is null ? 0 : newParent.Level + 1;
        HierarchyPath = newParent is null
            ? $"/{ReferenceId}/"
            : $"{newParent.HierarchyPath}{ReferenceId}/";

        var levelDelta = Level - oldLevel;

        foreach (var descendant in descendants.Where(d => !d.IsDeleted && d.Id != Id))
        {
            descendant.ApplyAncestorMove(
                oldAncestorPath: oldPath,
                newAncestorPath: HierarchyPath,
                levelDelta: levelDelta);
        }

        AddDomainEvent(DomainEvent.Create((id, ts) =>
            new OrganizationUnitMovedDomainEvent(
                OrganizationUnitId: Id,
                OldParentId: oldParentId,
                NewParentId: ParentId,
                NewHierarchyPath: HierarchyPath,
                NewLevel: Level,
                EventId: id,
                OccurredOnUtc: ts)));
    }

    internal void ApplyAncestorMove(string oldAncestorPath, string newAncestorPath, int levelDelta)
    {
        HierarchyPath = string.Concat(newAncestorPath, HierarchyPath.AsSpan(oldAncestorPath.Length));
        Level += levelDelta;
    }

    public void Activate()
    {
        EnsureNotDeleted();
        if (IsActive)
        {
            return;
        }

        IsActive = true;

        AddDomainEvent(DomainEvent.Create((id, ts) =>
            new OrganizationUnitStatusChangedDomainEvent(
                OrganizationUnitId: Id,
                IsActive: true,
                EventId: id,
                OccurredOnUtc: ts)));
    }

    public void Deactivate()
    {
        EnsureNotDeleted();
        if (!IsActive)
        {
            return;
        }

        IsActive = false;

        AddDomainEvent(DomainEvent.Create((id, ts) =>
            new OrganizationUnitStatusChangedDomainEvent(
                OrganizationUnitId: Id,
                IsActive: false,
                EventId: id,
                OccurredOnUtc: ts)));
    }

    public void Delete(string? deletedBy = null)
    {
        if (IsDeleted)
        {
            return;
        }

        if (_children.Any(c => !c.IsDeleted))
        {
            throw new InvalidOperationException($"Cannot delete OrganizationUnit '{ReferenceId}' while it still has active child units.");
        }

        DeletedOnUtc = DateTimeOffset.UtcNow;
        DeletedBy = deletedBy;
        IsDeleted = true;
        IsActive = false;
        ManagerPositionId = null;
        ManagerPosition = null;

        AddDomainEvent(DomainEvent.Create((id, ts) =>
            new OrganizationUnitDeletedDomainEvent(
                OrganizationUnitId: Id,
                EventId: id,
                OccurredOnUtc: ts)));
    }

    private static void ValidateManagerPositionCandidate(
        Guid targetOuId,
        string targetOuRefId,
        Position candidate,
        bool allowExternalManagerPosition)
    {
        if (candidate.IsDeleted)
        {
            throw new InvalidOperationException($"Cannot assign deleted Position '{candidate.ReferenceId}' as manager of OrganizationUnit '{targetOuRefId}'.");
        }

        if (!candidate.IsActive)
        {
            throw new InvalidOperationException($"Cannot assign inactive Position '{candidate.ReferenceId}' as manager of OrganizationUnit '{targetOuRefId}'.");
        }

        if (!allowExternalManagerPosition && candidate.OrgUnitId != targetOuId)
        {
            throw new InvalidOperationException(
                $"Position '{candidate.ReferenceId}' belongs to another OrganizationUnit. Enable 'AllowExternalManagerPosition' for matrix/shared leadership.");
        }
    }

    private void EnsureNotDeleted()
    {
        if (IsDeleted)
        {
            throw new InvalidOperationException($"OrganizationUnit '{ReferenceId}' has already been deleted.");
        }
    }
}