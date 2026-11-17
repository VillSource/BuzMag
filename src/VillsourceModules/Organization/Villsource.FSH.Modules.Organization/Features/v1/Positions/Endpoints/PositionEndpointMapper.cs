using FSH.Framework.Shared.Persistence;
using Riok.Mapperly.Abstractions;
using Villsource.FSH.Modules.Organization.Contracts.v1.Positions.Commands;
using Villsource.FSH.Modules.Organization.Contracts.v1.Positions.Dtos;
using Villsource.FSH.Modules.Organization.Contracts.v1.Positions.Queries;

namespace Villsource.FSH.Modules.Organization.Features.v1.Positions.Endpoints;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
internal static partial class PositionEndpointMapper
{
    [MapProperty(nameof(CreatePositionRequest.IntlTitle), nameof(CreatePositionCommand.InternationalTitle))]
    [MapProperty(nameof(CreatePositionRequest.TaxBranchRefId), nameof(CreatePositionCommand.WorkLocationTaxBranchRefId))]
    [MapProperty(nameof(CreatePositionRequest.ApprovalLimit), nameof(CreatePositionCommand.ApprovalLimitAmount))]
    [MapProperty(nameof(CreatePositionRequest.ApprovalCurrency), nameof(CreatePositionCommand.ApprovalLimitCurrency))]
    [MapProperty(nameof(CreatePositionRequest.ReportsToRefId), nameof(CreatePositionCommand.ReportsToPositionRefId))]
    [MapProperty(nameof(CreatePositionRequest.DottedLineRefId), nameof(CreatePositionCommand.DottedLinePositionRefId))]
    [MapProperty(nameof(CreatePositionRequest.Headcount), nameof(CreatePositionCommand.HeadcountCapacity))]
    [MapProperty(nameof(CreatePositionRequest.IsLeadership), nameof(CreatePositionCommand.IsPrimaryLeadership))]
    internal static partial CreatePositionCommand ToCommand(this CreatePositionRequest request);

    [MapProperty(nameof(UpdatePositionRequest.IntlTitle), nameof(UpdatePositionCommand.InternationalTitle))]
    [MapProperty(nameof(UpdatePositionRequest.TaxBranchRefId), nameof(UpdatePositionCommand.WorkLocationTaxBranchRefId))]
    [MapProperty(nameof(UpdatePositionRequest.ApprovalLimit), nameof(UpdatePositionCommand.ApprovalLimitAmount))]
    [MapProperty(nameof(UpdatePositionRequest.ApprovalCurrency), nameof(UpdatePositionCommand.ApprovalLimitCurrency))]
    [MapProperty(nameof(UpdatePositionRequest.ReportsToRefId), nameof(UpdatePositionCommand.ReportsToPositionRefId))]
    [MapProperty(nameof(UpdatePositionRequest.DottedLineRefId), nameof(UpdatePositionCommand.DottedLinePositionRefId))]
    [MapProperty(nameof(UpdatePositionRequest.Headcount), nameof(UpdatePositionCommand.HeadcountCapacity))]
    [MapProperty(nameof(UpdatePositionRequest.IsLeadership), nameof(UpdatePositionCommand.IsPrimaryLeadership))]
    internal static partial UpdatePositionCommand ToCommand(this UpdatePositionRequest request, string refId);

    [MapProperty(nameof(SearchPositionsRequest.ReportsToRefId), nameof(SearchPositionsQuery.ReportsToPositionRefId))]
    [MapProperty(nameof(SearchPositionsRequest.IsLeadership), nameof(SearchPositionsQuery.IsPrimaryLeadership))]
    internal static partial SearchPositionsQuery ToQuery(this SearchPositionsRequest request);

    internal static partial GetPositionLookupsQuery ToQuery(this GetPositionLookupsRequest request);

    internal static partial GetPositionReportingChainQuery ToQuery(this GetPositionReportingChainRequest request, string refId);

    internal static partial GetVacancyStatusQuery ToQuery(this GetVacancyStatusRequest request);

    [MapProperty(nameof(ResolveApprovalAuthorityRequest.DocTypeCode), nameof(ResolveApprovalAuthorityQuery.DocumentTypeCode))]
    internal static partial ResolveApprovalAuthorityQuery ToQuery(this ResolveApprovalAuthorityRequest request, string positionRefId);

    internal static partial GetPositionSubtreeQuery ToQuery(this GetPositionSubtreeRequest request, string refId);

