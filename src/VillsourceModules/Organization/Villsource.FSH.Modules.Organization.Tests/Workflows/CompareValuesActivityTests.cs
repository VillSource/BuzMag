using System.Text.Json;
using Elsa.Workflows.Memory;
using Elsa.Workflows.Models;
using Elsa.Workflows.UIHints;
using Villsource.FSH.Modules.Organization.Contracts.Services.Dtos;
using Villsource.Modules.Elsa.Core.Activities;
using Villsource.Modules.Elsa.Core.Constants;

namespace Villsource.FSH.Modules.Organization.Tests.Workflows;

public sealed class CompareValuesActivityTests
{
    [Fact]
    public async Task CoreModule_Should_Register_CompareValues_With_Dropdown_Operators()
    {
        await using var sp = WorkflowTestHost.BuildServiceProvider();
        var registry = await WorkflowTestHost.PopulateRegistryAsync(sp);

        var descriptor = registry.Find(d => d.ClrType == typeof(CompareValues));
        descriptor.ShouldNotBeNull();
        descriptor.Category.ShouldBe(CoreActivityConstants.Category);
        descriptor.DisplayName.ShouldBe("Compare Values");

        var opInput = descriptor.Inputs.Single(i => i.Name == nameof(CompareValues.Operator));
        opInput.UIHint.ShouldBe(InputUIHints.DropDown);
        opInput.DefaultValue.ShouldBe("==");

        var selectList = WorkflowTestHost.GetDropDownSelectList(opInput);
        selectList.Items.Select(i => (i.Text, i.Value)).ShouldBe(
        [
            ("Equal", "=="),
            ("Not Equal", "!="),
            ("Greater Than", ">"),
            ("Less Than", "<"),
            ("Greater Than or Equal", ">="),
            ("Less Than or Equal", "<=")
        ]);

        descriptor.Outputs.Select(o => o.Name).ShouldContain("Result");
    }

    [Theory]
    [InlineData(10, "==", 10, true)]
    [InlineData(10, "==", 20, false)]
    [InlineData(10, "!=", 20, true)]
    [InlineData(10, ">", 5, true)]
    [InlineData(10, ">", 10, false)]
    [InlineData(5, "<", 10, true)]
    [InlineData(10, "<", 10, false)]
    [InlineData(10, ">=", 10, true)]
    [InlineData(9, ">=", 10, false)]
    [InlineData(10, "<=", 10, true)]
    [InlineData(11, "<=", 10, false)]
    public async Task CompareValues_Should_Evaluate_Numeric_Comparisons(int left, string op, int right, bool expected)
    {
        await using var sp = WorkflowTestHost.BuildServiceProvider();
        var (_, actual) = await WorkflowTestHost.RunCodeActivityAsync(
            sp,
            new CompareValues
            {
                Left = new Input<object?>(left),
                Operator = new Input<string>(op),
                Right = new Input<object?>(right)
            });

        actual.ShouldBe(expected);
    }

    [Theory]
    [InlineData(100000, ">=", "50000", true)]
    [InlineData(100000, ">", "100000", false)]
    [InlineData(100000, "==", "100000.00", true)]
    [InlineData("10", ">", "2", true)]
    [InlineData("001", "==", "1", false)]
    [InlineData("MANAGEMENT", "==", "MANAGEMENT", true)]
    [InlineData("MANAGEMENT", "!=", "OPERATIONAL", true)]
    [InlineData(true, "==", "true", true)]
    [InlineData(true, "!=", "false", true)]
    [InlineData(true, ">", false, false)]
    [InlineData(null, "==", null, true)]
    [InlineData(null, "!=", null, false)]
    [InlineData(null, "!=", "abc", true)]
    [InlineData(null, ">", 0, false)]
    [InlineData(10, "INVALID", 10, false)]
    public async Task CompareValues_Should_Handle_Mixed_Strings_Booleans_Nulls_And_Operators(
        object? left,
        string op,
        object? right,
        bool expected)
    {
        await using var sp = WorkflowTestHost.BuildServiceProvider();
        var (_, actual) = await WorkflowTestHost.RunCodeActivityAsync(
            sp,
            new CompareValues
            {
                Left = new Input<object?>(left),
                Operator = new Input<string>(op),
                Right = new Input<object?>(right)
            });

        actual.ShouldBe(expected);
    }

    [Fact]
    public async Task CompareValues_Should_Handle_Decimal_Double_DateTimeOffset_Enum_Guid_And_JsonElement()
    {
        await using var sp = WorkflowTestHost.BuildServiceProvider();

        var (_, decimalVsDouble) = await WorkflowTestHost.RunCodeActivityAsync(sp, new CompareValues
        {
            Left = new Input<object?>(100_000m),
            Operator = new Input<string>(">="),
            Right = new Input<object?>(99_999.5d)
        });
        decimalVsDouble.ShouldBeTrue();

        var date = new DateTimeOffset(2026, 11, 18, 12, 0, 0, TimeSpan.Zero);
        var (_, dateVsIso) = await WorkflowTestHost.RunCodeActivityAsync(sp, new CompareValues
        {
            Left = new Input<object?>(date),
            Operator = new Input<string>("=="),
            Right = new Input<object?>("2026-11-18T12:00:00Z")
        });
        dateVsIso.ShouldBeTrue();

        var (_, dateEarlier) = await WorkflowTestHost.RunCodeActivityAsync(sp, new CompareValues
        {
            Left = new Input<object?>(date),
            Operator = new Input<string>(">"),
            Right = new Input<object?>("2026-11-18T11:59:59Z")
        });
        dateEarlier.ShouldBeTrue();

        var (_, enumVsString) = await WorkflowTestHost.RunCodeActivityAsync(sp, new CompareValues
        {
            Left = new Input<object?>(ManagerResolutionSource.DirectReporting),
            Operator = new Input<string>("=="),
            Right = new Input<object?>("directreporting")
        });
        enumVsString.ShouldBeTrue();

        var guid = Guid.Parse("11111111-2222-3333-4444-555555555555");
        var (_, guidVsString) = await WorkflowTestHost.RunCodeActivityAsync(sp, new CompareValues
        {
            Left = new Input<object?>(guid),
            Operator = new Input<string>("=="),
            Right = new Input<object?>("11111111-2222-3333-4444-555555555555")
        });
        guidVsString.ShouldBeTrue();

        using var doc = JsonDocument.Parse("{\"limit\": 50000, \"active\": true}");
        var jsonNum = doc.RootElement.GetProperty("limit");
        var jsonBool = doc.RootElement.GetProperty("active");

        var (_, jsonCompare) = await WorkflowTestHost.RunCodeActivityAsync(sp, new CompareValues
        {
            Left = new Input<object?>(jsonNum),
            Operator = new Input<string>(">"),
            Right = new Input<object?>(10_000m)
        });
        jsonCompare.ShouldBeTrue();

        var (_, jsonBoolCompare) = await WorkflowTestHost.RunCodeActivityAsync(sp, new CompareValues
        {
            Left = new Input<object?>(jsonBool),
            Operator = new Input<string>("=="),
            Right = new Input<object?>(true)
        });
        jsonBoolCompare.ShouldBeTrue();
    }
}
