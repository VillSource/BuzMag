using Mediator;

namespace Villsource.FSH.Modules.Organization.Contracts.v1.Positions.Commands;

public sealed record DeletePositionCommand(string RefId) : IRequest<string>;