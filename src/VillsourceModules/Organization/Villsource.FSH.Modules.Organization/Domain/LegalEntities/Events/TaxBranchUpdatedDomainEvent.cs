using FSH.Framework.Core.Domain;

namespace Villsource.FSH.Modules.Organization.Domain.LegalEntities.Events;

public sealed record TaxBranchUpdatedDomainEvent(
    Guid LegalEntityId,
    Guid TaxBranchId,
    string BranchCode,
    string Name,
    string InternationalName,
    Guid EventId,
    DateTimeOffset OccurredOnUtc) : DomainEvent(EventId, OccurredOnUtc);