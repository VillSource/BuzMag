using System.Text.Json;
using Elsa.Workflows.Activities;
using Elsa.Workflows.Memory;
using Elsa.Workflows.Models;
using Elsa.Workflows.UIHints;
using Villsource.FSH.Modules.Organization.Contracts.Services.Dtos;
using Villsource.FSH.Modules.Organization.Workflows.Activities.ChainOfCommand;
using Villsource.FSH.Modules.Organization.Workflows.Constants;

namespace Villsource.FSH.Modules.Organization.Tests.Workflows;

public sealed class ChainOfCommandPositionFieldSelectorTests
{
    [Fact]
    public async Task Registry_Should_Expose_ChainOfCommandPositionFieldSelector_With_Only_Position_Input_And_All_Outputs()
    {
        await using var sp = WorkflowTestHost.BuildServiceProvider();
        var registry = await WorkflowTestHost.PopulateRegistryAsync(sp);

        var descriptor = registry.Find(d => d.ClrType == typeof(ChainOfCommandPositionFieldSelector));
        descriptor.ShouldNotBeNull();
        descriptor.Category.ShouldBe(OrganizationActivityConstants.Category);
        descriptor.DisplayName.ShouldBe("Chain Of Command Position Field Selector");

        descriptor.Inputs.Count.ShouldBe(1);
        var positionInput = descriptor.Inputs.Single();
        positionInput.Name.ShouldBe(nameof(ChainOfCommandPositionFieldSelector.Position));
        positionInput.Type.ShouldBe(typeof(object));
        positionInput.UIHint.ShouldBe(InputUIHints.SingleLine);

        var dtoProperties = typeof(ChainOfCommandPositionDto)
            .GetProperties()
            .Select(p => p.Name)
            .ToArray();

        var outputNames = descriptor.Outputs.Select(o => o.Name).ToHashSet(StringComparer.Ordinal);
        outputNames.ShouldContain("Result");
        foreach (var propertyName in dtoProperties)
        {
            outputNames.ShouldContain(propertyName);
        }
    }

    [Fact]
    public async Task Execute_Should_Bind_Multiple_Outputs_To_Variables()
    {
        await using var sp = WorkflowTestHost.BuildServiceProvider();
        var position = WorkflowTestHost.CreateSamplePosition();

        var positionVar = new Variable<ChainOfCommandPositionDto?>("pos", position);
        var refIdVar = new Variable<string?>("refId", null);
        var limitVar = new Variable<decimal?>("limit", null);
        var rankVar = new Variable<int?>("rank", null);
        var isActiveVar = new Variable<bool?>("isActive", null);
        var titleVar = new Variable<string?>("title", null);

        string? capturedRefId = null;
        decimal? capturedLimit = null;
        int? capturedRank = null;
        bool? capturedIsActive = null;
        string? capturedTitle = null;

        var selector = new ChainOfCommandPositionFieldSelector
        {
            Id = "multi-selector",
            Position = new Input<object?>(positionVar),
            RefId = new Output<string?>(refIdVar),
            ApprovalLimitAmount = new Output<decimal?>(limitVar),
            JobGradeRankOrder = new Output<int?>(rankVar),
            IsActive = new Output<bool?>(isActiveVar),
            Title = new Output<string?>(titleVar)
        };

        var sequence = new Sequence
        {
            Variables = { positionVar, refIdVar, limitVar, rankVar, isActiveVar, titleVar },
            Activities =
            {
                selector,
                new Inline(ctx =>
                {
                    capturedRefId = refIdVar.Get(ctx);
                    capturedLimit = limitVar.Get(ctx);
                    capturedRank = rankVar.Get(ctx);
                    capturedIsActive = isActiveVar.Get(ctx);
                    capturedTitle = titleVar.Get(ctx);
                    return ValueTask.CompletedTask;
                })
            }
        };

        var runResult = await WorkflowTestHost.RunActivityAsync(sp, sequence);
        runResult.WorkflowState.Incidents.ShouldBeEmpty();
        capturedRefId.ShouldBe(position.RefId);
        capturedLimit.ShouldBe(position.ApprovalLimitAmount);
        capturedRank.ShouldBe(position.JobGradeRankOrder);
        capturedIsActive.ShouldBe(position.IsActive);
        capturedTitle.ShouldBe(position.Title);
    }

    [Fact]
    public async Task Execute_Should_Output_All_Fields_Of_ChainOfCommandPositionDto()
    {
        await using var sp = WorkflowTestHost.BuildServiceProvider();
        var position = WorkflowTestHost.CreateSamplePosition();

        var expectedByField = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            [nameof(ChainOfCommandPositionDto.RefId)] = position.RefId,
            [nameof(ChainOfCommandPositionDto.Code)] = position.Code,
            [nameof(ChainOfCommandPositionDto.Title)] = position.Title,
            [nameof(ChainOfCommandPositionDto.InternationalTitle)] = position.InternationalTitle,
            [nameof(ChainOfCommandPositionDto.LegalEntityRefId)] = position.LegalEntityRefId,
            [nameof(ChainOfCommandPositionDto.OrgUnitRefId)] = position.OrgUnitRefId,
            [nameof(ChainOfCommandPositionDto.OrgUnitCode)] = position.OrgUnitCode,
            [nameof(ChainOfCommandPositionDto.OrgUnitName)] = position.OrgUnitName,
            [nameof(ChainOfCommandPositionDto.JobCode)] = position.JobCode,
            [nameof(ChainOfCommandPositionDto.JobGradeCode)] = position.JobGradeCode,
            [nameof(ChainOfCommandPositionDto.JobGradeRankOrder)] = position.JobGradeRankOrder,
            [nameof(ChainOfCommandPositionDto.JobLevelCategory)] = position.JobLevelCategory,
            [nameof(ChainOfCommandPositionDto.ApprovalLimitAmount)] = position.ApprovalLimitAmount,
            [nameof(ChainOfCommandPositionDto.ApprovalLimitCurrency)] = position.ApprovalLimitCurrency,
            [nameof(ChainOfCommandPositionDto.IsPrimaryLeadership)] = position.IsPrimaryLeadership,
            [nameof(ChainOfCommandPositionDto.IsActive)] = position.IsActive,
            [nameof(ChainOfCommandPositionDto.ValidFrom)] = position.ValidFrom,
            [nameof(ChainOfCommandPositionDto.ValidTo)] = position.ValidTo,
        };

