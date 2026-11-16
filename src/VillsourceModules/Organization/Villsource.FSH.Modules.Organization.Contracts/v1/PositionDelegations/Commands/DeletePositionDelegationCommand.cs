using Mediator;

namespace Villsource.FSH.Modules.Organization.Contracts.v1.PositionDelegations.Commands;

public sealed record DeletePositionDelegationCommand(string RefId) : IRequest<string>;