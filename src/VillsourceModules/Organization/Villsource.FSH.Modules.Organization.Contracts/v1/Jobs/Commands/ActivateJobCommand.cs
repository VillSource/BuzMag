using Mediator;

namespace Villsource.FSH.Modules.Organization.Contracts.v1.Jobs.Commands;

public sealed record ActivateJobCommand(string RefId) : IRequest<string>;