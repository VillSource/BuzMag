using Riok.Mapperly.Abstractions;
using Villsource.FSH.Modules.Organization.Contracts.v1.OrganizationUnits.Commands;

namespace Villsource.FSH.Modules.Organization.Features.v1.OrganizationUnits;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
internal static partial class OrganizationUnitRequestMapper
{
    internal static partial UpdateOrganizationUnitCommand ToCommand(
        this UpdateOrganizationUnitRequest request,
        string refId);

    internal static partial AssignManagerPositionCommand ToCommand(
        this AssignManagerPositionRequest request,
        string refId);

    internal static partial MoveOrganizationUnitCommand ToCommand(
        this MoveOrganizationUnitRequest request,
        string refId);
}