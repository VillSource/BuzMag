using Mediator;

namespace Villsource.FSH.Modules.Organization.Contracts.v1.Positions.Commands;

public sealed record CreatePositionCommand(
    string LegalEntityRefId,
    Guid OrgUnitId,
    string JobRefId,
    string Code,
    string Title,
    string InternationalTitle,
    string? JobGradeRefId = null,
    string? CostCenterRefId = null,
    string? ReportsToPositionRefId = null,
    string? DottedLinePositionRefId = null,
    int HeadcountCapacity = 1,
    bool IsPrimaryLeadership = false,
    DateTimeOffset? ValidFrom = null,
    DateTimeOffset? ValidTo = null,
    bool IsActive = true) : IRequest<string>;