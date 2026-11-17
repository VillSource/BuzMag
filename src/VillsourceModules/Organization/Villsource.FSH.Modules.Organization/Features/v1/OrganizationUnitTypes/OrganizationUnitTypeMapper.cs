using Riok.Mapperly.Abstractions;
using Villsource.FSH.Modules.Organization.Contracts.v1.OrganizationUnitTypes.Dtos;
using Villsource.FSH.Modules.Organization.Domain.OrganizationUnitTypes;

namespace Villsource.FSH.Modules.Organization.Features.v1.OrganizationUnitTypes;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public static partial class OrganizationUnitTypeMapper
{
    [MapProperty(nameof(OrganizationUnitType.ReferenceId), nameof(OrganizationUnitTypeDetailDto.RefId))]
    [MapPropertyFromSource(nameof(OrganizationUnitTypeDetailDto.CreatedOnUtc), Use = nameof(GetCreatedOnUtc))]
    [MapPropertyFromSource(nameof(OrganizationUnitTypeDetailDto.LastModifiedOnUtc), Use = nameof(GetLastModifiedOnUtc))]
    public static partial OrganizationUnitTypeDetailDto ToDetailDto(this OrganizationUnitType entity);

    private static DateTimeOffset GetCreatedOnUtc(OrganizationUnitType entity) => entity.CreatedOnUtc;
    private static DateTimeOffset? GetLastModifiedOnUtc(OrganizationUnitType entity) => entity.LastModifiedOnUtc;
}
