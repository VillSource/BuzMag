using Mediator;
using Villsource.Modules.HumanResource.Contracts.Dtos;
using Villsource.Modules.HumanResource.Contracts.v1;

namespace Villsource.Modules.HumanResource.Features.v1;

public class GetEmploymentHistoryQueryHandler : IQueryHandler<GetEmploymentHistoryQuery, ICollection<EmploymentDto>>
{
    public ValueTask<ICollection<EmploymentDto>> Handle(GetEmploymentHistoryQuery query,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}