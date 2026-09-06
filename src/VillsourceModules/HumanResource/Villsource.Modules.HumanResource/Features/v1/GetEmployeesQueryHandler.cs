using Mediator;
using Villsource.Modules.HumanResource.Contracts.Dtos;
using Villsource.Modules.HumanResource.Contracts.v1;

namespace Villsource.Modules.HumanResource.Features.v1;

public class GetEmployeesQueryHandler : IQueryHandler<GetEmployeesQuery, ICollection<EmployeeDto>>
{
    public ValueTask<ICollection<EmployeeDto>> Handle(GetEmployeesQuery query, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}