using Mediator;

namespace Villsource.FSH.Modules.Organization.Contracts.v1.OrganizationUnitTypes.Commands;

public sealed record UpdateOrganizationUnitTypeCommand(
    string RefId,
    string Code,
    string Name,
    string InternationalName,
    int HierarchyLevel,
    string? Description = null) : IRequest<string>;
