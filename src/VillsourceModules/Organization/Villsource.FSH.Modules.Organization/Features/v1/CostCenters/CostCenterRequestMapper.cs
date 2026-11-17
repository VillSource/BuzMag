using Riok.Mapperly.Abstractions;
using Villsource.FSH.Modules.Organization.Contracts.v1.CostCenters.Commands;

namespace Villsource.FSH.Modules.Organization.Features.v1.CostCenters;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
internal static partial class CostCenterRequestMapper
{
    internal static partial UpdateCostCenterCommand ToCommand(
        this UpdateCostCenterRequest request,
        string refId);
}
