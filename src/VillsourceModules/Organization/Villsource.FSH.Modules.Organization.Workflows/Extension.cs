using Elsa.Extensions;
using Elsa.Features.Services;
using Elsa.Workflows;
using Microsoft.Extensions.DependencyInjection;
using Villsource.FSH.Modules.Organization.Contracts.Services.Dtos;
using Villsource.FSH.Modules.Organization.Workflows.Activities.OptionsProviders;
using Villsource.FSH.Modules.Organization.Workflows.Constants;

namespace Villsource.FSH.Modules.Organization.Workflows;

public static class Extension
{
    public static IModule AddOrganizationWorkflowsModule(this IModule elsa)
    {
        ArgumentNullException.ThrowIfNull(elsa);

        elsa.Services.AddScoped<IPropertyUIHandler, JobLevelCategoryOptionsProvider>();
        elsa.Services.AddScoped<IPropertyUIHandler, DelegationTypeOptionsProvider>();

        // Register Organization DTOs and enums with Elsa for variable types and JS expression autocomplete.
        elsa.AddVariableTypeAndAlias<ChainOfCommandPositionDto>(nameof(ChainOfCommandPositionDto), OrganizationActivityConstants.Category);
        elsa.AddVariableTypeAndAlias<OuManagerResolutionDto>(nameof(OuManagerResolutionDto), OrganizationActivityConstants.Category);
        elsa.AddVariableTypeAndAlias<OuLeadershipStepDto>(nameof(OuLeadershipStepDto), OrganizationActivityConstants.Category);
        elsa.AddVariableTypeAndAlias<ResolvedDelegationDto>(nameof(ResolvedDelegationDto), OrganizationActivityConstants.Category);
        elsa.AddVariableTypeAndAlias<EffectiveManagerResolutionDto>(nameof(EffectiveManagerResolutionDto), OrganizationActivityConstants.Category);
        elsa.AddVariableTypeAndAlias<ManagerResolutionSource>(nameof(ManagerResolutionSource), OrganizationActivityConstants.Category);

        elsa.AddWorkflowsFrom<OrganizationWorkflowMarker>();
        elsa.AddActivitiesFrom<OrganizationWorkflowMarker>();

        return elsa;
    }
}
