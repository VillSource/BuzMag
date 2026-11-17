using Mediator;

namespace Villsource.FSH.Modules.Organization.Contracts.v1.CostCenters.Commands;

public sealed record UpdateCostCenterCommand(
    string RefId,
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
    DateTimeOffset? ValidTo = null) : IRequest<string>;