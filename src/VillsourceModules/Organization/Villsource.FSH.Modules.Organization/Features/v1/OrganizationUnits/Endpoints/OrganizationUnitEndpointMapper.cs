using FSH.Framework.Shared.Persistence;
using Riok.Mapperly.Abstractions;
using Villsource.FSH.Modules.Organization.Contracts.v1.OrganizationUnits.Commands;
using Villsource.FSH.Modules.Organization.Contracts.v1.OrganizationUnits.Dtos;
using Villsource.FSH.Modules.Organization.Contracts.v1.OrganizationUnits.Queries;
using Villsource.FSH.Modules.Organization.Contracts.v1.OrganizationUnitTypes.Dtos;
using Villsource.FSH.Modules.Organization.Features.v1.OrganizationUnitTypes.Endpoints;

namespace Villsource.FSH.Modules.Organization.Features.v1.OrganizationUnits.Endpoints;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
internal static partial class OrganizationUnitEndpointMapper
{
    [MapProperty(nameof(CreateOrganizationUnitRequest.IntlName), nameof(CreateOrganizationUnitCommand.InternationalName))]
    [MapProperty(nameof(CreateOrganizationUnitRequest.TypeRefId), nameof(CreateOrganizationUnitCommand.OrganizationUnitTypeRefId))]
    [MapProperty(nameof(CreateOrganizationUnitRequest.CostCenterRefId), nameof(CreateOrganizationUnitCommand.DefaultCostCenterRefId))]
    [MapProperty(nameof(CreateOrganizationUnitRequest.ManagerRefId), nameof(CreateOrganizationUnitCommand.ManagerPositionRefId))]
    [MapProperty(nameof(CreateOrganizationUnitRequest.AllowExternalManager), nameof(CreateOrganizationUnitCommand.AllowExternalManagerPosition))]
    internal static partial CreateOrganizationUnitCommand ToCommand(this CreateOrganizationUnitRequest request);

    [MapProperty(nameof(UpdateOrganizationUnitRequest.IntlName), nameof(UpdateOrganizationUnitCommand.InternationalName))]
    [MapProperty(nameof(UpdateOrganizationUnitRequest.TypeRefId), nameof(UpdateOrganizationUnitCommand.OrganizationUnitTypeRefId))]
    [MapProperty(nameof(UpdateOrganizationUnitRequest.CostCenterRefId), nameof(UpdateOrganizationUnitCommand.DefaultCostCenterRefId))]
    internal static partial UpdateOrganizationUnitCommand ToCommand(this UpdateOrganizationUnitRequest request, string refId);

    [MapProperty(nameof(AssignManagerPositionRequest.ManagerRefId), nameof(AssignManagerPositionCommand.ManagerPositionRefId))]
    [MapProperty(nameof(AssignManagerPositionRequest.AllowExternalManager), nameof(AssignManagerPositionCommand.AllowExternalManagerPosition))]
    internal static partial AssignManagerPositionCommand ToCommand(this AssignManagerPositionRequest request, string refId);

    [MapProperty(nameof(MoveOrganizationUnitRequest.ParentRefId), nameof(MoveOrganizationUnitCommand.NewParentRefId))]
    internal static partial MoveOrganizationUnitCommand ToCommand(this MoveOrganizationUnitRequest request, string refId);

    [MapProperty(nameof(SearchOrganizationUnitsRequest.ManagerRefId), nameof(SearchOrganizationUnitsQuery.ManagerPositionRefId))]
    [MapProperty(nameof(SearchOrganizationUnitsRequest.TypeRefId), nameof(SearchOrganizationUnitsQuery.OrganizationUnitTypeRefId))]
    internal static partial SearchOrganizationUnitsQuery ToQuery(this SearchOrganizationUnitsRequest request);

    internal static GetOrganizationTreeQuery ToQuery(this GetOrganizationTreeRequest request) =>
        new(request.LegalEntityRefId, request.MaxDepth, request.ActiveOnly ?? true);

    [MapProperty(nameof(OrganizationUnitSummaryDto.InternationalName), nameof(OrganizationUnitSummaryResponse.IntlName))]
    [MapProperty(nameof(OrganizationUnitSummaryDto.OrganizationUnitTypeRefId), nameof(OrganizationUnitSummaryResponse.TypeRefId))]
    [MapProperty(nameof(OrganizationUnitSummaryDto.OrganizationUnitTypeCode), nameof(OrganizationUnitSummaryResponse.TypeCode))]
    [MapProperty(nameof(OrganizationUnitSummaryDto.OrganizationUnitTypeName), nameof(OrganizationUnitSummaryResponse.TypeName))]
    [MapProperty(nameof(OrganizationUnitSummaryDto.OrganizationUnitTypeHierarchyLevel), nameof(OrganizationUnitSummaryResponse.TypeLevel))]
    [MapProperty(nameof(OrganizationUnitSummaryDto.ManagerPositionRefId), nameof(OrganizationUnitSummaryResponse.ManagerRefId))]
    [MapProperty(nameof(OrganizationUnitSummaryDto.ManagerPositionCode), nameof(OrganizationUnitSummaryResponse.ManagerCode))]
    [MapProperty(nameof(OrganizationUnitSummaryDto.ManagerPositionTitle), nameof(OrganizationUnitSummaryResponse.ManagerTitle))]
    internal static partial OrganizationUnitSummaryResponse ToResponse(this OrganizationUnitSummaryDto dto);

    internal static partial PagedResponse<OrganizationUnitSummaryResponse> ToResponse(this PagedResponse<OrganizationUnitSummaryDto> page);

