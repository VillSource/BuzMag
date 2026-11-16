namespace Villsource.FSH.Modules.Organization.Contracts.v1.CostCenters;

public sealed record CostCenterFunctionTypeDto(
    string Key,
    string DisplayName,
    string FinancialStatementLine,
    bool IsDirectOperatingCost,
    bool RequiresPeriodEndAllocation);