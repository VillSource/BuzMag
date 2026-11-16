namespace Villsource.FSH.Modules.Organization.Contracts.v1.CostCenters;

public sealed record CostCenterTaxBranchDto(
    string RefId,
    string BranchCode,
    string Name,
    bool IsHeadOffice);