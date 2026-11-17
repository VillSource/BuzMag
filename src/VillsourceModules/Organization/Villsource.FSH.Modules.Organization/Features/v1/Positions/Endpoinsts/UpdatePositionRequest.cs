namespace Villsource.FSH.Modules.Organization.Features.v1.Positions.Endpoinsts;

internal sealed record UpdatePositionRequest(
    Guid OrgUnitId,
    string JobRefId,
    string JobGradeRefId,
    string Code,
    string Title,
    string InternationalTitle,
    string? CostCenterRefId = null,
    string? ReportsToPositionRefId = null,
    string? DottedLinePositionRefId = null,
    int HeadcountCapacity = 1,
    bool IsPrimaryLeadership = false,
    DateTimeOffset? ValidFrom = null,
    DateTimeOffset? ValidTo = null);