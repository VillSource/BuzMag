using Mediator;
using Villsource.FSH.Modules.Organization.Contracts.Dtos;

namespace Villsource.FSH.Modules.Organization.Contracts.v1.Structures.PositionAllocations;

public sealed record EndPositionAllocationCommand(Guid AllocationId) : ICommand<OrganizationUnitPositionAllocationDto>;