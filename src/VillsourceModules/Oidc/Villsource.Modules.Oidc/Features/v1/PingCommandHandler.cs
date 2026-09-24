using Mediator;
using Villsource.Modules.Oidc.Contracts.v1;

namespace Villsource.Modules.Oidc.Features.v1;

public class PingCommandHandler : ICommandHandler<PingCommand, string>
{
    public ValueTask<string> Handle(PingCommand command, CancellationToken cancellationToken)
    {
        return ValueTask.FromResult("Pong.");
    }
}