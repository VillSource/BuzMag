using Elsa.Extensions;
using Elsa.Workflows;
using Elsa.Workflows.Activities;
using Elsa.Workflows.Activities.Flowchart.Activities;
using Elsa.Workflows.Activities.Flowchart.Models;
using Villsource.Modules.Elsa.Core.Workflows;
using Villsource.Modules.HumanResource.Workflow.Activities;
using Villsource.Modules.HumanResource.Workflow.Contract.Dtos;

namespace Villsource.Modules.HumanResource.Workflow.Workflows;

public class BuzMagLeaveRequest : BuzMagWorkflowBase
{
    protected override void Build(IWorkflowBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        base.Build(builder);

        builder.Name = "LeaveRequest";
        builder.Version = 7;

        builder.WithInput<string>("EmployeeId");
        
        var employeeId = builder.WithVariable<string>("EmployeeId");
        var employeePosition = builder.WithVariable<object>("EmployeePosition");
        
        var setEmployeePosition = new SetVariable
        {
            Name = "Save EmployeeId to variable",
            Variable = employeeId,
            Value = new(context => context.GetInput<string>("EmployeeId")),
        };

        var resolveEmployeePosition = new ResolveEmployeePosition
        {
            EmployeeId = new(employeeId), Result = new(employeePosition)
        };

        var showPosition = new WriteLine(context =>
        {
            var x = (EmployeePosition?)employeePosition.Get(context);
            return $"{x?.PositionRef} {x?.PositionTier}";
        });
        

        builder.Root = new Flowchart
        {
            Activities =
            {
                setEmployeePosition,
                resolveEmployeePosition,
                showPosition,
            },
            Connections =
            {
                new Connection(setEmployeePosition, resolveEmployeePosition),
                new Connection( resolveEmployeePosition, showPosition),
            },
        };
    }
}