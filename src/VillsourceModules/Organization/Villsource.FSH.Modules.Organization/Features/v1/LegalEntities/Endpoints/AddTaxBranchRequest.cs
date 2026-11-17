using Villsource.ObjectValue;

namespace Villsource.FSH.Modules.Organization.Features.v1.LegalEntities;

internal sealed record AddTaxBranchRequest(
    string BranchCode,
    string Name,
    string InternationalName,
    Address RegisteredAddress,
    string? TaxSubId = null,
    DateTimeOffset? RegisteredDate = null,
    bool IsActive = true);