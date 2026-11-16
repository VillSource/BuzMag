using Mediator;

namespace Villsource.FSH.Modules.Organization.Contracts.v1.LegalEntities.Commands;

public sealed record RemoveTaxBranchCommand(
    string LegalEntityRefId,
    string RefId) : IRequest<string>;