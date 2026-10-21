using Elsa.Extensions;
using Elsa.Features.Services;
using Elsa.Workflows;
using Microsoft.Extensions.DependencyInjection;
using Villsource.Modules.Elsa.Core.Activities.OptionsProviders;

namespace Villsource.Modules.Elsa.Core;

public static class Extension
{
    public static IModule AddCoreWorkflowsModule(this IModule elsa)
    {
        ArgumentNullException.ThrowIfNull(elsa);
        
        elsa.Services.AddScoped<IPropertyUIHandler, ComparisonOperatorOptionsProvider>();
        
        return elsa;
    }
}