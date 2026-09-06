using Mediator;
using Villsource.Modules.HumanResource.Contracts.Dtos;
using Villsource.Modules.HumanResource.Contracts.v1;

namespace Villsource.Modules.HumanResource.Features.v1;

public class UpdateEmploymentCommandHandler : ICommandHandler<UpdateEmploymentCommand, EmploymentDto>
{
    public ValueTask<EmploymentDto> Handle(UpdateEmploymentCommand command, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}