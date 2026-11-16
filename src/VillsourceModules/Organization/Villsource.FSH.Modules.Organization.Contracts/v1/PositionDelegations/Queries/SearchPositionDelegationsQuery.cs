using FSH.Framework.Shared.Persistence;
using Mediator;
using Villsource.FSH.Modules.Organization.Contracts.v1.PositionDelegations.Dtos;

namespace Villsource.FSH.Modules.Organization.Contracts.v1.PositionDelegations.Queries;

public sealed record SearchPositionDelegationsQuery : IPagedQuery, IRequest<PagedResponse<PositionDelegationSummaryDto>>
{
    public int? PageNumber { get; set; }
    public int? PageSize { get; set; }
    public string? Sort { get; set; }
    public string? LegalEntityRefId { get; set; }
    public string? DelegatorPositionRefId { get; set; }
    public string? DelegatePositionRefId { get; set; }
    public bool? IsRevoked { get; set; }
    public DateTimeOffset? EffectiveAtUtc { get; set; }
}