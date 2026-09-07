using FSH.Framework.Persistence.Specifications;
using Villsource.Modules.HumanResource.Contracts.Constants;
using Villsource.Modules.HumanResource.Domain;

namespace Villsource.Modules.HumanResource.Features.v1.Employments;

public sealed class EmployeeEmploymentAtTimeSpec : Specification<Employment>
{
    public EmployeeEmploymentAtTimeSpec(EmploymentType type, Guid employeeId, DateTimeOffset now)
    {
        Where(e => e.Type == type);
        Where(e => e.EmployeeId == employeeId);
        Where(e => e.EffectiveFrom <= now && (now <= e.EffectiveTo || e.EffectiveTo == null));
    }
}