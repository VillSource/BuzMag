using Mediator;
using Villsource.ObjectValue;

namespace Villsource.FSH.Modules.Organization.Contracts.v1.LegalEntities.Commands;

public sealed record AddTaxBranchCommand(
    string LegalEntityRefId,
    string BranchCode,
    string Name,
    string InternationalName,
    Address RegisteredAddress,
    bool IsHeadOffice = false,
    string? TaxSubId = null,
    DateTimeOffset? RegisteredDate = null,
    bool IsActive = true) : IRequest<string>;