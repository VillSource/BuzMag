using Mediator;

namespace Villsource.FSH.Modules.Organization.Contracts.v1.PositionDelegations.Commands;

public sealed record RevokePositionDelegationCommand(
    string RefId,
    string? RevocationReason = null) : IRequest<string>;