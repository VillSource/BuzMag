using FSH.Framework.Shared.Persistence;
using Riok.Mapperly.Abstractions;
using Villsource.FSH.Modules.Organization.Contracts.v1.JobGrades.Commands;
using Villsource.FSH.Modules.Organization.Contracts.v1.JobGrades.Dtos;
using Villsource.FSH.Modules.Organization.Contracts.v1.JobGrades.Queries;

namespace Villsource.FSH.Modules.Organization.Features.v1.JobGrades.Endpoints;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
internal static partial class JobGradeEndpointMapper
{
    [MapProperty(nameof(CreateJobGradeRequest.IntlName), nameof(CreateJobGradeCommand.InternationalName))]
    [MapProperty(nameof(CreateJobGradeRequest.Category), nameof(CreateJobGradeCommand.JobLevelCategory))]
    internal static partial CreateJobGradeCommand ToCommand(this CreateJobGradeRequest request);

    [MapProperty(nameof(UpdateJobGradeRequest.IntlName), nameof(UpdateJobGradeCommand.InternationalName))]
    [MapProperty(nameof(UpdateJobGradeRequest.Category), nameof(UpdateJobGradeCommand.JobLevelCategory))]
    internal static partial UpdateJobGradeCommand ToCommand(this UpdateJobGradeRequest request, string refId);

    [MapProperty(nameof(SearchJobGradesRequest.MinRank), nameof(SearchJobGradesQuery.MinRankOrder))]
    [MapProperty(nameof(SearchJobGradesRequest.MaxRank), nameof(SearchJobGradesQuery.MaxRankOrder))]
    internal static partial SearchJobGradesQuery ToQuery(this SearchJobGradesRequest request);

    [MapProperty(nameof(JobGradeSummaryDto.InternationalName), nameof(JobGradeSummaryResponse.IntlName))]
    internal static partial JobGradeSummaryResponse ToResponse(this JobGradeSummaryDto dto);

    internal static partial PagedResponse<JobGradeSummaryResponse> ToResponse(this PagedResponse<JobGradeSummaryDto> page);

    [MapProperty(nameof(JobGradeDetailDto.InternationalName), nameof(JobGradeDetailResponse.IntlName))]
    [MapProperty(nameof(JobGradeDetailDto.JobLevelCategory), nameof(JobGradeDetailResponse.Category))]
    internal static partial JobGradeDetailResponse ToResponse(this JobGradeDetailDto dto);

    [MapProperty(nameof(JobGradeLookupDto.InternationalName), nameof(JobGradeLookupResponse.IntlName))]
    internal static partial JobGradeLookupResponse ToResponse(this JobGradeLookupDto dto);

    internal static partial IReadOnlyList<JobGradeLookupResponse> ToResponse(this IReadOnlyList<JobGradeLookupDto> dtos);

    internal static partial JobLevelCategoryResponse ToResponse(this JobLevelCategoryDto dto);

    internal static partial IReadOnlyList<JobLevelCategoryResponse> ToResponse(this IReadOnlyList<JobLevelCategoryDto> dtos);
}
