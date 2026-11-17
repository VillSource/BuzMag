using FSH.Framework.Shared.Persistence;
using Riok.Mapperly.Abstractions;
using Villsource.FSH.Modules.Organization.Contracts.v1.JobFamilies.Dtos;
using Villsource.FSH.Modules.Organization.Contracts.v1.Jobs.Commands;
using Villsource.FSH.Modules.Organization.Contracts.v1.Jobs.Dtos;
using Villsource.FSH.Modules.Organization.Contracts.v1.Jobs.Queries;
using Villsource.FSH.Modules.Organization.Features.v1.JobFamilies.Endpoints;

namespace Villsource.FSH.Modules.Organization.Features.v1.Jobs.Endpoints;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
internal static partial class JobEndpointMapper
{
    [MapProperty(nameof(CreateJobRequest.IntlName), nameof(CreateJobCommand.InternationalName))]
    [MapProperty(nameof(CreateJobRequest.JobGradeRefId), nameof(CreateJobCommand.DefaultJobGradeRefId))]
    [MapProperty(nameof(CreateJobRequest.FamilyRefId), nameof(CreateJobCommand.JobFamilyRefId))]
    internal static partial CreateJobCommand ToCommand(this CreateJobRequest request);

    [MapProperty(nameof(UpdateJobRequest.IntlName), nameof(UpdateJobCommand.InternationalName))]
    [MapProperty(nameof(UpdateJobRequest.JobGradeRefId), nameof(UpdateJobCommand.DefaultJobGradeRefId))]
    [MapProperty(nameof(UpdateJobRequest.FamilyRefId), nameof(UpdateJobCommand.JobFamilyRefId))]
    internal static partial UpdateJobCommand ToCommand(this UpdateJobRequest request, string refId);

    [MapProperty(nameof(SearchJobsRequest.FamilyRefId), nameof(SearchJobsQuery.JobFamilyRefId))]
    [MapProperty(nameof(SearchJobsRequest.JobGradeRefId), nameof(SearchJobsQuery.DefaultJobGradeRefId))]
    internal static partial SearchJobsQuery ToQuery(this SearchJobsRequest request);

    [MapProperty(nameof(GetJobLookupsRequest.FamilyRefId), nameof(GetJobLookupsQuery.JobFamilyRefId))]
    internal static partial GetJobLookupsQuery ToQuery(this GetJobLookupsRequest request);

    [MapProperty(nameof(JobSummaryDto.InternationalName), nameof(JobSummaryResponse.IntlName))]
    [MapProperty(nameof(JobSummaryDto.JobFamilyRefId), nameof(JobSummaryResponse.FamilyRefId))]
    [MapProperty(nameof(JobSummaryDto.JobFamilyCode), nameof(JobSummaryResponse.FamilyCode))]
    [MapProperty(nameof(JobSummaryDto.JobFamilyName), nameof(JobSummaryResponse.FamilyName))]
    [MapProperty(nameof(JobSummaryDto.DefaultJobGradeRefId), nameof(JobSummaryResponse.JobGradeRefId))]
    [MapProperty(nameof(JobSummaryDto.DefaultJobGradeCode), nameof(JobSummaryResponse.JobGradeCode))]
    [MapProperty(nameof(JobSummaryDto.DefaultJobGradeRankOrder), nameof(JobSummaryResponse.JobGradeRankOrder))]
    internal static partial JobSummaryResponse ToResponse(this JobSummaryDto dto);

    internal static partial PagedResponse<JobSummaryResponse> ToResponse(this PagedResponse<JobSummaryDto> page);

    [MapProperty(nameof(JobDetailDto.InternationalName), nameof(JobDetailResponse.IntlName))]
    [MapProperty(nameof(JobDetailDto.JobFamily), nameof(JobDetailResponse.Family))]
    [MapProperty(nameof(JobDetailDto.DefaultJobGrade), nameof(JobDetailResponse.JobGrade))]
    internal static partial JobDetailResponse ToResponse(this JobDetailDto dto);

    [MapProperty(nameof(JobLookupDto.InternationalName), nameof(JobLookupResponse.IntlName))]
    [MapProperty(nameof(JobLookupDto.JobFamily), nameof(JobLookupResponse.Family))]
    [MapProperty(nameof(JobLookupDto.DefaultJobGradeRefId), nameof(JobLookupResponse.JobGradeRefId))]
    [MapProperty(nameof(JobLookupDto.DefaultJobGradeCode), nameof(JobLookupResponse.JobGradeCode))]
    [MapProperty(nameof(JobLookupDto.DefaultJobGradeRankOrder), nameof(JobLookupResponse.JobGradeRankOrder))]
    internal static partial JobLookupResponse ToResponse(this JobLookupDto dto);

    internal static partial IReadOnlyList<JobLookupResponse> ToResponse(this IReadOnlyList<JobLookupDto> dtos);

    internal static partial JobGradeRefResponse ToResponse(this JobGradeRefDto dto);

    [MapProperty(nameof(JobFamilyLookupDto.InternationalName), nameof(JobFamilyLookupResponse.IntlName))]
    private static partial JobFamilyLookupResponse MapJobFamily(JobFamilyLookupDto dto);
}
