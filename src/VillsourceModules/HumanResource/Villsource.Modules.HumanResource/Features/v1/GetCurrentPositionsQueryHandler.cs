using Mediator;
using Villsource.Modules.HumanResource.Contracts.Dtos;
using Villsource.Modules.HumanResource.Contracts.v1;

namespace Villsource.Modules.HumanResource.Features.v1;

public class
    GetCurrentPositionsQueryHandler : IQueryHandler<GetCurrentPositionsQuery, ICollection<PositionAssignmentDto>>
{
    public ValueTask<ICollection<PositionAssignmentDto>> Handle(GetCurrentPositionsQuery query,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}