using Mediator;
using Villsource.FSH.Modules.Organization.Contracts.v1.Positions.Dtos;

namespace Villsource.FSH.Modules.Organization.Contracts.v1.Positions.Queries;

public sealed record GetPositionLookupsQuery(
    string? LegalEntityRefId = null,
    Guid? OrgUnitId = null,
    bool ActiveOnly = true,
    bool LeadershipOnly = false) : IRequest<IReadOnlyList<PositionLookupDto>>;