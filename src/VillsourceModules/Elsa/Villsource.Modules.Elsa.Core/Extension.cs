using Elsa.Extensions;
using Elsa.Features.Services;
using Elsa.Workflows;
using Microsoft.Extensions.DependencyInjection;
using Villsource.Modules.Elsa.Core.Activities.OptionsProviders;
using Villsource.Modules.Elsa.Core.Modifiers;

namespace Villsource.Modules.Elsa.Core;

public static class Extension
{
    public static IModule AddCoreWorkflowsModule(this IModule elsa)
    {
        ArgumentNullException.ThrowIfNull(elsa);

        elsa.Services.AddSingleton<IActivityDescriptorModifier, TypedOutputDescriptionModifier>();
        elsa.Services.AddScoped<IPropertyUIHandler, ComparisonOperatorOptionsProvider>();
        elsa.AddActivitiesFrom<ElsaCoreWorkflowMarker>();

        return elsa;
    }
}