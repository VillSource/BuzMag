using Villsource.Modules.HumanResource.Workflow.Contract.Dtos;
using Villsource.Modules.HumanResource.Workflow.Contract.Services;

namespace Villsource.Modules.HumanResource.Workflow.Services;

public class EmployeeResolverStub : IEmployeeResolver
{
    public Task<EmployeePosition> GetPosition(string employeeRef)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(employeeRef);
        
        return Task.FromResult(new EmployeePosition(
            OrganizationUnitRef: $"OU-{employeeRef}",
            PositionRef: $"PO-{employeeRef}",
            PositionTierName: $"PT-{employeeRef}",
            PositionTier: employeeRef.Length));
    }
}