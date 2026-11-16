using Mediator;
using Villsource.FSH.Modules.Organization.Contracts.v1.OrganizationUnits.Dtos;

namespace Villsource.FSH.Modules.Organization.Contracts.v1.OrganizationUnits.Queries;

public sealed record GetOrganizationUnitLeadershipChainQuery(string RefId) : IRequest<OrganizationUnitLeadershipChainDto>;