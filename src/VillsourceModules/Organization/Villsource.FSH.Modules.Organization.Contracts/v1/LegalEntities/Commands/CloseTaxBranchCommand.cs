using Mediator;

namespace Villsource.FSH.Modules.Organization.Contracts.v1.LegalEntities.Commands;

public sealed record CloseTaxBranchCommand(
    string LegalEntityRefId,
    string RefId,
    DateTimeOffset? ClosedDate = null) : IRequest<string>;