    [MapProperty(nameof(PositionSummaryDto.ReportsToPositionRefId), nameof(PositionSummaryResponse.ReportsToRefId))]
    [MapProperty(nameof(PositionSummaryDto.ReportsToPositionCode), nameof(PositionSummaryResponse.ReportsToCode))]
    [MapProperty(nameof(PositionSummaryDto.InternationalTitle), nameof(PositionSummaryResponse.IntlTitle))]
    [MapProperty(nameof(PositionSummaryDto.HeadcountCapacity), nameof(PositionSummaryResponse.Headcount))]
    [MapProperty(nameof(PositionSummaryDto.IsPrimaryLeadership), nameof(PositionSummaryResponse.IsLeadership))]
    [MapProperty(nameof(PositionSummaryDto.ActiveDirectReportCount), nameof(PositionSummaryResponse.DirectReportCount))]
    internal static partial PositionSummaryResponse ToResponse(this PositionSummaryDto dto);

    internal static partial PagedResponse<PositionSummaryResponse> ToResponse(this PagedResponse<PositionSummaryDto> page);

    [MapProperty(nameof(PositionDetailDto.ReportsToPosition), nameof(PositionDetailResponse.ReportsTo))]
    [MapProperty(nameof(PositionDetailDto.DottedLinePosition), nameof(PositionDetailResponse.DottedLine))]
    [MapProperty(nameof(PositionDetailDto.InternationalTitle), nameof(PositionDetailResponse.IntlTitle))]
    [MapProperty(nameof(PositionDetailDto.WorkLocationTaxBranchRefId), nameof(PositionDetailResponse.TaxBranchRefId))]
    [MapProperty(nameof(PositionDetailDto.ApprovalLimitAmount), nameof(PositionDetailResponse.ApprovalLimit))]
    [MapProperty(nameof(PositionDetailDto.ApprovalLimitCurrency), nameof(PositionDetailResponse.ApprovalCurrency))]
    [MapProperty(nameof(PositionDetailDto.HeadcountCapacity), nameof(PositionDetailResponse.Headcount))]
    [MapProperty(nameof(PositionDetailDto.IsPrimaryLeadership), nameof(PositionDetailResponse.IsLeadership))]
    internal static partial PositionDetailResponse ToResponse(this PositionDetailDto dto);

    [MapProperty(nameof(PositionLookupDto.InternationalTitle), nameof(PositionLookupResponse.IntlTitle))]
    [MapProperty(nameof(PositionLookupDto.IsPrimaryLeadership), nameof(PositionLookupResponse.IsLeadership))]
    internal static partial PositionLookupResponse ToResponse(this PositionLookupDto dto);

    internal static partial IReadOnlyList<PositionLookupResponse> ToResponse(this IReadOnlyList<PositionLookupDto> dtos);

    [MapProperty(nameof(PositionNodeDto.InternationalTitle), nameof(PositionNodeResponse.IntlTitle))]
    [MapProperty(nameof(PositionNodeDto.IsPrimaryLeadership), nameof(PositionNodeResponse.IsLeadership))]
    internal static partial PositionNodeResponse ToResponse(this PositionNodeDto dto);

    internal static partial IReadOnlyList<PositionNodeResponse> ToResponse(this IReadOnlyList<PositionNodeDto> dtos);

    [MapProperty(nameof(PositionVacancyDto.HeadcountCapacity), nameof(PositionVacancyResponse.Headcount))]
    internal static partial PositionVacancyResponse ToResponse(this PositionVacancyDto dto);

    internal static partial IReadOnlyList<PositionVacancyResponse> ToResponse(this IReadOnlyList<PositionVacancyDto> dtos);

    [MapProperty(nameof(ApprovalNodeDto.EffectiveApproverPositionRefId), nameof(ApprovalNodeResponse.ApproverRefId))]
    [MapProperty(nameof(ApprovalNodeDto.EffectiveApproverPositionCode), nameof(ApprovalNodeResponse.ApproverCode))]
    [MapProperty(nameof(ApprovalNodeDto.EffectiveApproverPositionTitle), nameof(ApprovalNodeResponse.ApproverTitle))]
    internal static partial ApprovalNodeResponse ToResponse(this ApprovalNodeDto dto);

    [MapProperty(nameof(ApprovalChainDto.TargetPositionRefId), nameof(ApprovalChainResponse.TargetRefId))]
    [MapProperty(nameof(ApprovalChainDto.TargetPositionCode), nameof(ApprovalChainResponse.TargetCode))]
    [MapProperty(nameof(ApprovalChainDto.TargetPositionTitle), nameof(ApprovalChainResponse.TargetTitle))]
    [MapProperty(nameof(ApprovalChainDto.DocumentTypeCode), nameof(ApprovalChainResponse.DocTypeCode))]
    internal static partial ApprovalChainResponse ToResponse(this ApprovalChainDto dto);

    internal static partial PositionCostCenterResponse ToResponse(this PositionCostCenterDto dto);

    internal static partial PositionJobResponse ToResponse(this PositionJobDto dto);

    internal static partial PositionJobGradeResponse ToResponse(this PositionJobGradeDto dto);
}
