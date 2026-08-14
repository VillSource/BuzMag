using Mediator;
using Villsource.FSH.Modules.Organization.Contracts.Dtos;

namespace Villsource.FSH.Modules.Organization.Contracts.v1.Structures.PositionAllocations;

public sealed record GetCurrentPositionAllocationsQuery(string OrganizationUnitId) : IQuery<ICollection<OrganizationUnitPositionAllocationDto>>;