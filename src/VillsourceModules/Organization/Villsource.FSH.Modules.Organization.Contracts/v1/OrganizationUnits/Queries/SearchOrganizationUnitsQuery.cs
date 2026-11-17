using FSH.Framework.Shared.Persistence;
using Mediator;
using Villsource.FSH.Modules.Organization.Contracts.v1.OrganizationUnits.Dtos;

namespace Villsource.FSH.Modules.Organization.Contracts.v1.OrganizationUnits.Queries;

public sealed record SearchOrganizationUnitsQuery : IPagedQuery, IRequest<PagedResponse<OrganizationUnitSummaryDto>>
{
    public int? PageNumber { get; set; }
    public int? PageSize { get; set; }
    public string? Sort { get; set; }
    public string? Keyword { get; set; }
    public string? LegalEntityRefId { get; set; }
    public string? ParentRefId { get; set; }
    public string? ManagerPositionRefId { get; set; }
    public string? OrganizationUnitTypeRefId { get; set; }
    public bool? IsActive { get; set; }
    public bool RootOnly { get; set; }
    public bool VacantManagerOnly { get; set; }
}