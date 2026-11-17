using Riok.Mapperly.Abstractions;
using Villsource.FSH.Modules.Organization.Contracts.v1.JobGrades.Dtos;
using Villsource.FSH.Modules.Organization.Domain.JobGrades;


namespace Villsource.FSH.Modules.Organization.Features.v1.JobGrades
{
    [Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
    public static partial class JobGradeMapper
    {
        [MapProperty(nameof(JobGrade.ReferenceId), nameof(JobGradeDetailDto.RefId))]
        [MapPropertyFromSource(nameof(JobGradeDetailDto.JobLevelCategory), Use = nameof(GetJobLevelCategory))]
        [MapPropertyFromSource(nameof(JobGradeDetailDto.CreatedOnUtc), Use = nameof(GetCreatedOnUtc))]
        [MapPropertyFromSource(nameof(JobGradeDetailDto.LastModifiedOnUtc), Use = nameof(GetLastModifiedOnUtc))]
        public static partial JobGradeDetailDto ToDetailDto(this JobGrade entity);

        private static string? GetJobLevelCategory(JobGrade entity) => entity.JobLevelCategory?.Key;
        private static DateTimeOffset GetCreatedOnUtc(JobGrade entity) => entity.CreatedOnUtc;
        private static DateTimeOffset? GetLastModifiedOnUtc(JobGrade entity) => entity.LastModifiedOnUtc;
    }

}