using FSH.Framework.Shared.Persistence;
using Riok.Mapperly.Abstractions;
using Villsource.FSH.Modules.Organization.Contracts.v1.CostCenters;
using Villsource.FSH.Modules.Organization.Contracts.v1.CostCenters.Commands;
using Villsource.FSH.Modules.Organization.Contracts.v1.CostCenters.Dtos;
using Villsource.FSH.Modules.Organization.Contracts.v1.CostCenters.Queries;

namespace Villsource.FSH.Modules.Organization.Features.v1.CostCenters.Endpoints;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
internal static partial class CostCenterEndpointMapper
{
    [MapProperty(nameof(CreateCostCenterRequest.IntlName), nameof(CreateCostCenterCommand.InternationalName))]
    [MapProperty(nameof(CreateCostCenterRequest.ParentRefId), nameof(CreateCostCenterCommand.ParentCostCenterRefId))]
    [MapProperty(nameof(CreateCostCenterRequest.TaxBranchRefId), nameof(CreateCostCenterCommand.DefaultTaxBranchRefId))]
    [MapProperty(nameof(CreateCostCenterRequest.ResponsibleRefId), nameof(CreateCostCenterCommand.ResponsiblePositionRefId))]
    internal static partial CreateCostCenterCommand ToCommand(this CreateCostCenterRequest request);

    [MapProperty(nameof(UpdateCostCenterRequest.IntlName), nameof(UpdateCostCenterCommand.InternationalName))]
    [MapProperty(nameof(UpdateCostCenterRequest.ParentRefId), nameof(UpdateCostCenterCommand.ParentCostCenterRefId))]
    [MapProperty(nameof(UpdateCostCenterRequest.TaxBranchRefId), nameof(UpdateCostCenterCommand.DefaultTaxBranchRefId))]
    [MapProperty(nameof(UpdateCostCenterRequest.ResponsibleRefId), nameof(UpdateCostCenterCommand.ResponsiblePositionRefId))]
    internal static partial UpdateCostCenterCommand ToCommand(this UpdateCostCenterRequest request, string refId);

    [MapProperty(nameof(SearchCostCentersRequest.ParentRefId), nameof(SearchCostCentersQuery.ParentCostCenterRefId))]
    [MapProperty(nameof(SearchCostCentersRequest.ResponsibleRefId), nameof(SearchCostCentersQuery.ResponsiblePositionRefId))]
    internal static partial SearchCostCentersQuery ToQuery(this SearchCostCentersRequest request);

    [MapProperty(nameof(CostCenterSummaryDto.ParentCostCenterRefId), nameof(CostCenterSummaryResponse.ParentRefId))]
    [MapProperty(nameof(CostCenterSummaryDto.ParentCostCenterCode), nameof(CostCenterSummaryResponse.ParentCode))]
    [MapProperty(nameof(CostCenterSummaryDto.InternationalName), nameof(CostCenterSummaryResponse.IntlName))]
    [MapProperty(nameof(CostCenterSummaryDto.DefaultTaxBranchCode), nameof(CostCenterSummaryResponse.TaxBranchCode))]
    [MapProperty(nameof(CostCenterSummaryDto.ResponsiblePositionRefId), nameof(CostCenterSummaryResponse.ResponsibleRefId))]
    [MapProperty(nameof(CostCenterSummaryDto.ResponsiblePositionCode), nameof(CostCenterSummaryResponse.ResponsibleCode))]
    [MapProperty(nameof(CostCenterSummaryDto.ResponsiblePositionTitle), nameof(CostCenterSummaryResponse.ResponsibleTitle))]
    internal static partial CostCenterSummaryResponse ToResponse(this CostCenterSummaryDto dto);

    internal static partial PagedResponse<CostCenterSummaryResponse> ToResponse(this PagedResponse<CostCenterSummaryDto> page);

    [MapProperty(nameof(CostCenterDetailDto.ParentCostCenter), nameof(CostCenterDetailResponse.Parent))]
    [MapProperty(nameof(CostCenterDetailDto.DefaultTaxBranch), nameof(CostCenterDetailResponse.TaxBranch))]
    [MapProperty(nameof(CostCenterDetailDto.InternationalName), nameof(CostCenterDetailResponse.IntlName))]
    [MapProperty(nameof(CostCenterDetailDto.ResponsiblePositionRefId), nameof(CostCenterDetailResponse.ResponsibleRefId))]
    [MapProperty(nameof(CostCenterDetailDto.ResponsiblePositionCode), nameof(CostCenterDetailResponse.ResponsibleCode))]
    [MapProperty(nameof(CostCenterDetailDto.ResponsiblePositionTitle), nameof(CostCenterDetailResponse.ResponsibleTitle))]
    [MapProperty(nameof(CostCenterDetailDto.ChildCostCenters), nameof(CostCenterDetailResponse.Children))]
    internal static partial CostCenterDetailResponse ToResponse(this CostCenterDetailDto dto);

    [MapProperty(nameof(CostCenterNodeDto.InternationalName), nameof(CostCenterNodeResponse.IntlName))]
    internal static partial CostCenterNodeResponse ToResponse(this CostCenterNodeDto dto);

    internal static partial CostCenterTaxBranchResponse ToResponse(this CostCenterTaxBranchDto dto);

    internal static partial CostCenterFunctionTypeResponse ToResponse(this CostCenterFunctionTypeDto dto);

    internal static partial IReadOnlyList<CostCenterFunctionTypeResponse> ToResponse(this IReadOnlyList<CostCenterFunctionTypeDto> dtos);
}
