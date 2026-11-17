using Villsource.FSH.Modules.Organization.Features.v1.OrganizationUnitTypes.Endpoints;

namespace Villsource.FSH.Modules.Organization.Features.v1.OrganizationUnits.Endpoints;

internal sealed record OrganizationUnitSummaryResponse(
    string RefId,
    string Code,
    string Name,
    string IntlName,
    string TypeRefId,
    string TypeCode,
    string TypeName,
    int TypeLevel,
    string? LegalEntityRefId,
    string? ParentRefId,
    string? ParentName,
    int Depth,
    string HierarchyPath,
    string? ManagerRefId,
    string? ManagerCode,
    string? ManagerTitle,
    bool IsActive,
    int ActiveChildCount);

internal sealed record OrganizationUnitDetailResponse(
    string RefId,
    string Code,
    string Name,
    string IntlName,
    OrganizationUnitTypeLookupResponse Type,
    string? Description,
    string? LegalEntityRefId,
    string? ParentRefId,
    string? ParentCode,
    string? ParentName,
    int Depth,
    string HierarchyPath,
    string? CostCenterRefId,
    string? ManagerRefId,
    string? ManagerCode,
    string? ManagerTitle,
    bool IsActive,
    IReadOnlyList<OrganizationUnitChildResponse> Children,
    DateTimeOffset CreatedOnUtc,
    DateTimeOffset? LastModifiedOnUtc);

internal sealed record OrganizationTreeNodeResponse(
    string RefId,
    string Code,
    string Name,
    string IntlName,
    string TypeRefId,
    string TypeCode,
    string TypeName,
    int TypeLevel,
    int Depth,
    string HierarchyPath,
    bool IsActive,
    string? CostCenterRefId,
    string? ManagerRefId,
    string? ManagerCode,
    string? ManagerTitle,
    IReadOnlyList<OrganizationTreeNodeResponse> Children);

internal sealed record OrganizationUnitChildResponse(
    string RefId,
    string Code,
    string Name,
    string IntlName,
    string TypeCode,
    string TypeName,
    int Depth,
    string? ManagerRefId,
    string? ManagerTitle,
    bool IsActive);

internal sealed record OrganizationUnitLeadershipChainResponse(
    string TargetRefId,
    string TargetCode,
    string TargetName,
    IReadOnlyList<OrganizationUnitLeadershipNodeResponse> Chain);

internal sealed record OrganizationUnitLeadershipNodeResponse(
    int DepthFromTarget,
    int OuDepth,
    string OuRefId,
    string OuCode,
    string OuName,
    bool OuIsActive,
    string? ManagerRefId,
    string? ManagerCode,
    string? ManagerTitle,
    bool? ManagerIsActive);
