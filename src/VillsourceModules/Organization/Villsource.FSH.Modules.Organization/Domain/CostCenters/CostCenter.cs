using FSH.Framework.Core.Domain;
using Villsource.FSH.Modules.Organization.Contracts.Constants;
using Villsource.FSH.Modules.Organization.Domain.CostCenters.Events;
using Villsource.FSH.Modules.Organization.Domain.LegalEntities;
using Villsource.Tool.UniqueKey;
using Position = Villsource.FSH.Modules.Organization.Domain.Positions.Position;

namespace Villsource.FSH.Modules.Organization.Domain.CostCenters;

public sealed partial class CostCenter : BaseEntity<Guid>, IAuditableEntity, ISoftDeletable
{
    private readonly List<CostCenter> _children = [];

    private CostCenter() { }

    // 1. Identity & Ownership
    public string ReferenceId { get; } = VillsourceId.Key;
    public Guid LegalEntityId { get; private set; }
    public LegalEntity? LegalEntity { get; } = null!;

    public string Code { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string InternationalName { get; private set; } = null!;
    public string? Description { get; private set; }

    // 2. Hierarchy & Classification
    public Guid? ParentCostCenterId { get; private set; }
    public CostCenter? ParentCostCenter { get; } = null!;
    public IReadOnlyCollection<CostCenter> Children => _children.AsReadOnly();

    public CostCenterFunctionType FunctionType { get; private set; } = null!;

    // 3. Accountability (HR / Org Link)
    public Guid? ResponsiblePositionId { get; private set; }
    public Position? ResponsiblePosition { get; } = null!;
    public Guid? OrgUnitId { get; private set; }

    // 4. Validity & Lifecycle
    public DateTimeOffset? ValidFrom { get; private set; }
    public DateTimeOffset? ValidTo { get; private set; }
    public bool IsActive { get; private set; } = true;
    public bool IsPostingBlocked { get; private set; }

    // 5. Tax & Reporting Link
    public Guid? DefaultTaxBranchId { get; private set; }
    public TaxBranch? DefaultTaxBranch { get; } = null!;

    public static CostCenter Create(
        Guid legalEntityId,
        string code,
        string name,
        string internationalName,
        CostCenterFunctionType functionType,
        string? description = null,
        Guid? parentCostCenterId = null,
        Guid? defaultTaxBranchId = null,
        Guid? responsiblePositionId = null,
        Guid? orgUnitId = null,
        DateTimeOffset? validFrom = null,
        DateTimeOffset? validTo = null,
        bool isActive = true,
        bool isPostingBlocked = false)
    {
        if (legalEntityId == Guid.Empty)
        {
            throw new ArgumentException("LegalEntityId cannot be empty.", nameof(legalEntityId));
        }

        ArgumentNullException.ThrowIfNull(functionType);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(internationalName);

        var id = Guid.CreateVersion7();
        if (parentCostCenterId.HasValue && parentCostCenterId.Value == id)
        {
            throw new InvalidOperationException("A cost center cannot be its own parent.");
        }

        var normalizedCode = CostCenterRules.NormalizeAndValidateCode(code);
        var (utcValidFrom, utcValidTo) = CostCenterRules.NormalizeAndValidateValidity(validFrom, validTo);

        var entity = new CostCenter
        {
            Id = id,
            LegalEntityId = legalEntityId,
            ParentCostCenterId = parentCostCenterId,
            DefaultTaxBranchId = defaultTaxBranchId,
            Code = normalizedCode,
            Name = name.Trim(),
            InternationalName = internationalName.Trim(),
            Description = NormalizeOptionalText(description),
            FunctionType = functionType,
            ResponsiblePositionId = responsiblePositionId,
            OrgUnitId = orgUnitId,
            ValidFrom = utcValidFrom,
            ValidTo = utcValidTo,
            IsActive = isActive,
            IsPostingBlocked = isPostingBlocked
        };

        entity.AddCostCenterCreatedDomainEvent();
        return entity;
    }

    public void Update(
        string code,
        string name,
        string internationalName,
        CostCenterFunctionType functionType,
        string? description = null,
        Guid? parentCostCenterId = null,
        Guid? defaultTaxBranchId = null,
        Guid? responsiblePositionId = null,
        Guid? orgUnitId = null,
        DateTimeOffset? validFrom = null,
        DateTimeOffset? validTo = null)
    {
        EnsureNotDeleted();
        ArgumentNullException.ThrowIfNull(functionType);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(internationalName);

        if (parentCostCenterId.HasValue && parentCostCenterId.Value == Id)
        {
            throw new InvalidOperationException("A cost center cannot be its own parent.");
        }

        var normalizedCode = CostCenterRules.NormalizeAndValidateCode(code);
        var (utcValidFrom, utcValidTo) = CostCenterRules.NormalizeAndValidateValidity(validFrom, validTo);

        Code = normalizedCode;
        Name = name.Trim();
        InternationalName = internationalName.Trim();
        Description = NormalizeOptionalText(description);
        FunctionType = functionType;
        ParentCostCenterId = parentCostCenterId;
        DefaultTaxBranchId = defaultTaxBranchId;
        ResponsiblePositionId = responsiblePositionId;
        OrgUnitId = orgUnitId;
        ValidFrom = utcValidFrom;
        ValidTo = utcValidTo;

        AddCostCenterUpdatedDomainEvent();
    }

    public void Activate()
    {
        EnsureNotDeleted();

        if (IsActive)
        {
            return;
        }

        if (ParentCostCenter is { IsActive: false } or { IsDeleted: true })
        {
            throw new InvalidOperationException(
                "Cannot activate a cost center while its parent cost center is inactive or deleted.");
        }

        IsActive = true;
        AddCostCenterActivatedDomainEvent();
    }

    public void Deactivate()
    {
        EnsureNotDeleted();

        if (!IsActive)
        {
            return;
        }

        if (_children.Any(c => !c.IsDeleted && c.IsActive))
        {
            throw new InvalidOperationException(
                "Cannot deactivate a cost center while active child cost centers still exist.");
        }

        IsActive = false;
        AddCostCenterDeactivatedDomainEvent();
    }

    public void BlockPosting()
    {
        EnsureNotDeleted();

        if (IsPostingBlocked)
        {
            return;
        }

        IsPostingBlocked = true;
        AddCostCenterPostingBlockedDomainEvent();
    }

    public void UnblockPosting()
    {
        EnsureNotDeleted();

        if (!IsPostingBlocked)
        {
            return;
        }

        IsPostingBlocked = false;
        AddCostCenterPostingUnblockedDomainEvent();
    }

    public void Delete(string? deletedBy = null)
    {
        if (IsDeleted)
        {
            return;
        }

        if (_children.Any(c => !c.IsDeleted))
        {
            throw new InvalidOperationException(
                "Cannot delete a cost center while child cost centers still exist.");
        }

        DeletedOnUtc = DateTimeOffset.UtcNow;
        DeletedBy = deletedBy;
        IsDeleted = true;
        IsActive = false;
        IsPostingBlocked = true;

        AddCostCenterDeletedDomainEvent();
    }

    public bool IsEffectiveAt(DateTimeOffset utcTimestamp)
    {
        var target = utcTimestamp.ToUniversalTime();
        if (IsDeleted || !IsActive || IsPostingBlocked)
        {
            return false;
        }

        if (ValidFrom.HasValue && target < ValidFrom.Value)
        {
            return false;
        }

        if (ValidTo.HasValue && target > ValidTo.Value)
        {
            return false;
        }

        return true;
    }

    private void EnsureNotDeleted()
    {
        if (IsDeleted)
        {
            throw new InvalidOperationException($"Cost center '{Id}' has already been deleted.");
        }
    }

    private static string? NormalizeOptionalText(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    #region Domain Events

    private void AddCostCenterCreatedDomainEvent() => AddDomainEvent(DomainEvent.Create((id, ts) =>
        new CostCenterCreatedDomainEvent(
            CostCenterId: Id,
            LegalEntityId: LegalEntityId,
            ParentCostCenterId: ParentCostCenterId,
            DefaultTaxBranchId: DefaultTaxBranchId,
            Code: Code,
            Name: Name,
            InternationalName: InternationalName,
            FunctionType: FunctionType.Key,
            IsActive: IsActive,
            EventId: id,
            OccurredOnUtc: ts)));

    private void AddCostCenterUpdatedDomainEvent() => AddDomainEvent(DomainEvent.Create((id, ts) =>
        new CostCenterUpdatedDomainEvent(
            CostCenterId: Id,
            LegalEntityId: LegalEntityId,
            ParentCostCenterId: ParentCostCenterId,
            DefaultTaxBranchId: DefaultTaxBranchId,
            Code: Code,
            Name: Name,
            InternationalName: InternationalName,
            FunctionType: FunctionType.Key,
            EventId: id,
            OccurredOnUtc: ts)));

    private void AddCostCenterActivatedDomainEvent() => AddDomainEvent(DomainEvent.Create((id, ts) =>
        new CostCenterActivatedDomainEvent(
            CostCenterId: Id,
            LegalEntityId: LegalEntityId,
            Code: Code,
            EventId: id,
            OccurredOnUtc: ts)));

    private void AddCostCenterDeactivatedDomainEvent() => AddDomainEvent(DomainEvent.Create((id, ts) =>
        new CostCenterDeactivatedDomainEvent(
            CostCenterId: Id,
            LegalEntityId: LegalEntityId,
            Code: Code,
            EventId: id,
            OccurredOnUtc: ts)));

    private void AddCostCenterPostingBlockedDomainEvent() => AddDomainEvent(DomainEvent.Create((id, ts) =>
        new CostCenterPostingBlockedDomainEvent(
            CostCenterId: Id,
            LegalEntityId: LegalEntityId,
            Code: Code,
            EventId: id,
            OccurredOnUtc: ts)));

    private void AddCostCenterPostingUnblockedDomainEvent() => AddDomainEvent(DomainEvent.Create((id, ts) =>
        new CostCenterPostingUnblockedDomainEvent(
            CostCenterId: Id,
            LegalEntityId: LegalEntityId,
            Code: Code,
            EventId: id,
            OccurredOnUtc: ts)));

    private void AddCostCenterDeletedDomainEvent() => AddDomainEvent(DomainEvent.Create((id, ts) =>
        new CostCenterDeletedDomainEvent(
            CostCenterId: Id,
            LegalEntityId: LegalEntityId,
            Code: Code,
            EventId: id,
            OccurredOnUtc: ts)));

    #endregion
}