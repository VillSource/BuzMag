using Mediator;
using Villsource.FSH.Modules.Organization.Contracts.Dtos;

namespace Villsource.FSH.Modules.Organization.Contracts.v1.Structures;

public sealed record MoveOrganizationUnitCommand(
    string OrganizationUnitId,
    string? ParentId) : ICommand<OrganizationUnitDto>;