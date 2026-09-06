using Mediator;
using Villsource.Modules.HumanResource.Contracts.Dtos;
using Villsource.Modules.HumanResource.Contracts.v1;

namespace Villsource.Modules.HumanResource.Features.v1.Employ.GetEmployees;

public class GetEmployeeDetailQueryHandler : IQueryHandler<GetEmployeeDetailQuery, EmployeeDto>
{
    public ValueTask<EmployeeDto> Handle(GetEmployeeDetailQuery query, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}