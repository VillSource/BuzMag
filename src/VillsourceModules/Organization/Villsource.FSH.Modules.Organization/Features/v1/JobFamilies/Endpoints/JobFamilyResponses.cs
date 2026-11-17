namespace Villsource.FSH.Modules.Organization.Features.v1.JobFamilies.Endpoints;

internal sealed record JobFamilySummaryResponse(
    string RefId,
    string Code,
    string Name,
    string IntlName,
    string? Description,
    bool IsActive);

internal sealed record JobFamilyDetailResponse(
    string RefId,
    string Code,
    string Name,
    string IntlName,
    string? Description,
    bool IsActive,
    DateTimeOffset CreatedOnUtc,
    DateTimeOffset? LastModifiedOnUtc);

internal sealed record JobFamilyLookupResponse(
    string RefId,
    string Code,
    string Name,
    string IntlName);
