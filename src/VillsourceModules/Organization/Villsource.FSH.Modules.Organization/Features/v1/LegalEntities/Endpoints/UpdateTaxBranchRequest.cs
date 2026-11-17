using Villsource.ObjectValue;

namespace Villsource.FSH.Modules.Organization.Features.v1.LegalEntities;

internal sealed record UpdateTaxBranchRequest(
    string Name,
    string InternationalName,
    Address RegisteredAddress,
    string? TaxSubId = null,
    DateTimeOffset? RegisteredDate = null);