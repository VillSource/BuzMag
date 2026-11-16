using FSH.Framework.Shared.Persistence;
using Mediator;
using Villsource.FSH.Modules.Organization.Contracts.v1.LegalEntities.Dtos;

namespace Villsource.FSH.Modules.Organization.Contracts.v1.LegalEntities.Queries;

public sealed record SearchLegalEntitiesQuery : IPagedQuery, IRequest<PagedResponse<LegalEntitySummaryDto>>
{
    public int? PageNumber { get; set; }
    public int? PageSize { get; set; }
    public string? Sort { get; set; }
    public string? Keyword { get; set; }
    public string? CountryCode { get; set; }
}