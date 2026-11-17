using Riok.Mapperly.Abstractions;
using Villsource.FSH.Modules.Organization.Contracts.v1.JobFamilies.Dtos;
using Villsource.FSH.Modules.Organization.Domain.JobFamilies;

namespace Villsource.FSH.Modules.Organization.Features.v1.JobFamilies;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public static partial class JobFamilyMapper
{
    [MapProperty(nameof(JobFamily.ReferenceId), nameof(JobFamilyDetailDto.RefId))]
    [MapPropertyFromSource(nameof(JobFamilyDetailDto.CreatedOnUtc), Use = nameof(GetCreatedOnUtc))]
    [MapPropertyFromSource(nameof(JobFamilyDetailDto.LastModifiedOnUtc), Use = nameof(GetLastModifiedOnUtc))]
    public static partial JobFamilyDetailDto ToDetailDto(this JobFamily entity);

    private static DateTimeOffset GetCreatedOnUtc(JobFamily entity) => entity.CreatedOnUtc;
    private static DateTimeOffset? GetLastModifiedOnUtc(JobFamily entity) => entity.LastModifiedOnUtc;
}
