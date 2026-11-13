using FSH.Framework.Persistence.Specifications;
using Villsource.FSH.Modules.Organization.Domain;

namespace Villsource.FSH.Modules.Organization.Features.v1.OrganizationUnits.Specifications;

public sealed class OrganizationUnitByRefSpec : Specification<OrganizationUnit>
{
    public OrganizationUnitByRefSpec(string unitRef)
    {
        Where(p => p.ReferenceId == unitRef);
    }
    
    public static OrganizationUnitByRefSpec Create(string unitRef) => new(unitRef);
}