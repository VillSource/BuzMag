using Mediator;
using Villsource.Modules.HumanResource.Contracts.Dtos;
using Villsource.Modules.HumanResource.Contracts.v1;

namespace Villsource.Modules.HumanResource.Features.v1;

public class TerminateEmploymentCommandHandler : ICommandHandler<TerminateEmploymentCommand, EmployeeDto>
{
    public ValueTask<EmployeeDto> Handle(TerminateEmploymentCommand command, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}