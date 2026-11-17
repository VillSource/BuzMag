using Villsource.ObjectValue;

namespace Villsource.FSH.Modules.Organization.Features.v1.LegalEntities.Endpoints;

internal sealed record AddTaxBranchRequest(
    string BranchCode,
    string Name,
    string IntlName,
    Address Address,
    string? TaxSubId = null,
    DateTimeOffset? RegisteredDate = null,
    bool IsActive = true);
