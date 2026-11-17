namespace Villsource.FSH.Modules.Organization.Features.v1.JobGrades.Endpoints;

internal sealed record JobGradeSummaryResponse(
    string RefId,
    string Code,
    string Name,
    string IntlName,
    int RankOrder,
    string? Description,
    bool IsActive);

internal sealed record JobGradeDetailResponse(
    string RefId,
    string Code,
    string Name,
    string IntlName,
    int RankOrder,
    string? Category,
    string? Description,
    bool IsActive,
    DateTimeOffset CreatedOnUtc,
    DateTimeOffset? LastModifiedOnUtc);

internal sealed record JobGradeLookupResponse(
    string RefId,
    string Code,
    string Name,
    string IntlName,
    int RankOrder);

internal sealed record JobLevelCategoryResponse(
    string Key,
    string DisplayName,
    int SortOrder);
