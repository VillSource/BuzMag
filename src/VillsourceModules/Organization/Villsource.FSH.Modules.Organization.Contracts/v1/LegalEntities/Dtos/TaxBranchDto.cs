using Villsource.ObjectValue;

namespace Villsource.FSH.Modules.Organization.Contracts.v1.LegalEntities.Dtos;

public sealed record TaxBranchDto(
    string RefId,
    string LegalEntityRefId,
    string BranchCode,
    bool IsHeadOffice,
    string Name,
    string InternationalName,
    string? TaxSubId,
    Address RegisteredAddress,
    bool IsActive,
    DateTimeOffset? RegisteredDate,
    DateTimeOffset? ClosedDate);