using Mediator;

namespace Villsource.FSH.Modules.Organization.Contracts.v1.OrganizationUnits.Commands;

public sealed record UpdateOrganizationUnitCommand(
    string RefId,
    string Code,
    string Name,
    string InternationalName,
    string Type,
    string? Description = null,
    string? DefaultCostCenterRefId = null) : IRequest<string>;