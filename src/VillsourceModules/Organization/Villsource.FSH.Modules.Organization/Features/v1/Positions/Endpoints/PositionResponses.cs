namespace Villsource.FSH.Modules.Organization.Features.v1.Positions.Endpoints;

internal sealed record PositionSummaryResponse(
    string RefId,
    string LegalEntityRefId,
    string LegalEntityName,
    string? OrgUnitRefId,
    string JobCode,
    string JobGradeCode,
    int JobGradeRankOrder,
    string? CostCenterCode,
    string? ReportsToRefId,
    string? ReportsToCode,
    string Code,
    string Title,
    string IntlTitle,
    int Headcount,
    bool IsLeadership,
    bool IsActive,
    int DirectReportCount);

internal sealed record PositionDetailResponse(
    string RefId,
    string LegalEntityRefId,
    string LegalEntityCode,
    string LegalEntityName,
    string? OrgUnitRefId,
    PositionJobResponse Job,
    PositionJobGradeResponse JobGrade,
    PositionCostCenterResponse? CostCenter,
    PositionNodeResponse? ReportsTo,
    PositionNodeResponse? DottedLine,
    string Code,
    string Title,
    string IntlTitle,
    string? Description,
    string? TaxBranchRefId,
    decimal? ApprovalLimit,
    string? ApprovalCurrency,
    int Headcount,
    bool IsLeadership,
    DateTimeOffset? ValidFrom,
    DateTimeOffset? ValidTo,
    bool IsActive,
    IReadOnlyList<PositionNodeResponse> DirectReports,
    DateTimeOffset CreatedOnUtc,
    DateTimeOffset? LastModifiedOnUtc);

internal sealed record PositionLookupResponse(
    string RefId,
    string LegalEntityRefId,
    string? OrgUnitRefId,
    string Code,
    string Title,
    string IntlTitle,
    string JobGradeCode,
    int JobGradeRankOrder,
    bool IsLeadership);

internal sealed record PositionNodeResponse(
    string RefId,
    string Code,
    string Title,
    string IntlTitle,
    string JobGradeCode,
    int JobGradeRankOrder,
    bool IsLeadership,
    bool IsActive);

internal sealed record PositionVacancyResponse(
    string PositionRefId,
    string PositionCode,
    string Title,
    string? OrgUnitRefId,
    int Headcount,
    int FilledCount,
    int VacancyCount,
    bool IsVacant,
    bool IsActive);

internal sealed record ApprovalNodeResponse(
    int StepOrder,
    string PositionRefId,
    string PositionCode,
    string PositionTitle,
    bool IsVacantOrInactive,
    bool IsDelegated,
    string? ApproverRefId,
    string? ApproverCode,
    string? ApproverTitle,
    string? DelegationRefId,
    string? DelegationReason);

internal sealed record ApprovalChainResponse(
    string TargetRefId,
    string TargetCode,
    string TargetTitle,
    DateTimeOffset EvaluatedAtUtc,
    string? DocTypeCode,
    IReadOnlyList<ApprovalNodeResponse> Nodes);

internal sealed record PositionCostCenterResponse(
    string RefId,
    string Code,
    string Name);

internal sealed record PositionJobResponse(
    string RefId,
    string Code,
    string Name);

internal sealed record PositionJobGradeResponse(
    string RefId,
    string Code,
    string Name,
    int RankOrder);
