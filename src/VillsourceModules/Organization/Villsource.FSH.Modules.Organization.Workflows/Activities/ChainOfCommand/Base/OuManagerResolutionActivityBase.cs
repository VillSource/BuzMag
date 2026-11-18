using System.Text.Json.Nodes;
using Elsa.Extensions;
using Elsa.Workflows;
using Elsa.Workflows.Attributes;
using Elsa.Workflows.Models;
using Villsource.FSH.Modules.Organization.Contracts.Services.Dtos;
using Villsource.Modules.Elsa.Core.Activities.Base;
using Villsource.Modules.Elsa.Core.Helpers;

namespace Villsource.FSH.Modules.Organization.Workflows.Activities.ChainOfCommand.Base;

/// <summary>
/// Base activity for Organization Unit manager resolvers returning <see cref="OuManagerResolutionDto"/> as a <see cref="JsonObject"/>
/// and exposing its <see cref="ManagerPosition"/> as a <see cref="JsonObject"/> for downstream activities.
/// </summary>
public abstract class OuManagerResolutionActivityBase : JsonObjectActivityBase
{
    [Output(Description = "The resolved manager position as a JsonObject (null if no manager was found).")]
    public Output<JsonObject?> ManagerPosition { get; set; } = default!;

    [Output(Description = "Reference ID of the Organization Unit where the manager was found.")]
    public Output<string?> ResolvedOrgUnitRefId { get; set; } = default!;

    [Output(Description = "Code of the Organization Unit where the manager was found.")]
    public Output<string?> ResolvedOrgUnitCode { get; set; } = default!;

    [Output(Description = "Name of the Organization Unit where the manager was found.")]
    public Output<string?> ResolvedOrgUnitName { get; set; } = default!;

    [Output(Description = "Tree depth (0 = root) of the Organization Unit where the manager was found.")]
    public Output<int?> ResolvedOrgUnitDepth { get; set; } = default!;

    [Output(Description = "Number of parent OU levels climbed from the starting OU.")]
    public Output<int?> StepsCrawledUp { get; set; } = default!;

    protected void SetOutputs(ActivityExecutionContext context, OuManagerResolutionDto? resolution)
    {
        SetJsonResult(context, resolution);
        ManagerPosition.Set(context, WorkflowJsonHelper.ToJsonObject(resolution?.ManagerPosition));
        ResolvedOrgUnitRefId.Set(context, resolution?.ResolvedOrgUnitRefId);
        ResolvedOrgUnitCode.Set(context, resolution?.ResolvedOrgUnitCode);
        ResolvedOrgUnitName.Set(context, resolution?.ResolvedOrgUnitName);
        ResolvedOrgUnitDepth.Set(context, resolution?.ResolvedOrgUnitDepth);
        StepsCrawledUp.Set(context, resolution?.StepsCrawledUp);
    }
}
