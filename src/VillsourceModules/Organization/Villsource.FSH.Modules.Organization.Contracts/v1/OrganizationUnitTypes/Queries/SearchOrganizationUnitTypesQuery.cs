using FSH.Framework.Shared.Persistence;
using Mediator;
using Villsource.FSH.Modules.Organization.Contracts.v1.OrganizationUnitTypes.Dtos;

namespace Villsource.FSH.Modules.Organization.Contracts.v1.OrganizationUnitTypes.Queries;

public sealed record SearchOrganizationUnitTypesQuery : IPagedQuery, IRequest<PagedResponse<OrganizationUnitTypeSummaryDto>>
{
    public int? PageNumber { get; set; }
    public int? PageSize { get; set; }
    public string? Sort { get; set; }
    public string? Keyword { get; set; }
    public bool? IsActive { get; set; }
}
