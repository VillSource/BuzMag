using Mediator;

namespace Villsource.FSH.Modules.Organization.Contracts.v1.LegalEntities.Commands;

public sealed record ActivateTaxBranchCommand(
    string LegalEntityRefId,
    string RefId) : IRequest<string>;