using Mediator;

namespace Villsource.FSH.Modules.Organization.Contracts.v1.CostCenters.Commands;

public sealed record CreateCostCenterCommand(
    string LegalEntityRefId,
    string Code,
    string Name,
    string InternationalName,
    string FunctionType,
    string? Description = null,
    string? ParentCostCenterRefId = null,
    string? DefaultTaxBranchRefId = null,
    string? ResponsiblePositionRefId = null,
    string? OrgUnitRefId = null,
    DateTimeOffset? ValidFrom = null,
    DateTimeOffset? ValidTo = null,
    bool IsActive = true,
    bool IsPostingBlocked = false) : IRequest<string>;