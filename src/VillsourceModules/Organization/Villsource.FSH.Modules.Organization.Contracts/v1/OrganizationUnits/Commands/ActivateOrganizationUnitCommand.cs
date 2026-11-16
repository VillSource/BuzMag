using Mediator;

namespace Villsource.FSH.Modules.Organization.Contracts.v1.OrganizationUnits.Commands;

public sealed record ActivateOrganizationUnitCommand(string RefId) : IRequest<string>;