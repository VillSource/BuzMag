using Riok.Mapperly.Abstractions;
using Villsource.FSH.Modules.Organization.Contracts.v1.Jobs.Dtos;
using Villsource.FSH.Modules.Organization.Domain.JobGrades;
using Villsource.FSH.Modules.Organization.Domain.Jobs;

namespace Villsource.FSH.Modules.Organization.Features.v1.Jobs;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public static partial class JobMapper
{
    [MapProperty(nameof(Job.ReferenceId), nameof(JobDetailDto.RefId))]
    [MapPropertyFromSource(nameof(JobDetailDto.CreatedOnUtc), Use = nameof(GetCreatedOnUtc))]
    [MapPropertyFromSource(nameof(JobDetailDto.LastModifiedOnUtc), Use = nameof(GetLastModifiedOnUtc))]
    public static partial JobDetailDto ToDetailDto(this Job entity);

    [MapProperty(nameof(JobGrade.ReferenceId), nameof(JobGradeRefDto.RefId))]
    public static partial JobGradeRefDto ToRefDto(this JobGrade grade);

    private static DateTimeOffset GetCreatedOnUtc(Job entity) => entity.CreatedOnUtc;
    private static DateTimeOffset? GetLastModifiedOnUtc(Job entity) => entity.LastModifiedOnUtc;
}