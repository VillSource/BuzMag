namespace Villsource.FSH.Modules.Organization.Contracts.v1.CostCenters;

public sealed record CostCenterNodeDto(
    string RefId,
    string Code,
    string Name,
    string InternationalName,
    string FunctionType,
    bool IsActive,
    bool IsPostingBlocked);