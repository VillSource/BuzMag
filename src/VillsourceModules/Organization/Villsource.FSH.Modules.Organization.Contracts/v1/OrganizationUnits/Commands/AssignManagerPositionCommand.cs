using Mediator;

namespace Villsource.FSH.Modules.Organization.Contracts.v1.OrganizationUnits.Commands;

public sealed record AssignManagerPositionCommand(
    string RefId,
    string? ManagerPositionRefId,
    bool AllowExternalManagerPosition = false) : IRequest<string>;