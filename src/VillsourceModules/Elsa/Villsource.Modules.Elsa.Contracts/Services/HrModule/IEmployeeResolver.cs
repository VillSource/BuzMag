namespace Villsource.Modules.Elsa.Contracts.Services.HrModule;

public record EmployeePosition(
    string OrganizationUnitRef,
    string PositionRef,
    string PositionTierName,
    int PositionTier);

public interface IEmployeeResolver
{
    Task<EmployeePosition> GetPosition(string employeeRef);
}

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