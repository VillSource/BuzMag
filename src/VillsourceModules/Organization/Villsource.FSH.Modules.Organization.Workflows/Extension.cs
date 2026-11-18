using Elsa.Extensions;
using Elsa.Features.Services;
using Elsa.Workflows;
using System.Diagnostics.CodeAnalysis;

namespace Villsource.FSH.Modules.Organization.Workflows;

public static class Extension
{
    [SuppressMessage("Major Code Smell", "S125:Sections of code should not be commented out")]
    public static IModule AddOrganizationWorkflowsModule(this IModule elsa)
    {
        ArgumentNullException.ThrowIfNull(elsa);
        
        // elsa.Services.AddScoped<IPropertyUIHandler, PositionTierOptionsProvider>();
        //
        // elsa.AddWorkflowsFrom<HumanResourceWorkflowMarker>();
        // elsa.AddActivitiesFrom<HumanResourceWorkflowMarker>();
        
        return elsa;
    }
}