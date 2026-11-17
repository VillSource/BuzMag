using FSH.Framework.Core.Domain;
using Villsource.FSH.Modules.Organization.Domain.LegalEntities;
using Villsource.FSH.Modules.Organization.Domain.PositionDelegations.Events;
using Villsource.FSH.Modules.Organization.Features.v1.Positions;
using Villsource.Tool.UniqueKey;
using Position = Villsource.FSH.Modules.Organization.Domain.Positions.Position;

namespace Villsource.FSH.Modules.Organization.Domain.PositionDelegations;

public sealed partial class PositionDelegation : BaseEntity<Guid>, IAuditableEntity, ISoftDeletable
{
    private PositionDelegation() { }

    public string ReferenceId { get; } = VillsourceId.Key;
    public Guid LegalEntityId { get; private set; }
    public LegalEntity? LegalEntity { get; private set; } = null!;

    public Guid DelegatorPositionId { get; private set; }
    public Position? DelegatorPosition { get; private set; } = null!;

    public Guid DelegatePositionId { get; private set; }
    public Position? DelegatePosition { get; private set; } = null!;

    public DelegationType DelegationType { get; private set; } = null!;
    public string? DocumentTypeScope { get; private set; }
    public string? Reason { get; private set; }

    public DateTimeOffset ValidFrom { get; private set; }
    public DateTimeOffset ValidTo { get; private set; }

    public bool IsRevoked { get; private set; }
    public DateTimeOffset? RevokedOnUtc { get; private set; }
    public string? RevokedBy { get; private set; }
    public string? RevocationReason { get; private set; }

    public static PositionDelegation Create(
        Guid legalEntityId,
        Guid delegatorPositionId,
        Guid delegatePositionId,
        DelegationType delegationType,
        DateTimeOffset validFrom,
        DateTimeOffset validTo,
        string? documentTypeScope = null,
        string? reason = null)
    {
        if (legalEntityId == Guid.Empty) throw new ArgumentException("LegalEntityId cannot be empty.", nameof(legalEntityId));
        if (delegatorPositionId == Guid.Empty) throw new ArgumentException("DelegatorPositionId cannot be empty.", nameof(delegatorPositionId));
        if (delegatePositionId == Guid.Empty) throw new ArgumentException("DelegatePositionId cannot be empty.", nameof(delegatePositionId));
        if (delegatorPositionId == delegatePositionId)
        {
            throw new InvalidOperationException("A position cannot delegate authority to itself.");
        }

        var (utcFrom, utcTo, normalizedScope, normalizedReason) =
            PositionDelegationRules.NormalizeAndValidate(
                delegationType, validFrom, validTo, documentTypeScope, reason);

        var entity = new PositionDelegation
        {
            Id = Guid.CreateVersion7(),
            LegalEntityId = legalEntityId,
            DelegatorPositionId = delegatorPositionId,
            DelegatePositionId = delegatePositionId,
            DelegationType = delegationType,
            DocumentTypeScope = normalizedScope,
            Reason = normalizedReason,
            ValidFrom = utcFrom,
            ValidTo = utcTo,
            IsRevoked = false
        };

        entity.AddDomainEvent(DomainEvent.Create((id, ts) =>
            new PositionDelegationCreatedDomainEvent(
                PositionDelegationId: entity.Id,
                LegalEntityId: entity.LegalEntityId,
                DelegatorPositionId: entity.DelegatorPositionId,
                DelegatePositionId: entity.DelegatePositionId,
                DelegationType: entity.DelegationType.Key,
                ValidFrom: entity.ValidFrom,
                ValidTo: entity.ValidTo,
                EventId: id,
                OccurredOnUtc: ts)));

        return entity;
    }

    /// <summary>
    /// Updates mutable attributes. Returns <c>true</c> if any field changed;
    /// returns <c>false</c> on no-op updates to skip redundant DB writes and events.
    /// </summary>
    public bool Update(
        DelegationType delegationType,
        DateTimeOffset validFrom,
        DateTimeOffset validTo,
        string? documentTypeScope = null,
        string? reason = null)
    {
        EnsureModifiable();

        var (utcFrom, utcTo, normalizedScope, normalizedReason) =
            PositionDelegationRules.NormalizeAndValidate(
                delegationType, validFrom, validTo, documentTypeScope, reason);

        if (DelegationType == delegationType &&
            ValidFrom == utcFrom &&
            ValidTo == utcTo &&
            string.Equals(DocumentTypeScope, normalizedScope, StringComparison.Ordinal) &&
            string.Equals(Reason, normalizedReason, StringComparison.Ordinal))
        {
            return false;
        }

        DelegationType = delegationType;
        DocumentTypeScope = normalizedScope;
        Reason = normalizedReason;
        ValidFrom = utcFrom;
        ValidTo = utcTo;

        AddDomainEvent(DomainEvent.Create((id, ts) =>
            new PositionDelegationUpdatedDomainEvent(
                PositionDelegationId: Id,
                LegalEntityId: LegalEntityId,
                DelegationType: DelegationType.Key,
                ValidFrom: ValidFrom,
                ValidTo: ValidTo,
                EventId: id,
                OccurredOnUtc: ts)));

        return true;
    }

    public void Revoke(string? revokedBy = null, string? revocationReason = null)
    {
        EnsureNotDeleted();
        if (IsRevoked) return;

        IsRevoked = true;
        RevokedOnUtc = DateTimeOffset.UtcNow;
        RevokedBy = string.IsNullOrWhiteSpace(revokedBy) ? null : revokedBy.Trim();
        RevocationReason = string.IsNullOrWhiteSpace(revocationReason) ? null : revocationReason.Trim();

        AddDomainEvent(DomainEvent.Create((id, ts) =>
            new PositionDelegationRevokedDomainEvent(
                PositionDelegationId: Id,
                LegalEntityId: LegalEntityId,
                DelegatorPositionId: DelegatorPositionId,
                DelegatePositionId: DelegatePositionId,
                EventId: id,
                OccurredOnUtc: ts)));
    }

    public void Delete(string? deletedBy = null)
    {
        if (IsDeleted) return;

        DeletedOnUtc = DateTimeOffset.UtcNow;
        DeletedBy = deletedBy;
        IsDeleted = true;

        AddDomainEvent(DomainEvent.Create((id, ts) =>
            new PositionDelegationDeletedDomainEvent(
                PositionDelegationId: Id,
                LegalEntityId: LegalEntityId,
                EventId: id,
                OccurredOnUtc: ts)));
    }

    public bool IsEffectiveAt(DateTimeOffset utcTimestamp, string? documentTypeCode = null)
    {
        var target = utcTimestamp.ToUniversalTime();
        if (IsDeleted || IsRevoked || target < ValidFrom || target > ValidTo)
        {
            return false;
        }

        if (!DelegationType.RequiresDocumentScope || string.IsNullOrWhiteSpace(documentTypeCode))
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(DocumentTypeScope))
        {
            return false;
        }

        var requestedDoc = documentTypeCode.Trim().ToUpperInvariant();
        return DocumentTypeScope
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Contains(requestedDoc, StringComparer.OrdinalIgnoreCase);
    }

    private void EnsureNotDeleted()
    {
        if (IsDeleted)
        {
            throw new InvalidOperationException($"Position delegation '{Id}' has already been deleted.");
        }
    }

    private void EnsureModifiable()
    {
        EnsureNotDeleted();
        if (IsRevoked)
        {
            throw new InvalidOperationException($"Position delegation '{Id}' has been revoked and cannot be modified.");
        }
    }
}