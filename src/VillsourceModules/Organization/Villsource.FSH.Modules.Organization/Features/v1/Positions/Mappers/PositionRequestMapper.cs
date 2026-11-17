using Riok.Mapperly.Abstractions;
using Villsource.FSH.Modules.Organization.Contracts.v1.Positions.Commands;
using Villsource.FSH.Modules.Organization.Features.v1.Positions.Endpoinsts;

namespace Villsource.FSH.Modules.Organization.Features.v1.Positions.Mappers;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
internal static partial class PositionRequestMapper
{
    internal static partial UpdatePositionCommand ToCommand(this UpdatePositionRequest request, string refId);
}