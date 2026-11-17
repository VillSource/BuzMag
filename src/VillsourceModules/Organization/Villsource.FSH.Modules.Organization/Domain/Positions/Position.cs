using FSH.Framework.Core.Domain;
using Villsource.FSH.Modules.Organization.Domain.CostCenters;
using Villsource.FSH.Modules.Organization.Domain.JobGrades;
using Villsource.FSH.Modules.Organization.Domain.Jobs;
using Villsource.FSH.Modules.Organization.Domain.LegalEntities;
using Villsource.FSH.Modules.Organization.Domain.OrganizationUnits;
using Villsource.FSH.Modules.Organization.Domain.Positions.Events;
using Villsource.FSH.Modules.Organization.Features.v1.Positions;
using Villsource.Tool.UniqueKey;

namespace Villsource.FSH.Modules.Organization.Domain.Positions;

public sealed partial class Position : BaseEntity<Guid>, IAuditableEntity, ISoftDeletable
{
    private readonly List<Position> _directReports = [];

    private Position() { }

    // 1. Identity & Organizational Placement
    public string ReferenceId { get; } = VillsourceId.Key;
    public Guid LegalEntityId { get; private set; }
    public LegalEntity? LegalEntity { get; private set; } = null!;

    public Guid OrgUnitId { get; private set; }
    public OrganizationUnit? OrgUnit { get; } = null!;

    public Guid JobId { get; private set; }
    public Job? Job { get; private set; } = null!;

    public Guid JobGradeId { get; private set; }
    public JobGrade? JobGrade { get; private set; } = null!;

    public Guid? CostCenterId { get; private set; }
    public CostCenter? CostCenter { get; private set; } = null!;

    // 2. Reporting Lines (Chain of Command)
    public Guid? ReportsToPositionId { get; private set; }
    public Position? ReportsToPosition { get; private set; } = null!;
    public IReadOnlyCollection<Position> DirectReports => _directReports.AsReadOnly();

    public Guid? DottedLinePositionId { get; private set; }
    public Position? DottedLinePosition { get; private set; } = null!;

    // 3. Position Attributes & Headcount
    public string Code { get; private set; } = null!;
    public string Title { get; private set; } = null!;
    public string InternationalTitle { get; private set; } = null!;
    public string? Description { get; private set; }
    public Guid? WorkLocationTaxBranchId { get; private set; }
    public TaxBranch? WorkLocationTaxBranch { get; } = null!;
    public decimal? ApprovalLimitAmount { get; private set; }
    public string? ApprovalLimitCurrency { get; private set; }
    public int HeadcountCapacity { get; private set; } = 1;
    public bool IsPrimaryLeadership { get; private set; }

    // 4. Validity & Lifecycle
    public DateTimeOffset? ValidFrom { get; private set; }
    public DateTimeOffset? ValidTo { get; private set; }
    public bool IsActive { get; private set; } = true;

    public static Position Create(
        Guid legalEntityId,
        Guid orgUnitId,
        Guid jobId,
        Guid jobGradeId,
        string code,
        string title,
        string internationalTitle,
        string? description = null,
        Guid? costCenterId = null,
        Guid? workLocationTaxBranchId = null,
        decimal? approvalLimitAmount = null,
        string? approvalLimitCurrency = null,
        Guid? reportsToPositionId = null,
        Guid? dottedLinePositionId = null,
        int headcountCapacity = 1,
        bool isPrimaryLeadership = false,
        DateTimeOffset? validFrom = null,
        DateTimeOffset? validTo = null,
        bool isActive = true)
    {
        if (legalEntityId == Guid.Empty) throw new ArgumentException("LegalEntityId cannot be empty.", nameof(legalEntityId));
        if (orgUnitId == Guid.Empty) throw new ArgumentException("OrgUnitId cannot be empty.", nameof(orgUnitId));
        if (jobId == Guid.Empty) throw new ArgumentException("JobId cannot be empty.", nameof(jobId));
        if (jobGradeId == Guid.Empty) throw new ArgumentException("JobGradeId cannot be empty.", nameof(jobGradeId));

        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(internationalTitle);

        if (headcountCapacity < PositionRules.MinHeadcountCapacity)
        {
            throw new ArgumentOutOfRangeException(
                nameof(headcountCapacity),
                $"HeadcountCapacity must be at least {PositionRules.MinHeadcountCapacity}.");
        }

        if (approvalLimitAmount.HasValue && approvalLimitAmount.Value < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(approvalLimitAmount), "ApprovalLimitAmount cannot be negative.");
        }

