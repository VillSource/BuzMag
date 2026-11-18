using Elsa.Extensions;
using Elsa.Features.Services;
using Elsa.Workflows;
using Microsoft.Extensions.DependencyInjection;
using Villsource.FSH.Modules.Organization.Workflows.Activities.OptionsProviders;

namespace Villsource.FSH.Modules.Organization.Workflows;

public static class Extension
{
    public static IModule AddOrganizationWorkflowsModule(this IModule elsa)
    {
        ArgumentNullException.ThrowIfNull(elsa);

        elsa.Services.AddScoped<IPropertyUIHandler, JobLevelCategoryOptionsProvider>();
        elsa.Services.AddScoped<IPropertyUIHandler, DelegationTypeOptionsProvider>();

        elsa.AddWorkflowsFrom<OrganizationWorkflowMarker>();
        elsa.AddActivitiesFrom<OrganizationWorkflowMarker>();

        return elsa;
    }
}