        var selector = new ChainOfCommandPositionFieldSelector
        {
            Id = "all-fields-selector",
            Position = new Input<object?>(position)
        };

        var (runResult, resultDict) = await WorkflowTestHost.RunCodeActivityAsync(sp, selector);
        resultDict.ShouldNotBeNull();
        resultDict.Count.ShouldBe(expectedByField.Count);

        foreach (var (fieldName, expectedValue) in expectedByField)
        {
            resultDict[fieldName].ShouldBe(expectedValue, $"Dictionary mismatch for field '{fieldName}'.");
            var outputValue = WorkflowTestHost.GetOutput<object?>(runResult, selector, fieldName);
            outputValue.ShouldBe(expectedValue, $"Output mismatch for field '{fieldName}'.");
        }
    }

    [Fact]
    public async Task Execute_Should_Output_Null_Values_For_All_Fields_When_Position_Is_Null()
    {
        await using var sp = WorkflowTestHost.BuildServiceProvider();

        var nullPosSelector = new ChainOfCommandPositionFieldSelector
        {
            Id = "null-pos",
            Position = new Input<object?>((object?)null)
        };

        var expectedFieldNames = typeof(ChainOfCommandPositionDto)
            .GetProperties()
            .Select(p => p.Name)
            .ToArray();

        var (nullRun, nullDict) = await WorkflowTestHost.RunCodeActivityAsync(sp, nullPosSelector);
        nullDict.ShouldNotBeNull();
        nullDict.Count.ShouldBe(expectedFieldNames.Length);

        foreach (var fieldName in expectedFieldNames)
        {
            nullDict.ContainsKey(fieldName).ShouldBeTrue();
            nullDict[fieldName].ShouldBeNull();
            WorkflowTestHost.GetOutput<object?>(nullRun, nullPosSelector, fieldName).ShouldBeNull();
        }
    }

    [Fact]
    public async Task Execute_Should_Bind_Nullable_Output_To_NonNullable_Variable()
    {
        await using var sp = WorkflowTestHost.BuildServiceProvider();
        var position = WorkflowTestHost.CreateSamplePosition(jobGradeRankOrder: 42, isActive: true);

        var rankIntVar = new Variable<int>("rankInt", 0);
        var activeBoolVar = new Variable<bool>("activeBool", false);
        int capturedRank = 0;
        bool capturedActive = false;

        var selector = new ChainOfCommandPositionFieldSelector
        {
            Position = new Input<object?>(position),
            JobGradeRankOrder = new Output<int?>(rankIntVar),
            IsActive = new Output<bool?>(activeBoolVar)
        };

        var sequence = new Sequence
        {
            Variables = { rankIntVar, activeBoolVar },
            Activities =
            {
                selector,
                new Inline(ctx =>
                {
                    capturedRank = rankIntVar.Get(ctx);
                    capturedActive = activeBoolVar.Get(ctx);
                    return ValueTask.CompletedTask;
                })
            }
        };

        var runResult = await WorkflowTestHost.RunActivityAsync(sp, sequence);
        runResult.WorkflowState.Incidents.ShouldBeEmpty();
        capturedRank.ShouldBe(42);
        capturedActive.ShouldBeTrue();
    }

    [Fact]
    public async Task Execute_Should_Parse_Json_String_Input_And_Bind_To_Outputs()
    {
        await using var sp = WorkflowTestHost.BuildServiceProvider();
        var position = WorkflowTestHost.CreateSamplePosition(refId: "POS-JSON", code: "JSON-01", jobGradeRankOrder: 77);
        var json = JsonSerializer.Serialize(position, JsonSerializerOptions.Web);

        var jsonVar = new Variable<string?>("posJson", json);
        var refIdVar = new Variable<string?>("refId", null);
        var codeVar = new Variable<string?>("code", null);
        var rankVar = new Variable<int?>("rank", null);

        string? capturedRefId = null;
        string? capturedCode = null;
        int? capturedRank = null;

        var sequence = new Sequence
        {
            Variables = { jsonVar, refIdVar, codeVar, rankVar },
            Activities =
            {
                new ChainOfCommandPositionFieldSelector
                {
                    Position = new Input<object?>(jsonVar),
                    RefId = new Output<string?>(refIdVar),
                    Code = new Output<string?>(codeVar),
                    JobGradeRankOrder = new Output<int?>(rankVar)
                },
                new Inline(ctx =>
                {
                    capturedRefId = refIdVar.Get(ctx);
                    capturedCode = codeVar.Get(ctx);
                    capturedRank = rankVar.Get(ctx);
                    return ValueTask.CompletedTask;
                })
            }
        };

        var runResult = await WorkflowTestHost.RunActivityAsync(sp, sequence);
        runResult.WorkflowState.Incidents.ShouldBeEmpty();
        capturedRefId.ShouldBe("POS-JSON");
        capturedCode.ShouldBe("JSON-01");
        capturedRank.ShouldBe(77);
    }
}