    [MapProperty(nameof(OrganizationUnitDetailDto.InternationalName), nameof(OrganizationUnitDetailResponse.IntlName))]
    [MapProperty(nameof(OrganizationUnitDetailDto.OrganizationUnitType), nameof(OrganizationUnitDetailResponse.Type))]
    [MapProperty(nameof(OrganizationUnitDetailDto.DefaultCostCenterRefId), nameof(OrganizationUnitDetailResponse.CostCenterRefId))]
    [MapProperty(nameof(OrganizationUnitDetailDto.ManagerPositionRefId), nameof(OrganizationUnitDetailResponse.ManagerRefId))]
    [MapProperty(nameof(OrganizationUnitDetailDto.ManagerPositionCode), nameof(OrganizationUnitDetailResponse.ManagerCode))]
    [MapProperty(nameof(OrganizationUnitDetailDto.ManagerPositionTitle), nameof(OrganizationUnitDetailResponse.ManagerTitle))]
    internal static partial OrganizationUnitDetailResponse ToResponse(this OrganizationUnitDetailDto dto);

    [MapProperty(nameof(OrganizationTreeNodeDto.InternationalName), nameof(OrganizationTreeNodeResponse.IntlName))]
    [MapProperty(nameof(OrganizationTreeNodeDto.OrganizationUnitTypeRefId), nameof(OrganizationTreeNodeResponse.TypeRefId))]
    [MapProperty(nameof(OrganizationTreeNodeDto.OrganizationUnitTypeCode), nameof(OrganizationTreeNodeResponse.TypeCode))]
    [MapProperty(nameof(OrganizationTreeNodeDto.OrganizationUnitTypeName), nameof(OrganizationTreeNodeResponse.TypeName))]
    [MapProperty(nameof(OrganizationTreeNodeDto.OrganizationUnitTypeHierarchyLevel), nameof(OrganizationTreeNodeResponse.TypeLevel))]
    [MapProperty(nameof(OrganizationTreeNodeDto.DefaultCostCenterRefId), nameof(OrganizationTreeNodeResponse.CostCenterRefId))]
    [MapProperty(nameof(OrganizationTreeNodeDto.ManagerPositionRefId), nameof(OrganizationTreeNodeResponse.ManagerRefId))]
    [MapProperty(nameof(OrganizationTreeNodeDto.ManagerPositionCode), nameof(OrganizationTreeNodeResponse.ManagerCode))]
    [MapProperty(nameof(OrganizationTreeNodeDto.ManagerPositionTitle), nameof(OrganizationTreeNodeResponse.ManagerTitle))]
    internal static partial OrganizationTreeNodeResponse ToResponse(this OrganizationTreeNodeDto dto);

    internal static partial IReadOnlyList<OrganizationTreeNodeResponse> ToResponse(this IReadOnlyList<OrganizationTreeNodeDto> dtos);

    [MapProperty(nameof(OrganizationUnitChildDto.InternationalName), nameof(OrganizationUnitChildResponse.IntlName))]
    [MapProperty(nameof(OrganizationUnitChildDto.OrganizationUnitTypeCode), nameof(OrganizationUnitChildResponse.TypeCode))]
    [MapProperty(nameof(OrganizationUnitChildDto.OrganizationUnitTypeName), nameof(OrganizationUnitChildResponse.TypeName))]
    [MapProperty(nameof(OrganizationUnitChildDto.ManagerPositionRefId), nameof(OrganizationUnitChildResponse.ManagerRefId))]
    [MapProperty(nameof(OrganizationUnitChildDto.ManagerPositionTitle), nameof(OrganizationUnitChildResponse.ManagerTitle))]
    internal static partial OrganizationUnitChildResponse ToResponse(this OrganizationUnitChildDto dto);

    [MapProperty(nameof(OrganizationUnitLeadershipChainDto.TargetOuRefId), nameof(OrganizationUnitLeadershipChainResponse.TargetRefId))]
    [MapProperty(nameof(OrganizationUnitLeadershipChainDto.TargetOuCode), nameof(OrganizationUnitLeadershipChainResponse.TargetCode))]
    [MapProperty(nameof(OrganizationUnitLeadershipChainDto.TargetOuName), nameof(OrganizationUnitLeadershipChainResponse.TargetName))]
    internal static partial OrganizationUnitLeadershipChainResponse ToResponse(this OrganizationUnitLeadershipChainDto dto);

    [MapProperty(nameof(OrganizationUnitLeadershipNodeDto.ManagerPositionRefId), nameof(OrganizationUnitLeadershipNodeResponse.ManagerRefId))]
    [MapProperty(nameof(OrganizationUnitLeadershipNodeDto.ManagerPositionCode), nameof(OrganizationUnitLeadershipNodeResponse.ManagerCode))]
    [MapProperty(nameof(OrganizationUnitLeadershipNodeDto.ManagerPositionTitle), nameof(OrganizationUnitLeadershipNodeResponse.ManagerTitle))]
    [MapProperty(nameof(OrganizationUnitLeadershipNodeDto.ManagerPositionIsActive), nameof(OrganizationUnitLeadershipNodeResponse.ManagerIsActive))]
    internal static partial OrganizationUnitLeadershipNodeResponse ToResponse(this OrganizationUnitLeadershipNodeDto dto);

    [MapProperty(nameof(OrganizationUnitTypeLookupDto.InternationalName), nameof(OrganizationUnitTypeLookupResponse.IntlName))]
    [MapProperty(nameof(OrganizationUnitTypeLookupDto.HierarchyLevel), nameof(OrganizationUnitTypeLookupResponse.Level))]
    private static partial OrganizationUnitTypeLookupResponse MapUnitType(OrganizationUnitTypeLookupDto dto);
}
