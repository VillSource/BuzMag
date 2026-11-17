using Mediator;
using Villsource.FSH.Modules.Organization.Contracts.v1.OrganizationUnits.Dtos;

namespace Villsource.FSH.Modules.Organization.Contracts.v1.OrganizationUnits.Queries;

public sealed record GetOrganizationTreeQuery(
    string LegalEntityRefId,
    int? MaxDepth = null,
    bool ActiveOnly = true) : IRequest<IReadOnlyList<OrganizationTreeNodeDto>>;
