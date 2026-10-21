using Villsource.Modules.HumanResource.Workflow.Contract.Dtos;

namespace Villsource.Modules.HumanResource.Workflow.Contract.Services;

public interface IEmployeeResolver
{
    Task<EmployeePosition> GetPosition(string employeeRef);
}