using Mediator;
using Villsource.FSH.Modules.Organization.Contracts.v1.LegalEntities.Dtos;

namespace Villsource.FSH.Modules.Organization.Contracts.v1.LegalEntities.Queries;

public sealed record GetLegalEntityByRefIdQuery(string RefId) : IRequest<LegalEntityDetailDto>;