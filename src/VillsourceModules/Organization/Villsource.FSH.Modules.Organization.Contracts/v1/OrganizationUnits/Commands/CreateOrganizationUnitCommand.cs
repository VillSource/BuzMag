using Mediator;

namespace Villsource.FSH.Modules.Organization.Contracts.v1.OrganizationUnits.Commands;

public sealed record CreateOrganizationUnitCommand(
    string Code,
    string Name,
    string InternationalName,
    string OrganizationUnitTypeRefId,
    string? Description = null,
    string? LegalEntityRefId = null,
    string? ParentRefId = null,
    string? DefaultCostCenterRefId = null,
    string? ManagerPositionRefId = null,
    bool AllowExternalManagerPosition = false) : IRequest<string>;