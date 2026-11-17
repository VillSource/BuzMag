using Mediator;

namespace Villsource.FSH.Modules.Organization.Contracts.v1.OrganizationUnitTypes.Commands;

public sealed record DeactivateOrganizationUnitTypeCommand(string RefId) : IRequest<Unit>;
