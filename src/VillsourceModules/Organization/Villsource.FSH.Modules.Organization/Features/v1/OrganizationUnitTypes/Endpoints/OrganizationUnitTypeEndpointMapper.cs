using FSH.Framework.Shared.Persistence;
using Riok.Mapperly.Abstractions;
using Villsource.FSH.Modules.Organization.Contracts.v1.OrganizationUnitTypes.Commands;
using Villsource.FSH.Modules.Organization.Contracts.v1.OrganizationUnitTypes.Dtos;
using Villsource.FSH.Modules.Organization.Contracts.v1.OrganizationUnitTypes.Queries;

namespace Villsource.FSH.Modules.Organization.Features.v1.OrganizationUnitTypes.Endpoints;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
internal static partial class OrganizationUnitTypeEndpointMapper
{
    [MapProperty(nameof(CreateOrganizationUnitTypeRequest.IntlName), nameof(CreateOrganizationUnitTypeCommand.InternationalName))]
    [MapProperty(nameof(CreateOrganizationUnitTypeRequest.Level), nameof(CreateOrganizationUnitTypeCommand.HierarchyLevel))]
    internal static partial CreateOrganizationUnitTypeCommand ToCommand(this CreateOrganizationUnitTypeRequest request);

    [MapProperty(nameof(UpdateOrganizationUnitTypeRequest.IntlName), nameof(UpdateOrganizationUnitTypeCommand.InternationalName))]
    [MapProperty(nameof(UpdateOrganizationUnitTypeRequest.Level), nameof(UpdateOrganizationUnitTypeCommand.HierarchyLevel))]
    internal static partial UpdateOrganizationUnitTypeCommand ToCommand(this UpdateOrganizationUnitTypeRequest request, string refId);

    internal static partial SearchOrganizationUnitTypesQuery ToQuery(this SearchOrganizationUnitTypesRequest request);

    [MapProperty(nameof(OrganizationUnitTypeSummaryDto.InternationalName), nameof(OrganizationUnitTypeSummaryResponse.IntlName))]
    [MapProperty(nameof(OrganizationUnitTypeSummaryDto.HierarchyLevel), nameof(OrganizationUnitTypeSummaryResponse.Level))]
    internal static partial OrganizationUnitTypeSummaryResponse ToResponse(this OrganizationUnitTypeSummaryDto dto);

    internal static partial PagedResponse<OrganizationUnitTypeSummaryResponse> ToResponse(this PagedResponse<OrganizationUnitTypeSummaryDto> page);

    [MapProperty(nameof(OrganizationUnitTypeDetailDto.InternationalName), nameof(OrganizationUnitTypeDetailResponse.IntlName))]
    [MapProperty(nameof(OrganizationUnitTypeDetailDto.HierarchyLevel), nameof(OrganizationUnitTypeDetailResponse.Level))]
    internal static partial OrganizationUnitTypeDetailResponse ToResponse(this OrganizationUnitTypeDetailDto dto);

    [MapProperty(nameof(OrganizationUnitTypeLookupDto.InternationalName), nameof(OrganizationUnitTypeLookupResponse.IntlName))]
    [MapProperty(nameof(OrganizationUnitTypeLookupDto.HierarchyLevel), nameof(OrganizationUnitTypeLookupResponse.Level))]
    internal static partial OrganizationUnitTypeLookupResponse ToResponse(this OrganizationUnitTypeLookupDto dto);

    internal static partial IReadOnlyList<OrganizationUnitTypeLookupResponse> ToResponse(this IReadOnlyList<OrganizationUnitTypeLookupDto> dtos);
}
