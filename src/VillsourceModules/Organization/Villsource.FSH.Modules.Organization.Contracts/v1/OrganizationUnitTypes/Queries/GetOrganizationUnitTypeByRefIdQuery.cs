using Mediator;
using Villsource.FSH.Modules.Organization.Contracts.v1.OrganizationUnitTypes.Dtos;

namespace Villsource.FSH.Modules.Organization.Contracts.v1.OrganizationUnitTypes.Queries;

public sealed record GetOrganizationUnitTypeByRefIdQuery(string RefId) : IRequest<OrganizationUnitTypeDetailDto>;
