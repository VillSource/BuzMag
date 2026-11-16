using Mediator;

namespace Villsource.FSH.Modules.Organization.Contracts.v1.LegalEntities.Commands;

public sealed record DeleteLegalEntityCommand(string RefId) : IRequest<string>;