using Elsa.Extensions;
using Elsa.Workflows;
using Elsa.Workflows.Attributes;
using Elsa.Workflows.Models;
using Villsource.Modules.HumanResource.Workflow.Contract.Constants;
using Villsource.Modules.HumanResource.Workflow.Contract.Dtos;
using Villsource.Modules.HumanResource.Workflow.Contract.Services;

namespace Villsource.Modules.HumanResource.Workflow.Activities;

[Activity(
    Category = HrActivityConstant.Category,
    Description = "Retrieves the organization unit, position, and level of an employee.",
    DisplayName = "Get Employee Position Details"
)]
public class ResolveEmployeePosition : CodeActivity<EmployeePosition>
{
    public Input<string> EmployeeId { get; init; } = null!;
    
    public Output<string> OrganizationUnitRef { get; set; } = default!;
    public Output<string> PositionRef { get; set; } = default!;
    public Output<string> PositionTierName { get; set; } = default!;
    public Output<int> PositionTier { get; set; } = default!;

    protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var employeeId = EmployeeId.Get(context);

        if (employeeId is null) return;

        var service = context.GetRequiredService<IEmployeeResolver>();
        var position = await service.GetPosition(employeeId);
        OrganizationUnitRef.Set(context, position.OrganizationUnitRef);
        PositionRef.Set(context, position.PositionRef);
        PositionTierName.Set(context, position.PositionTierName);
        PositionTier.Set(context, position.PositionTier);
        context.SetResult(position);
    }
}