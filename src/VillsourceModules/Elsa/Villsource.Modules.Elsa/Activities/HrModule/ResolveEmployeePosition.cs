using Elsa.Workflows;
using Elsa.Workflows.Attributes;
using Villsource.Modules.Elsa.Contracts.Services.HrModule;

namespace Villsource.Modules.Elsa.Activities.HrModule;

[Activity(
    Category = "BuzMag.Employee",
    Description = "Retrieves the organization unit, position, and level of an employee.",
    DisplayName = "Get Employee Position Details"
)]
public class ResolveEmployeePosition : CodeActivity
{
    protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var service = context.GetRequiredService<IEmployeeResolver>();
        _ = service;
    }
}