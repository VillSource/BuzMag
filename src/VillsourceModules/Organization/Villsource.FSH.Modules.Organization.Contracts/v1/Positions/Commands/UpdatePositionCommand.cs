using Mediator;

namespace Villsource.FSH.Modules.Organization.Contracts.v1.Positions.Commands;

public sealed record UpdatePositionCommand(
    string RefId,
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
    DateTimeOffset? ValidTo = null) : IRequest<string>;