using FSH.Framework.Core.Domain;

namespace Villsource.FSH.Modules.Organization.Domain.LegalEntities.Events;

public sealed record TaxBranchDeletedDomainEvent(
    Guid LegalEntityId,
    Guid TaxBranchId,
    string BranchCode,
    Guid EventId,
    DateTimeOffset OccurredOnUtc) : DomainEvent(EventId, OccurredOnUtc);