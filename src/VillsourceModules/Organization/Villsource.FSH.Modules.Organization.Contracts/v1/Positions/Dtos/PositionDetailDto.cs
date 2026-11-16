namespace Villsource.FSH.Modules.Organization.Contracts.v1.Positions.Dtos;

public sealed record PositionDetailDto(
    string RefId,
    string LegalEntityRefId,
    string LegalEntityCode,
    string LegalEntityName,
    Guid OrgUnitId,
    PositionJobDto Job,
    PositionJobGradeDto JobGrade,
    PositionCostCenterDto? CostCenter,
    PositionNodeDto? ReportsToPosition,
    PositionNodeDto? DottedLinePosition,
    string Code,
    string Title,
    string InternationalTitle,
    int HeadcountCapacity,
    bool IsPrimaryLeadership,
    DateTimeOffset? ValidFrom,
    DateTimeOffset? ValidTo,
    bool IsActive,
    IReadOnlyList<PositionNodeDto> DirectReports,
    DateTimeOffset CreatedOnUtc,
    DateTimeOffset? LastModifiedOnUtc);