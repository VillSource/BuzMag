using FSH.Framework.Persistence.Specifications;
using Villsource.Modules.HumanResource.Contracts.Constants;
using Villsource.Modules.HumanResource.Domain;

namespace Villsource.Modules.HumanResource.Features.v1.Positions;

public sealed class PositionAssignmentAtTimeSpec : Specification<PositionAssignment>
{
    public PositionAssignmentAtTimeSpec( Guid employeeId, DateTimeOffset now, bool? isPrimary = null)
    {
        if(isPrimary.HasValue)
            Where(e => e.IsPrimary == isPrimary.Value);
        Where(e => e.EmployeeId == employeeId);
        Where(e => e.EffectiveFrom <= now && (now <= e.EffectiveTo || e.EffectiveTo == null));
    }
}