        var normalizedCurrency = string.IsNullOrWhiteSpace(approvalLimitCurrency)
            ? null
            : approvalLimitCurrency.Trim().ToUpperInvariant();
        if (normalizedCurrency is not null && normalizedCurrency.Length != PositionRules.MaxCurrencyLength)
        {
            throw new ArgumentException("ApprovalLimitCurrency must be a 3-letter currency code.", nameof(approvalLimitCurrency));
        }

        var id = Guid.CreateVersion7();
        PositionRules.ValidateReportingTargets(id, reportsToPositionId, dottedLinePositionId);

        var normalizedCode = PositionRules.NormalizeAndValidateCode(code);
        var (utcFrom, utcTo) = PositionRules.NormalizeAndValidateValidity(validFrom, validTo);

        var entity = new Position
        {
            Id = id,
            LegalEntityId = legalEntityId,
            OrgUnitId = orgUnitId,
            JobId = jobId,
            JobGradeId = jobGradeId,
            CostCenterId = costCenterId,
            WorkLocationTaxBranchId = workLocationTaxBranchId,
            ReportsToPositionId = reportsToPositionId,
            DottedLinePositionId = dottedLinePositionId,
            Code = normalizedCode,
            Title = title.Trim(),
            InternationalTitle = internationalTitle.Trim(),
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            ApprovalLimitAmount = approvalLimitAmount,
            ApprovalLimitCurrency = normalizedCurrency,
            HeadcountCapacity = headcountCapacity,
            IsPrimaryLeadership = isPrimaryLeadership,
            ValidFrom = utcFrom,
            ValidTo = utcTo,
            IsActive = isActive
        };

        entity.AddDomainEvent(DomainEvent.Create((eventId, ts) =>
            new PositionCreatedDomainEvent(
                PositionId: entity.Id,
                LegalEntityId: entity.LegalEntityId,
                OrgUnitId: entity.OrgUnitId,
                JobId: entity.JobId,
                JobGradeId: entity.JobGradeId,
                ReportsToPositionId: entity.ReportsToPositionId,
                Code: entity.Code,
                Title: entity.Title,
                IsPrimaryLeadership: entity.IsPrimaryLeadership,
                EventId: eventId,
                OccurredOnUtc: ts)));

