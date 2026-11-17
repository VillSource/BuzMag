namespace Villsource.FSH.Modules.Organization.Features.v1.OrganizationUnitTypes.Endpoints;

internal sealed record OrganizationUnitTypeSummaryResponse(
    string RefId,
    string Code,
    string Name,
    string IntlName,
    int Level,
    string? Description,
    bool IsActive);

internal sealed record OrganizationUnitTypeDetailResponse(
    string RefId,
    string Code,
    string Name,
    string IntlName,
    int Level,
    string? Description,
    bool IsActive,
    DateTimeOffset CreatedOnUtc,
    DateTimeOffset? LastModifiedOnUtc);

internal sealed record OrganizationUnitTypeLookupResponse(
    string RefId,
    string Code,
    string Name,
    string IntlName,
    int Level);
