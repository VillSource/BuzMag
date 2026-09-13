using Elsa.Extensions;
using Elsa.Features.Services;
using Elsa.Workflows;
using Microsoft.Extensions.DependencyInjection;
using Villsource.Modules.HumanResource.Workflow.Activities.OptionsProviders;
using Villsource.Modules.HumanResource.Workflow.Services;
using Villsource.Modules.HumanResource.Workflow.Contract.Services;

namespace Villsource.Modules.HumanResource.Workflow;

public static class Extension
{
    public static IModule AddHrWorkflowsModule(this IModule elsa)
    {
        ArgumentNullException.ThrowIfNull(elsa);
        
        elsa.Services.AddScoped<IEmployeeResolver, EmployeeResolverStub>();
        elsa.Services.AddScoped<IPropertyUIHandler, PositionTierOptionsProvider>();
        
        elsa.AddWorkflowsFrom<HumanResourceWorkflowMarker>();
        elsa.AddActivitiesFrom<HumanResourceWorkflowMarker>();
        
        return elsa;
    }
}