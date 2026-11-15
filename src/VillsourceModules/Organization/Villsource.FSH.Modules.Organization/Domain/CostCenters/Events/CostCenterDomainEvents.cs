using FSH.Framework.Core.Domain;

namespace Villsource.FSH.Modules.Organization.Domain.CostCenters.Events;

public sealed record CostCenterCreatedDomainEvent(
    Guid CostCenterId,
    Guid LegalEntityId,
    Guid? ParentCostCenterId,
    Guid? DefaultTaxBranchId,
    string Code,
    string Name,
    string InternationalName,
    string FunctionType,
    bool IsActive,
    Guid EventId,
    DateTimeOffset OccurredOnUtc) : DomainEvent(EventId, OccurredOnUtc);

public sealed record CostCenterUpdatedDomainEvent(
    Guid CostCenterId,
    Guid LegalEntityId,
    Guid? ParentCostCenterId,
    Guid? DefaultTaxBranchId,
    string Code,
    string Name,
    string InternationalName,
    string FunctionType,
    Guid EventId,
    DateTimeOffset OccurredOnUtc) : DomainEvent(EventId, OccurredOnUtc);

public sealed record CostCenterActivatedDomainEvent(
    Guid CostCenterId,
    Guid LegalEntityId,
    string Code,
    Guid EventId,
    DateTimeOffset OccurredOnUtc) : DomainEvent(EventId, OccurredOnUtc);

public sealed record CostCenterDeactivatedDomainEvent(
    Guid CostCenterId,
    Guid LegalEntityId,
    string Code,
    Guid EventId,
    DateTimeOffset OccurredOnUtc) : DomainEvent(EventId, OccurredOnUtc);

public sealed record CostCenterPostingBlockedDomainEvent(
    Guid CostCenterId,
    Guid LegalEntityId,
    string Code,
    Guid EventId,
    DateTimeOffset OccurredOnUtc) : DomainEvent(EventId, OccurredOnUtc);

public sealed record CostCenterPostingUnblockedDomainEvent(
    Guid CostCenterId,
    Guid LegalEntityId,
    string Code,
    Guid EventId,
    DateTimeOffset OccurredOnUtc) : DomainEvent(EventId, OccurredOnUtc);

public sealed record CostCenterDeletedDomainEvent(
    Guid CostCenterId,
    Guid LegalEntityId,
    string Code,
    Guid EventId,
    DateTimeOffset OccurredOnUtc) : DomainEvent(EventId, OccurredOnUtc);