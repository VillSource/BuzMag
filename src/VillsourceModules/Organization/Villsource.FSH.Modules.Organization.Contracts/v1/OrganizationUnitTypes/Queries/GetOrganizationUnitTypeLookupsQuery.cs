using Mediator;
using Villsource.FSH.Modules.Organization.Contracts.v1.OrganizationUnitTypes.Dtos;

namespace Villsource.FSH.Modules.Organization.Contracts.v1.OrganizationUnitTypes.Queries;

public sealed record GetOrganizationUnitTypeLookupsQuery(bool ActiveOnly = true)
    : IRequest<IReadOnlyList<OrganizationUnitTypeLookupDto>>;
