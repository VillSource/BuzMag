using Mediator;

namespace Villsource.FSH.Modules.Organization.Contracts.v1.OrganizationUnitTypes.Commands;

public sealed record CreateOrganizationUnitTypeCommand(
    string Code,
    string Name,
    string InternationalName,
    int HierarchyLevel,
    string? Description = null,
    bool IsActive = true) : IRequest<string>;
