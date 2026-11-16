using Mediator;

namespace Villsource.FSH.Modules.Organization.Contracts.v1.Positions.Commands;

public sealed record ActivatePositionCommand(string RefId) : IRequest<string>;