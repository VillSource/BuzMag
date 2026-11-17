using Villsource.FSH.Modules.Organization.Features.v1.JobFamilies.Endpoints;

namespace Villsource.FSH.Modules.Organization.Features.v1.Jobs.Endpoints;

internal sealed record JobSummaryResponse(
    string RefId,
    string Code,
    string Name,
    string IntlName,
    string? FamilyRefId,
    string? FamilyCode,
    string? FamilyName,
    string JobGradeRefId,
    string JobGradeCode,
    int JobGradeRankOrder,
    bool IsActive);

internal sealed record JobDetailResponse(
    string RefId,
    string Code,
    string Name,
    string IntlName,
    JobFamilyLookupResponse? Family,
    string? Description,
    JobGradeRefResponse JobGrade,
    bool IsActive,
    DateTimeOffset CreatedOnUtc,
    DateTimeOffset? LastModifiedOnUtc);

internal sealed record JobLookupResponse(
    string RefId,
    string Code,
    string Name,
    string IntlName,
    string? Family,
    string JobGradeRefId,
    string JobGradeCode,
    int JobGradeRankOrder);

internal sealed record JobGradeRefResponse(
    string RefId,
    string Code,
    string Name,
    int RankOrder);
