using Mediator;
using Villsource.FSH.Modules.Organization.Contracts.Dtos;

namespace Villsource.FSH.Modules.Organization.Contracts.v1.Structures;

public sealed record GetAllPositionsQuery(string? OrganizationId = null) : IQuery<ICollection<PositionDto>>;