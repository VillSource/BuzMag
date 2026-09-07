using Mediator;
using Villsource.Modules.HumanResource.Contracts.Dtos;
using Villsource.Modules.HumanResource.Contracts.v1;

namespace Villsource.Modules.HumanResource.Features.v1;

public class GetPositionHistoryQueryHandler : IQueryHandler<GetPositionHistoryQuery, ICollection<PositionAssignmentDto>>
{
    public ValueTask<ICollection<PositionAssignmentDto>> Handle(GetPositionHistoryQuery query,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}