using Mediator;
using Villsource.FSH.Modules.Organization.Contracts.v1.PositionDelegations.Dtos;
using Villsource.FSH.Modules.Organization.Contracts.v1.PositionDelegations.Queries;
using Villsource.FSH.Modules.Organization.Domain.PositionDelegations;

namespace Villsource.FSH.Modules.Organization.Features.v1.PositionDelegations.Queries;

public sealed class GetDelegationTypesQueryHandler
    : IRequestHandler<GetDelegationTypesQuery, IReadOnlyList<DelegationTypeDto>>
{
    private static readonly IReadOnlyList<DelegationTypeDto> CachedItems =
        DelegationType.Items.Select(x => Mappers.PositionDelegationMapper.ToDto(x)).ToList();

    public ValueTask<IReadOnlyList<DelegationTypeDto>> Handle(
        GetDelegationTypesQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return ValueTask.FromResult(CachedItems);
    }
}