using FSH.Framework.Shared.Persistence;
using Mediator;

namespace Villsource.FSH.Modules.Organization.Contracts.v1.OrganizationUnits;

#region DTOs

public sealed record OrganizationUnitSummaryDto(
    string RefId,
    string Code,
    string Name,
    string InternationalName,
    string Type,
    string? LegalEntityRefId,
    string? ParentRefId,
    string? ParentName,
    int Level,
    string HierarchyPath,
    string? ManagerPositionRefId,
    string? ManagerPositionCode,
    string? ManagerPositionTitle,
    bool IsActive,
    int ActiveChildCount);

public sealed record OrganizationUnitDetailDto(
    string RefId,
    string Code,
    string Name,
    string InternationalName,
    string Type,
    string? Description,
    string? LegalEntityRefId,
    string? ParentRefId,
    string? ParentCode,
    string? ParentName,
    int Level,
    string HierarchyPath,
    string? CostCenterCode,
    string? ManagerPositionRefId,
    string? ManagerPositionCode,
    string? ManagerPositionTitle,
    bool IsActive,
    IReadOnlyList<OrganizationUnitChildDto> Children,
    DateTimeOffset CreatedOnUtc,
    DateTimeOffset? LastModifiedOnUtc);

public sealed record OrganizationUnitChildDto(
    string RefId,
    string Code,
    string Name,
    string InternationalName,
    string Type,
    int Level,
    string? ManagerPositionRefId,
    string? ManagerPositionTitle,
    bool IsActive);

public sealed record OrganizationUnitLeadershipChainDto(
    string TargetOuRefId,
    string TargetOuCode,
    string TargetOuName,
    IReadOnlyList<OrganizationUnitLeadershipNodeDto> Chain);

public sealed record OrganizationUnitLeadershipNodeDto(
    int DepthFromTarget,
    int OuLevel,
    string OuRefId,
    string OuCode,
    string OuName,
    bool OuIsActive,
    string? ManagerPositionRefId,
    string? ManagerPositionCode,
    string? ManagerPositionTitle,
    bool? ManagerPositionIsActive);

#endregion

#region Queries

public sealed record SearchOrganizationUnitsQuery : IPagedQuery, IRequest<PagedResponse<OrganizationUnitSummaryDto>>
{
    public int? PageNumber { get; set; }
    public int? PageSize { get; set; }
    public string? Sort { get; set; }
    public string? Keyword { get; set; }
    public string? LegalEntityRefId { get; set; }
    public string? ParentRefId { get; set; }
    public string? ManagerPositionRefId { get; set; }
    public string? Type { get; set; }
    public bool? IsActive { get; set; }
    public bool RootOnly { get; set; }
    public bool VacantManagerOnly { get; set; }
}

public sealed record GetOrganizationUnitByRefIdQuery(string RefId) : IRequest<OrganizationUnitDetailDto>;

public sealed record GetOrganizationUnitLeadershipChainQuery(string RefId) : IRequest<OrganizationUnitLeadershipChainDto>;

#endregion

#region Commands

public sealed record CreateOrganizationUnitCommand(
    string Code,
    string Name,
    string InternationalName,
    string Type,
    string? Description = null,
    string? LegalEntityRefId = null,
    string? ParentRefId = null,
    string? CostCenterCode = null,
    string? ManagerPositionRefId = null,
    bool AllowExternalManagerPosition = false) : IRequest<string>;

public sealed record UpdateOrganizationUnitCommand(
    string RefId,
    string Code,
    string Name,
    string InternationalName,
    string Type,
    string? Description = null,
    string? CostCenterCode = null) : IRequest<string>;

public sealed record AssignManagerPositionCommand(
    string RefId,
    string? ManagerPositionRefId,
    bool AllowExternalManagerPosition = false) : IRequest<string>;

public sealed record MoveOrganizationUnitCommand(
    string RefId,
    string? NewParentRefId) : IRequest<string>;

public sealed record ActivateOrganizationUnitCommand(string RefId) : IRequest<string>;

public sealed record DeactivateOrganizationUnitCommand(string RefId) : IRequest<string>;

public sealed record DeleteOrganizationUnitCommand(string RefId) : IRequest<string>;

#endregion