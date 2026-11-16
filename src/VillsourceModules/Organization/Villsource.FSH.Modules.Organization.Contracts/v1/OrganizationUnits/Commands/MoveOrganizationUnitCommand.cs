using Mediator;

namespace Villsource.FSH.Modules.Organization.Contracts.v1.OrganizationUnits.Commands;

public sealed record MoveOrganizationUnitCommand(
    string RefId,
    string? NewParentRefId) : IRequest<string>;