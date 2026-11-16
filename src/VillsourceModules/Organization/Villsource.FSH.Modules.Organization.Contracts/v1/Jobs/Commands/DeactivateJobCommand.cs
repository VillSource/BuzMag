using Mediator;

namespace Villsource.FSH.Modules.Organization.Contracts.v1.Jobs.Commands;

public sealed record DeactivateJobCommand(string RefId) : IRequest<string>;