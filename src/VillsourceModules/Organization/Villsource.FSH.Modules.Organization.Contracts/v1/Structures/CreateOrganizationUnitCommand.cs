using Mediator;
using Villsource.FSH.Modules.Organization.Contracts.Dtos;

namespace Villsource.FSH.Modules.Organization.Contracts.v1.Structures;

public sealed record CreateOrganizationUnitCommand(
    string Code,
    string Name,
    string? Description = null,
    string? ParentId = null,
    string? OrganizationId = null) : ICommand<OrganizationUnitDto>;