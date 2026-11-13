using FSH.Framework.Persistence.Specifications;
using Villsource.FSH.Modules.Organization.Domain;

namespace Villsource.FSH.Modules.Organization.Features.v1.OrganizationUnits.Specifications;

public sealed class OrganizationUnitByAncestorSpec : Specification<OrganizationUnit>
{
    public OrganizationUnitByAncestorSpec(string ancestorPath)
    {
        Where(ou => ou.Path.StartsWith(ancestorPath));
        OrderBy(ou => ou.Path);
    }
    
    public static OrganizationUnitByRefSpec Create(string ancestorPath) => new(ancestorPath);
}