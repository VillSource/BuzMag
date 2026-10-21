
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Villsource.Modules.Elsa.Contracts.Services;

namespace Villsource.Modules.Elsa.Services;

public partial class StubEmployeeResolver(ILogger<StubEmployeeResolver> logger) : IEmployeeResolver
{
    public Task<EmployeeInfo?> GetEmployeeAsync(string employeeId, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(employeeId);
        LogStubGettingEmployeeInfoForEmployeeid(employeeId);
        
        // จำลอง Tier ตามรหัสพนักงาน (อ้างอิงจาก Scenario 2)
        var tier = employeeId.Contains("DIRECTOR", StringComparison.OrdinalIgnoreCase) ? "DIRECTOR" : "STAFF";
        
        return Task.FromResult<EmployeeInfo?>(new EmployeeInfo(employeeId, $"Mock {employeeId}", tier, 4));
    }

    public Task<EmployeeInfo?> GetPrimaryManagerAsync(string employeeId, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(employeeId);
        LogStubResolvingManagerForEmployeeid(employeeId);
        
        // จำลองกรณีไม่มี Manager (อ้างอิงจาก Scenario 7)
        if (employeeId.Contains("NO-MANAGER", StringComparison.OrdinalIgnoreCase)) 
        {
            logger.LogWarning("[Stub] Manager not found for {EmployeeId}", employeeId);
            return Task.FromResult<EmployeeInfo?>(null);
        }

        return Task.FromResult<EmployeeInfo?>(new EmployeeInfo($"MGR-OF-{employeeId}", "Mock Manager", "MANAGER",3));
    }

    [LoggerMessage(LogLevel.Information, "[Stub] Getting Employee Info for: {EmployeeId}")]
    partial void LogStubGettingEmployeeInfoForEmployeeid(string employeeId);

    [LoggerMessage(LogLevel.Information, "[Stub] Resolving Manager for: {EmployeeId}")]
    partial void LogStubResolvingManagerForEmployeeid(string employeeId);
}



public partial class StubNotificationService(ILogger<StubNotificationService> logger) : INotificationService
{
    public Task SendApprovalTaskAsync(string employeeId, string docNo, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(employeeId);
        ArgumentNullException.ThrowIfNull(docNo);
        
        LogStubSendApprovalTask(employeeId, docNo);
        
        return Task.CompletedTask;
    }

    public Task NotifyHrEscalationAsync(string docNo, string reason, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(docNo);
        ArgumentNullException.ThrowIfNull(reason);
        
        LogStubNotifyHrEscalation(docNo, reason);
        
        return Task.CompletedTask;
    }

    [LoggerMessage(LogLevel.Information, "[Stub Inbox] Send Approval Task to Manager {EmployeeId} for Doc: {DocNo}")]
    partial void LogStubSendApprovalTask(string employeeId, string docNo);

    [LoggerMessage(LogLevel.Warning, "[Stub HR] Escalation Alert! Doc: {DocNo}, Reason: {Reason}")]
    partial void LogStubNotifyHrEscalation(string docNo, string reason);
}



public partial class StubDocumentCallback(ILogger<StubDocumentCallback> logger) : IDocumentCallback
{
    public Task UpdateStatusAsync(string docNo, string status, bool isFinish, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(docNo);
        ArgumentNullException.ThrowIfNull(status);
        
        LogStubUpdateStatus(docNo, status, isFinish);
        
        return Task.CompletedTask;
    }

    [LoggerMessage(LogLevel.Information, "[Stub Database] Update {DocNo} -> Status: '{Status}' (IsFinished: {IsFinish})")]
    partial void LogStubUpdateStatus(string docNo, string status, bool isFinish);
}

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddElsaStubServices(this IServiceCollection services)
    {
        // 💡 ลงทะเบียน Service ทุกตัวด้วย Scoped (หรือ Transient/Singleton ตามความเหมาะสมของระบบ)
        services.AddScoped<IEmployeeResolver, StubEmployeeResolver>();
        services.AddScoped<INotificationService, StubNotificationService>();
        services.AddScoped<IDocumentCallback, StubDocumentCallback>();
        
        return services;
    }
}