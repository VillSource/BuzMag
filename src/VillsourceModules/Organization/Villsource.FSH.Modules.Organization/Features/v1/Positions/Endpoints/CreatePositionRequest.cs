namespace Villsource.FSH.Modules.Organization.Features.v1.Positions.Endpoints;

internal sealed record CreatePositionRequest(
    string LegalEntityRefId,
    string OrgUnitRefId,
    string JobRefId,
    string Code,
    string Title,
    string IntlTitle,
    string? Description = null,
    string? JobGradeRefId = null,
    string? CostCenterRefId = null,
    string? TaxBranchRefId = null,
    decimal? ApprovalLimit = null,
    string? ApprovalCurrency = null,
    string? ReportsToRefId = null,
    string? DottedLineRefId = null,
    int Headcount = 1,
    bool IsLeadership = false,
    DateTimeOffset? ValidFrom = null,
    DateTimeOffset? ValidTo = null,
    bool IsActive = true);
