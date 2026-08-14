using Mediator;
using Villsource.FSH.Modules.Organization.Contracts.Dtos;

namespace Villsource.FSH.Modules.Organization.Contracts.v1.Structures;

public sealed record UpdateOrganizationUnitCommand(
    string OrganizationUnitId,
    string Code,
    string Name,
    string? Description) : ICommand<OrganizationUnitDto>;