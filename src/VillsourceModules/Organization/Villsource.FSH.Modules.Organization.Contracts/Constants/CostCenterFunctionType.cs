
using Thinktecture;

namespace Villsource.FSH.Modules.Organization.Contracts.Constants;

/// <summary>
/// Functional cost classification aligned with IAS 1 / TFRS 1 (Classification of Expenses by Function)
/// and IAS 38 / TFRS 38 for R and D expenditure tracking.
/// </summary>
[SmartEnum<string>]
public sealed partial class CostCenterFunctionType
{
    public static readonly CostCenterFunctionType CostOfSales = new(
        key: "COST_OF_SALES",
        displayName: "Cost of Sales & Production",
        financialStatementLine: "COGS",
        isDirectOperatingCost: true,
        requiresPeriodEndAllocation: false);

    public static readonly CostCenterFunctionType SellingAndDistribution = new(
        key: "SELLING_AND_DISTRIBUTION",
        displayName: "Selling & Distribution Expenses",
        financialStatementLine: "OPEX_SELLING",
        isDirectOperatingCost: false,
        requiresPeriodEndAllocation: false);

    public static readonly CostCenterFunctionType Administrative = new(
        key: "ADMINISTRATIVE",
        displayName: "General & Administrative Expenses",
        financialStatementLine: "OPEX_ADMIN",
        isDirectOperatingCost: false,
        requiresPeriodEndAllocation: false);

    public static readonly CostCenterFunctionType ResearchAndDevelopment = new(
        key: "RESEARCH_AND_DEVELOPMENT",
        displayName: "Research & Development Expenses",
        financialStatementLine: "OPEX_RND",
        isDirectOperatingCost: false,
        requiresPeriodEndAllocation: false);

    public static readonly CostCenterFunctionType SharedServices = new(
        key: "SHARED_SERVICES",
        displayName: "Shared Services / Cost Allocation Pool",
        financialStatementLine: "ALLOCATION_POOL",
        isDirectOperatingCost: false,
        requiresPeriodEndAllocation: true);

    public string DisplayName { get; }
    public string FinancialStatementLine { get; }
    public bool IsDirectOperatingCost { get; }
    public bool RequiresPeriodEndAllocation { get; }


    public static string AllowedKeysDescription =>
        string.Join(", ", Items.Select(i => i.Key));

    public static CostCenterFunctionType FromKey(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        var normalized = key.Trim().ToUpperInvariant();

        if (!TryGet(normalized, out var result))
        {
            throw new ArgumentException(
                $"Invalid cost center function type '{key}'. Allowed values: {AllowedKeysDescription}.",
                nameof(key));
        }

        return result;
    }

    public static bool IsValidKey(string? key) =>
        !string.IsNullOrWhiteSpace(key) && TryGet(key.Trim().ToUpperInvariant(), out _);
}