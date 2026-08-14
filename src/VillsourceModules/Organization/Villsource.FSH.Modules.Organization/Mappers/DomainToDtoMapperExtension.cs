using Riok.Mapperly.Abstractions;
using Villsource.FSH.Modules.Organization.Contracts.Dtos;
using Villsource.FSH.Modules.Organization.Domain;

namespace Villsource.FSH.Modules.Organization.Mappers;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public static partial class DomainToDtoMapperExtension
{
    public static partial OrganizationUnitDto ToDto(this OrganizationUnit source);
    public static partial OrganizationDto ToDto(this Organization.Domain.Organization source);
}