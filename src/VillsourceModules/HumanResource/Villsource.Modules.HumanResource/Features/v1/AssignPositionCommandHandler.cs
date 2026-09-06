using Mediator;
using Villsource.Modules.HumanResource.Contracts.Dtos;
using Villsource.Modules.HumanResource.Contracts.v1;

namespace Villsource.Modules.HumanResource.Features.v1;

public class AssignPositionCommandHandler : ICommandHandler<AssignPositionCommand, PositionAssignmentDto>
{
    public ValueTask<PositionAssignmentDto> Handle(AssignPositionCommand command, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}