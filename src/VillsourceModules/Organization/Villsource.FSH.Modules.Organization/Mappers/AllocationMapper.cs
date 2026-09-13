using Villsource.FSH.Modules.Organization.Contracts.Dtos;
using Villsource.FSH.Modules.Organization.Domain;

namespace Villsource.FSH.Modules.Organization.Mappers;

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