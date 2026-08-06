using Mediator;
using Villsource.FSH.Modules.Organization.Contracts.Dtos;

namespace Villsource.FSH.Modules.Organization.Contracts.v1.Structures;

public sealed record MoveOrganizationUnitCommand(
    Guid OrganizationUnitId,
    Guid? OrganizationId,
    Guid? ParentId) : ICommand<OrganizationUnitDto>;