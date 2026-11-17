using Riok.Mapperly.Abstractions;
using Villsource.FSH.Modules.Organization.Contracts.v1.PositionDelegations.Commands;
using Villsource.FSH.Modules.Organization.Features.v1.PositionDelegations.Endpoints;

namespace Villsource.FSH.Modules.Organization.Features.v1.PositionDelegations.Mappers;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
internal static partial class PositionDelegationRequestMapper
{
    internal static partial UpdatePositionDelegationCommand ToCommand(
        this UpdatePositionDelegationRequest request,
        string refId);
}