using FSH.Framework.Core.Domain;

namespace Villsource.FSH.Modules.Organization.Domain.LegalEntities.Events;

public sealed record TaxBranchClosedDomainEvent(
    Guid LegalEntityId,
    Guid TaxBranchId,
    string BranchCode,
    DateTimeOffset ClosedDate,
    Guid EventId,
    DateTimeOffset OccurredOnUtc) : DomainEvent(EventId, OccurredOnUtc);