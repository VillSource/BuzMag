using System.Collections.Frozen;
using Elsa.Expressions.Models;
using Elsa.Workflows;
using Elsa.Workflows.Models;
using Villsource.FSH.Modules.Organization.Contracts.Services.Dtos;
using Villsource.FSH.Modules.Organization.Workflows.Activities.ChainOfCommand;

namespace Villsource.FSH.Modules.Organization.Workflows.Constants;

internal sealed record PositionFieldDefinition(
    string Name,
    string DisplayText,
    Func<ChainOfCommandPositionDto, object?> Accessor,
    Action<ChainOfCommandPositionFieldSelector, ActivityExecutionContext, object?, string> OutputSetter);

internal static class ChainOfCommandPositionFields
{
    public static IReadOnlyList<PositionFieldDefinition> All { get; } =
    [
        new(nameof(ChainOfCommandPositionDto.RefId), "RefId (string)", static p => p.RefId, static (s, c, v, n) => SetOutput(s.RefId, c, v, n)),
        new(nameof(ChainOfCommandPositionDto.Code), "Code (string)", static p => p.Code, static (s, c, v, n) => SetOutput(s.Code, c, v, n)),
        new(nameof(ChainOfCommandPositionDto.Title), "Title (string)", static p => p.Title, static (s, c, v, n) => SetOutput(s.Title, c, v, n)),
        new(nameof(ChainOfCommandPositionDto.InternationalTitle), "InternationalTitle (string)", static p => p.InternationalTitle, static (s, c, v, n) => SetOutput(s.InternationalTitle, c, v, n)),
        new(nameof(ChainOfCommandPositionDto.LegalEntityRefId), "LegalEntityRefId (string)", static p => p.LegalEntityRefId, static (s, c, v, n) => SetOutput(s.LegalEntityRefId, c, v, n)),
        new(nameof(ChainOfCommandPositionDto.OrgUnitRefId), "OrgUnitRefId (string)", static p => p.OrgUnitRefId, static (s, c, v, n) => SetOutput(s.OrgUnitRefId, c, v, n)),
        new(nameof(ChainOfCommandPositionDto.OrgUnitCode), "OrgUnitCode (string)", static p => p.OrgUnitCode, static (s, c, v, n) => SetOutput(s.OrgUnitCode, c, v, n)),
        new(nameof(ChainOfCommandPositionDto.OrgUnitName), "OrgUnitName (string)", static p => p.OrgUnitName, static (s, c, v, n) => SetOutput(s.OrgUnitName, c, v, n)),
        new(nameof(ChainOfCommandPositionDto.JobCode), "JobCode (string)", static p => p.JobCode, static (s, c, v, n) => SetOutput(s.JobCode, c, v, n)),
        new(nameof(ChainOfCommandPositionDto.JobGradeCode), "JobGradeCode (string)", static p => p.JobGradeCode, static (s, c, v, n) => SetOutput(s.JobGradeCode, c, v, n)),
        new(nameof(ChainOfCommandPositionDto.JobGradeRankOrder), "JobGradeRankOrder (int)", static p => p.JobGradeRankOrder, static (s, c, v, n) => SetOutput(s.JobGradeRankOrder, c, v, n)),
        new(nameof(ChainOfCommandPositionDto.JobLevelCategory), "JobLevelCategory (string?)", static p => p.JobLevelCategory, static (s, c, v, n) => SetOutput(s.JobLevelCategory, c, v, n)),
        new(nameof(ChainOfCommandPositionDto.ApprovalLimitAmount), "ApprovalLimitAmount (decimal?)", static p => p.ApprovalLimitAmount, static (s, c, v, n) => SetOutput(s.ApprovalLimitAmount, c, v, n)),
        new(nameof(ChainOfCommandPositionDto.ApprovalLimitCurrency), "ApprovalLimitCurrency (string?)", static p => p.ApprovalLimitCurrency, static (s, c, v, n) => SetOutput(s.ApprovalLimitCurrency, c, v, n)),
        new(nameof(ChainOfCommandPositionDto.IsPrimaryLeadership), "IsPrimaryLeadership (bool)", static p => p.IsPrimaryLeadership, static (s, c, v, n) => SetOutput(s.IsPrimaryLeadership, c, v, n)),
        new(nameof(ChainOfCommandPositionDto.IsActive), "IsActive (bool)", static p => p.IsActive, static (s, c, v, n) => SetOutput(s.IsActive, c, v, n)),
        new(nameof(ChainOfCommandPositionDto.ValidFrom), "ValidFrom (DateTimeOffset?)", static p => p.ValidFrom, static (s, c, v, n) => SetOutput(s.ValidFrom, c, v, n)),
        new(nameof(ChainOfCommandPositionDto.ValidTo), "ValidTo (DateTimeOffset?)", static p => p.ValidTo, static (s, c, v, n) => SetOutput(s.ValidTo, c, v, n)),
    ];

    private static readonly FrozenDictionary<string, PositionFieldDefinition> DefinitionMap =
        All.ToFrozenDictionary(static f => f.Name, static f => f, StringComparer.OrdinalIgnoreCase);

    public static bool TryGetDefinition(
        string? fieldName,
        out PositionFieldDefinition? definition)
    {
        if (string.IsNullOrWhiteSpace(fieldName))
        {
            definition = null;
            return false;
        }

        return DefinitionMap.TryGetValue(fieldName.Trim(), out definition);
    }

    private static void SetOutput<T>(
        Output<T>? output,
        ActivityExecutionContext context,
        object? value,
        string outputName)
    {
        T typedValue = value is null ? default! : (T)value;
        MemoryBlockReference? blockReference = output?.MemoryBlockReference?.Invoke();

        if (blockReference is not null)
        {
            context.Set(blockReference, typedValue);
        }

        context.WorkflowExecutionContext.GetActivityOutputRegister().Record(context, outputName, typedValue);
    }
}