        return entity;
    }

    /// <summary>
    /// Updates mutable attributes. Returns <c>true</c> if any property changed;
    /// returns <c>false</c> when identical to avoid redundant DB updates and domain events.
    /// </summary>
    public bool Update(
        Guid orgUnitId,
        Guid jobId,
        Guid jobGradeId,
        string code,
        string title,
        string internationalTitle,
        string? description = null,
        Guid? costCenterId = null,
        Guid? workLocationTaxBranchId = null,
        decimal? approvalLimitAmount = null,
        string? approvalLimitCurrency = null,
        Guid? reportsToPositionId = null,
        Guid? dottedLinePositionId = null,
        int headcountCapacity = 1,
        bool isPrimaryLeadership = false,
        DateTimeOffset? validFrom = null,
        DateTimeOffset? validTo = null)
    {
        EnsureNotDeleted();

        if (orgUnitId == Guid.Empty) throw new ArgumentException("OrgUnitId cannot be empty.", nameof(orgUnitId));
        if (jobId == Guid.Empty) throw new ArgumentException("JobId cannot be empty.", nameof(jobId));
        if (jobGradeId == Guid.Empty) throw new ArgumentException("JobGradeId cannot be empty.", nameof(jobGradeId));

        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(internationalTitle);

        if (headcountCapacity < PositionRules.MinHeadcountCapacity)
        {
            throw new ArgumentOutOfRangeException(
                nameof(headcountCapacity),
                $"HeadcountCapacity must be at least {PositionRules.MinHeadcountCapacity}.");
        }

        if (approvalLimitAmount.HasValue && approvalLimitAmount.Value < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(approvalLimitAmount), "ApprovalLimitAmount cannot be negative.");
        }

        var normalizedCurrency = string.IsNullOrWhiteSpace(approvalLimitCurrency)
            ? null
            : approvalLimitCurrency.Trim().ToUpperInvariant();
        if (normalizedCurrency is not null && normalizedCurrency.Length != PositionRules.MaxCurrencyLength)
        {
            throw new ArgumentException("ApprovalLimitCurrency must be a 3-letter currency code.", nameof(approvalLimitCurrency));
        }

        PositionRules.ValidateReportingTargets(Id, reportsToPositionId, dottedLinePositionId);

        var normalizedCode = PositionRules.NormalizeAndValidateCode(code);
        var normalizedTitle = title.Trim();
        var normalizedIntlTitle = internationalTitle.Trim();
        var normalizedDesc = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        var (utcFrom, utcTo) = PositionRules.NormalizeAndValidateValidity(validFrom, validTo);

        if (OrgUnitId == orgUnitId &&
            JobId == jobId &&
            JobGradeId == jobGradeId &&
            CostCenterId == costCenterId &&
            WorkLocationTaxBranchId == workLocationTaxBranchId &&
            ApprovalLimitAmount == approvalLimitAmount &&
            string.Equals(ApprovalLimitCurrency, normalizedCurrency, StringComparison.Ordinal) &&
            ReportsToPositionId == reportsToPositionId &&
            DottedLinePositionId == dottedLinePositionId &&
            string.Equals(Code, normalizedCode, StringComparison.Ordinal) &&
            string.Equals(Title, normalizedTitle, StringComparison.Ordinal) &&
            string.Equals(InternationalTitle, normalizedIntlTitle, StringComparison.Ordinal) &&
            string.Equals(Description, normalizedDesc, StringComparison.Ordinal) &&
            HeadcountCapacity == headcountCapacity &&
            IsPrimaryLeadership == isPrimaryLeadership &&
            ValidFrom == utcFrom &&
            ValidTo == utcTo)
        {
            return false;
        }

        OrgUnitId = orgUnitId;
        JobId = jobId;
        JobGradeId = jobGradeId;
        CostCenterId = costCenterId;
        WorkLocationTaxBranchId = workLocationTaxBranchId;
        ApprovalLimitAmount = approvalLimitAmount;
        ApprovalLimitCurrency = normalizedCurrency;
        ReportsToPositionId = reportsToPositionId;
        DottedLinePositionId = dottedLinePositionId;
        Code = normalizedCode;
        Title = normalizedTitle;
        InternationalTitle = normalizedIntlTitle;
        Description = normalizedDesc;
        HeadcountCapacity = headcountCapacity;
        IsPrimaryLeadership = isPrimaryLeadership;
        ValidFrom = utcFrom;
        ValidTo = utcTo;

        AddDomainEvent(DomainEvent.Create((eventId, ts) =>
            new PositionUpdatedDomainEvent(
                PositionId: Id,
                LegalEntityId: LegalEntityId,
                OrgUnitId: OrgUnitId,
                JobId: JobId,
                JobGradeId: JobGradeId,
                ReportsToPositionId: ReportsToPositionId,
                Code: Code,
                Title: Title,
                IsPrimaryLeadership: IsPrimaryLeadership,
                EventId: eventId,
                OccurredOnUtc: ts)));

        return true;
    }

    public void Activate()
    {
        EnsureNotDeleted();
        if (IsActive) return;

        if (ReportsToPosition is { IsActive: false } or { IsDeleted: true })
        {
            throw new InvalidOperationException(
                "Cannot activate a position while its reporting manager position is inactive or deleted.");
        }

        IsActive = true;
        AddDomainEvent(DomainEvent.Create((id, ts) =>
            new PositionActivatedDomainEvent(PositionId: Id, LegalEntityId: LegalEntityId, Code: Code, EventId: id, OccurredOnUtc: ts)));
    }

    public void Deactivate()
    {
        EnsureNotDeleted();
        if (!IsActive) return;

        if (_directReports.Any(p => !p.IsDeleted && p.IsActive))
        {
            throw new InvalidOperationException(
                "Cannot deactivate a position while active direct report positions still exist.");
        }

        IsActive = false;
        AddDomainEvent(DomainEvent.Create((id, ts) =>
            new PositionDeactivatedDomainEvent(PositionId: Id, LegalEntityId: LegalEntityId, Code: Code, EventId: id, OccurredOnUtc: ts)));
    }

    public void Delete(string? deletedBy = null)
    {
        if (IsDeleted) return;

        if (_directReports.Any(p => !p.IsDeleted))
        {
            throw new InvalidOperationException(
                "Cannot delete a position while direct report positions still exist.");
        }

        DeletedOnUtc = DateTimeOffset.UtcNow;
        DeletedBy = deletedBy;
        IsDeleted = true;
        IsActive = false;

        AddDomainEvent(DomainEvent.Create((id, ts) =>
            new PositionDeletedDomainEvent(PositionId: Id, LegalEntityId: LegalEntityId, Code: Code, EventId: id, OccurredOnUtc: ts)));
    }

    private void EnsureNotDeleted()
    {
        if (IsDeleted)
        {
            throw new InvalidOperationException($"Position '{Id}' has already been deleted.");
        }
    }
}