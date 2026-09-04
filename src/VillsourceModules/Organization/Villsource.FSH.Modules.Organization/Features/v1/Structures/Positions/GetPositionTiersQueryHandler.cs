using Mediator;
using Thinktecture;
using Villsource.FSH.Modules.Organization.Contracts.Constants;
using Villsource.FSH.Modules.Organization.Contracts.v1.Structures;

namespace Villsource.FSH.Modules.Organization.Features.v1.Structures.Positions;

public sealed class GetPositionTiersQueryHandler() : IQueryHandler<GetPositionTiersQuery, IReadOnlyCollection<PositionTier>>
{
    public ValueTask<IReadOnlyCollection<PositionTier>> Handle(GetPositionTiersQuery query, CancellationToken cancellationToken)
    {
        return ValueTask.FromResult<IReadOnlyCollection<PositionTier>>(PositionTier.Items);
    }
}