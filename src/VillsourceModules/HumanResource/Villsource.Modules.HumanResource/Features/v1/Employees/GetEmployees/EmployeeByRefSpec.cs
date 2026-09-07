using FSH.Framework.Persistence.Specifications;
using Villsource.Modules.HumanResource.Domain;

namespace Villsource.Modules.HumanResource.Features.v1.Employees.GetEmployees;

public class EmployeeByRefSpec : Specification<Employee>
{
    public EmployeeByRefSpec(string @ref)
    {
        Where(e => e.Ref == @ref);
        OrderBy(e => e.Code);
    }
}