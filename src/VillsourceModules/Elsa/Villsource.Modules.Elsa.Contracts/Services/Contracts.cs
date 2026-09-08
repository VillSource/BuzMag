namespace Villsource.Modules.Elsa.Contracts.Services;

public record EmployeeInfo(string EmployeeId, string Name, string PositionTierName, int PositionTierValue);

public interface IEmployeeResolver
{
    Task<EmployeeInfo?> GetEmployeeAsync(string employeeId, CancellationToken ct);
    Task<EmployeeInfo?> GetPrimaryManagerAsync(string employeeId, CancellationToken ct);
}

public interface INotificationService
{
    Task SendApprovalTaskAsync(string employeeId, string docNo, CancellationToken ct);
    Task NotifyHrEscalationAsync( string docNo,string reason, CancellationToken ct);
}

public interface IDocumentCallback
{
    Task UpdateStatusAsync(string docNo, string status, bool isFinish, CancellationToken ct);
}