using FSH.Framework.Shared.Persistence;
using Riok.Mapperly.Abstractions;
using Villsource.FSH.Modules.Organization.Contracts.v1.JobFamilies.Commands;
using Villsource.FSH.Modules.Organization.Contracts.v1.JobFamilies.Dtos;
using Villsource.FSH.Modules.Organization.Contracts.v1.JobFamilies.Queries;

namespace Villsource.FSH.Modules.Organization.Features.v1.JobFamilies.Endpoints;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
internal static partial class JobFamilyEndpointMapper
{
    [MapProperty(nameof(CreateJobFamilyRequest.IntlName), nameof(CreateJobFamilyCommand.InternationalName))]
    internal static partial CreateJobFamilyCommand ToCommand(this CreateJobFamilyRequest request);

    [MapProperty(nameof(UpdateJobFamilyRequest.IntlName), nameof(UpdateJobFamilyCommand.InternationalName))]
    internal static partial UpdateJobFamilyCommand ToCommand(this UpdateJobFamilyRequest request, string refId);

    internal static partial SearchJobFamiliesQuery ToQuery(this SearchJobFamiliesRequest request);

    [MapProperty(nameof(JobFamilySummaryDto.InternationalName), nameof(JobFamilySummaryResponse.IntlName))]
    internal static partial JobFamilySummaryResponse ToResponse(this JobFamilySummaryDto dto);

    internal static partial PagedResponse<JobFamilySummaryResponse> ToResponse(this PagedResponse<JobFamilySummaryDto> page);

    [MapProperty(nameof(JobFamilyDetailDto.InternationalName), nameof(JobFamilyDetailResponse.IntlName))]
    internal static partial JobFamilyDetailResponse ToResponse(this JobFamilyDetailDto dto);

    [MapProperty(nameof(JobFamilyLookupDto.InternationalName), nameof(JobFamilyLookupResponse.IntlName))]
    internal static partial JobFamilyLookupResponse ToResponse(this JobFamilyLookupDto dto);

    internal static partial IReadOnlyList<JobFamilyLookupResponse> ToResponse(this IReadOnlyList<JobFamilyLookupDto> dtos);
}
