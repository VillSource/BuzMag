using FSH.Framework.Shared.Persistence;
using Riok.Mapperly.Abstractions;
using Villsource.FSH.Modules.Organization.Contracts.v1.PositionDelegations.Commands;
using Villsource.FSH.Modules.Organization.Contracts.v1.PositionDelegations.Dtos;
using Villsource.FSH.Modules.Organization.Contracts.v1.PositionDelegations.Queries;

namespace Villsource.FSH.Modules.Organization.Features.v1.PositionDelegations.Endpoints;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
internal static partial class PositionDelegationEndpointMapper
{
    [MapProperty(nameof(CreatePositionDelegationRequest.DelegatorRefId), nameof(CreatePositionDelegationCommand.DelegatorPositionRefId))]
    [MapProperty(nameof(CreatePositionDelegationRequest.DelegateRefId), nameof(CreatePositionDelegationCommand.DelegatePositionRefId))]
    [MapProperty(nameof(CreatePositionDelegationRequest.Type), nameof(CreatePositionDelegationCommand.DelegationType))]
    [MapProperty(nameof(CreatePositionDelegationRequest.DocScope), nameof(CreatePositionDelegationCommand.DocumentTypeScope))]
    internal static partial CreatePositionDelegationCommand ToCommand(this CreatePositionDelegationRequest request);

    [MapProperty(nameof(UpdatePositionDelegationRequest.Type), nameof(UpdatePositionDelegationCommand.DelegationType))]
    [MapProperty(nameof(UpdatePositionDelegationRequest.DocScope), nameof(UpdatePositionDelegationCommand.DocumentTypeScope))]
    internal static partial UpdatePositionDelegationCommand ToCommand(this UpdatePositionDelegationRequest request, string refId);

    [MapProperty(nameof(RevokePositionDelegationRequest.Reason), nameof(RevokePositionDelegationCommand.RevocationReason))]
    internal static partial RevokePositionDelegationCommand ToCommand(this RevokePositionDelegationRequest request, string refId);

    [MapProperty(nameof(SearchPositionDelegationsRequest.DelegatorRefId), nameof(SearchPositionDelegationsQuery.DelegatorPositionRefId))]
    [MapProperty(nameof(SearchPositionDelegationsRequest.DelegateRefId), nameof(SearchPositionDelegationsQuery.DelegatePositionRefId))]
    internal static partial SearchPositionDelegationsQuery ToQuery(this SearchPositionDelegationsRequest request);

    [MapProperty(nameof(ResolveEffectiveDelegationsRequest.DelegatorRefId), nameof(ResolveEffectiveDelegationsQuery.DelegatorPositionRefId))]
    [MapProperty(nameof(ResolveEffectiveDelegationsRequest.DocTypeCode), nameof(ResolveEffectiveDelegationsQuery.DocumentTypeCode))]
    internal static partial ResolveEffectiveDelegationsQuery ToQuery(this ResolveEffectiveDelegationsRequest request);

    [MapProperty(nameof(PositionDelegationSummaryDto.DelegatorPositionRefId), nameof(PositionDelegationSummaryResponse.DelegatorRefId))]
    [MapProperty(nameof(PositionDelegationSummaryDto.DelegatorPositionCode), nameof(PositionDelegationSummaryResponse.DelegatorCode))]
    [MapProperty(nameof(PositionDelegationSummaryDto.DelegatorPositionTitle), nameof(PositionDelegationSummaryResponse.DelegatorTitle))]
    [MapProperty(nameof(PositionDelegationSummaryDto.DelegatePositionRefId), nameof(PositionDelegationSummaryResponse.DelegateRefId))]
    [MapProperty(nameof(PositionDelegationSummaryDto.DelegatePositionCode), nameof(PositionDelegationSummaryResponse.DelegateCode))]
    [MapProperty(nameof(PositionDelegationSummaryDto.DelegatePositionTitle), nameof(PositionDelegationSummaryResponse.DelegateTitle))]
    [MapProperty(nameof(PositionDelegationSummaryDto.DelegationType), nameof(PositionDelegationSummaryResponse.Type))]
    [MapProperty(nameof(PositionDelegationSummaryDto.DocumentTypeScope), nameof(PositionDelegationSummaryResponse.DocScope))]
    internal static partial PositionDelegationSummaryResponse ToResponse(this PositionDelegationSummaryDto dto);

    internal static partial PagedResponse<PositionDelegationSummaryResponse> ToResponse(this PagedResponse<PositionDelegationSummaryDto> page);

    [MapProperty(nameof(PositionDelegationDetailDto.DelegatorPosition), nameof(PositionDelegationDetailResponse.Delegator))]
    [MapProperty(nameof(PositionDelegationDetailDto.DelegatePosition), nameof(PositionDelegationDetailResponse.Delegate))]
    [MapProperty(nameof(PositionDelegationDetailDto.DelegationType), nameof(PositionDelegationDetailResponse.Type))]
    [MapProperty(nameof(PositionDelegationDetailDto.DocumentTypeScope), nameof(PositionDelegationDetailResponse.DocScope))]
    internal static partial PositionDelegationDetailResponse ToResponse(this PositionDelegationDetailDto dto);

    [MapProperty(nameof(EffectiveDelegationDto.DelegationType), nameof(EffectiveDelegationResponse.Type))]
    [MapProperty(nameof(EffectiveDelegationDto.DocumentTypeScope), nameof(EffectiveDelegationResponse.DocScope))]
    [MapProperty(nameof(EffectiveDelegationDto.DelegatePosition), nameof(EffectiveDelegationResponse.Delegate))]
    internal static partial EffectiveDelegationResponse ToResponse(this EffectiveDelegationDto dto);

    internal static partial IReadOnlyList<EffectiveDelegationResponse> ToResponse(this IReadOnlyList<EffectiveDelegationDto> dtos);

    internal static partial DelegationPositionResponse ToResponse(this DelegationPositionDto dto);

    [MapProperty(nameof(DelegationTypeDto.RequiresDocumentScope), nameof(DelegationTypeResponse.RequiresDocScope))]
    internal static partial DelegationTypeResponse ToResponse(this DelegationTypeDto dto);

    internal static partial IReadOnlyList<DelegationTypeResponse> ToResponse(this IReadOnlyList<DelegationTypeDto> dtos);
}
