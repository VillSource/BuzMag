using FSH.Framework.Persistence.Specifications;
using Villsource.FSH.Modules.Organization.Domain.PositionDelegations;

namespace Villsource.FSH.Modules.Organization.Features.v1.PositionDelegations.Specifications;

public sealed class GetPositionDelegationByRefIdSpec : Specification<PositionDelegation>
{
    public GetPositionDelegationByRefIdSpec(string refId)
    {
        Where(d => d.ReferenceId == refId);
        Include(d => d.LegalEntity!);
        Include("DelegatorPosition.JobGrade");
        Include("DelegatePosition.JobGrade");
    }
}