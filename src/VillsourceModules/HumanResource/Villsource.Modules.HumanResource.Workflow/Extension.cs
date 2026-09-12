using Elsa.Extensions;
using Elsa.Features.Services;
using Microsoft.Extensions.DependencyInjection;
using Villsource.Modules.HumanResource.Workflow.Services;
using Villsource.Modules.HumanResource.Workflow.Contract.Services;

namespace Villsource.Modules.HumanResource.Workflow;

public static class Extension
{
    public static IModule AddHrWorkflowsModule(this IModule elsa)
    {
        ArgumentNullException.ThrowIfNull(elsa);
        
        elsa.Services.AddScoped<IEmployeeResolver, EmployeeResolverStub>();
        elsa.AddWorkflowsFrom<HumanResourceWorkflowMarker>();
        elsa.AddActivitiesFrom<HumanResourceWorkflowMarker>();
        
        return elsa;
    }
}