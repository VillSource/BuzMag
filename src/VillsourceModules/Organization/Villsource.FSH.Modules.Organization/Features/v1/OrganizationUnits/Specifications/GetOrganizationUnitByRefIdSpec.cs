using FSH.Framework.Persistence.Specifications;
using Villsource.FSH.Modules.Organization.Domain.OrganizationUnits;

namespace Villsource.FSH.Modules.Organization.Features.v1.OrganizationUnits;

public sealed class GetOrganizationUnitByRefIdSpec : Specification<OrganizationUnit>
{
    public GetOrganizationUnitByRefIdSpec(string refId)
    {
        Where(e => e.ReferenceId == refId);
        Include(e => e.LegalEntity!);
        Include(e => e.DefaultCostCenter!);
        Include(e => e.Parent!);
        Include(e => e.ManagerPosition!);
        Include(e => e.Children.Where(c => !c.IsDeleted));
        Include(c => c.ManagerPosition!);
    }
}