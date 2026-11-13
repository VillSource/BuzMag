using FSH.Framework.Core.Domain;

namespace Villsource.FSH.Modules.Organization.Domain.LegalEntities.Events;

public sealed record TaxBranchAddedDomainEvent(
    Guid LegalEntityId,
    Guid TaxBranchId,
    string BranchCode,
    bool IsHeadOffice,
    string Name,
    string InternationalName,
    Guid EventId,
    DateTimeOffset OccurredOnUtc) : DomainEvent(EventId, OccurredOnUtc);