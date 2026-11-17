using Mediator;

namespace Villsource.FSH.Modules.Organization.Contracts.v1.OrganizationUnitTypes.Commands;

public sealed record ActivateOrganizationUnitTypeCommand(string RefId) : IRequest<Unit>;
