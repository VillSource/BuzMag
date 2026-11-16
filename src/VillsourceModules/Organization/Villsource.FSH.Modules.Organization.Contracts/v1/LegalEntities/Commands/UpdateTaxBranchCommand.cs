using Mediator;
using Villsource.ObjectValue;

namespace Villsource.FSH.Modules.Organization.Contracts.v1.LegalEntities.Commands;

public sealed record UpdateTaxBranchCommand(
    string LegalEntityRefId,
    string RefId,
    string Name,
    string InternationalName,
    Address RegisteredAddress,
    string? TaxSubId = null,
    DateTimeOffset? RegisteredDate = null) : IRequest<string>;