using Mediator;

namespace Villsource.FSH.Modules.Organization.Contracts.v1.PositionDelegations.Commands;

public sealed record CreatePositionDelegationCommand(
    string LegalEntityRefId,
    string DelegatorPositionRefId,
    string DelegatePositionRefId,
    string DelegationType,
    DateTimeOffset ValidFrom,
    DateTimeOffset ValidTo,
    string? DocumentTypeScope = null,
    string? Reason = null) : IRequest<string>;