namespace Villsource.FSH.Modules.Organization.Contracts.v1.Positions.Dtos;

public sealed record PositionSummaryDto(
    string RefId,
    string LegalEntityRefId,
    string LegalEntityName,
    string? OrgUnitRefId,
    string JobCode,
    string JobGradeCode,
    int JobGradeRankOrder,
    string? CostCenterCode,
    string? ReportsToPositionRefId,
    string? ReportsToPositionCode,
    string Code,
    string Title,
    string InternationalTitle,
    int HeadcountCapacity,
    bool IsPrimaryLeadership,
    bool IsActive,
    int ActiveDirectReportCount);