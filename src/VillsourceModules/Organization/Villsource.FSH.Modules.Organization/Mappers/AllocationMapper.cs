using Villsource.FSH.Modules.Organization.Contracts.Dtos;
using Villsource.FSH.Modules.Organization.Domain;
using Villsource.FSH.Modules.Organization.Mappers;

namespace Villsource.FSH.Modules.Organization.Features.v1.Structures.PositionAllocations;

internal static class AllocationMapper
{
    public static PositionAllocationDto ToDto(OrganizationUnitPositionAllocation allocation,
        string unitReferenceId, Position? position = null) => new()
    {
        Id = allocation.Id,
        ReferenceId = unitReferenceId,
        Position = (position ?? allocation.Position).ToDto(),
        HeadCount = allocation.HeadCount,
        EffectiveFrom = allocation.EffectiveFrom,
        EffectiveTo = allocation.EffectiveTo
    };
}