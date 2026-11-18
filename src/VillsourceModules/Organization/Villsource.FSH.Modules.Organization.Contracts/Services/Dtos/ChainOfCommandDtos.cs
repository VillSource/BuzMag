namespace Villsource.FSH.Modules.Organization.Contracts.Services.Dtos;

/// <summary>
/// Indicates how a manager position was resolved in the chain of command.
/// </summary>
public enum ManagerResolutionSource
{
    /// <summary>
    /// No resolution source.
    /// </summary>
    None = 0,

    /// <summary>
    /// Resolved via the solid-line <c>ReportsToPositionId</c> hierarchy on Position.
    /// </summary>
    DirectReporting = 1,

    /// <summary>
    /// Resolved via the matrix/functional <c>DottedLinePositionId</c> link on Position.
    /// </summary>
    DottedLine = 2,

    /// <summary>
    /// Resolved by crawling up the <c>OrganizationUnit</c> hierarchy to an assigned <c>ManagerPositionId</c>.
    /// </summary>
    OrganizationUnitHierarchy = 3
}

/// <summary>
/// Rich cross-module projection of a Position node within the chain of command,
/// including organizational placement, job grade rank/level, and financial approval authority.
/// </summary>
/// <param name="RefId">Public unique reference identifier of the position.</param>
/// <param name="Code">Normalized position code within the legal entity.</param>
/// <param name="Title">Local/primary title of the position.</param>
/// <param name="InternationalTitle">International (English) title of the position.</param>
/// <param name="LegalEntityRefId">Reference identifier of the owning legal entity.</param>
/// <param name="OrgUnitRefId">Reference identifier of the organization unit this position belongs to.</param>
/// <param name="OrgUnitCode">Code of the organization unit this position belongs to.</param>
/// <param name="OrgUnitName">Name of the organization unit this position belongs to.</param>
/// <param name="JobCode">Code of the associated Job template.</param>
/// <param name="JobGradeCode">Code of the associated JobGrade.</param>
/// <param name="JobGradeRankOrder">Numeric rank order of the JobGrade (higher means more senior).</param>
/// <param name="JobLevelCategory">Optional standardized level category key (e.g. OPERATIONAL, SUPERVISORY, MANAGEMENT, EXECUTIVE).</param>
/// <param name="ApprovalLimitAmount">Optional maximum financial approval limit assigned to the position.</param>
/// <param name="ApprovalLimitCurrency">ISO 3-letter currency code for <paramref name="ApprovalLimitAmount"/>.</param>
/// <param name="IsPrimaryLeadership">Whether this position is flagged as primary leadership.</param>
/// <param name="IsActive">Whether the position is currently active.</param>
/// <param name="ValidFrom">Optional UTC start timestamp of the position's validity window.</param>
/// <param name="ValidTo">Optional UTC end timestamp of the position's validity window.</param>
public sealed record ChainOfCommandPositionDto(
    string RefId,
    string Code,
    string Title,
    string InternationalTitle,
    string LegalEntityRefId,
    string OrgUnitRefId,
    string OrgUnitCode,
    string OrgUnitName,
    string JobCode,
    string JobGradeCode,
    int JobGradeRankOrder,
    string? JobLevelCategory,
    decimal? ApprovalLimitAmount,
    string? ApprovalLimitCurrency,
    bool IsPrimaryLeadership,
    bool IsActive,
    DateTimeOffset? ValidFrom,
    DateTimeOffset? ValidTo);

/// <summary>
/// Represents an Organization Unit manager resolved by crawling up the OU hierarchy tree.
/// </summary>
/// <param name="OriginOrgUnitRefId">Reference ID of the starting Organization Unit where the crawl began.</param>
/// <param name="ResolvedOrgUnitRefId">Reference ID of the Organization Unit where the manager position was found.</param>
/// <param name="ResolvedOrgUnitCode">Code of the Organization Unit where the manager position was found.</param>
/// <param name="ResolvedOrgUnitName">Name of the Organization Unit where the manager position was found.</param>
/// <param name="ResolvedOrgUnitDepth">Tree depth (0 = root) of the Organization Unit where the manager was found.</param>
/// <param name="StepsCrawledUp">Number of parent OU levels climbed from the starting OU (0 = found on starting OU).</param>
/// <param name="ManagerPosition">The resolved manager position details.</param>
public sealed record OuManagerResolutionDto(
    string OriginOrgUnitRefId,
    string ResolvedOrgUnitRefId,
    string ResolvedOrgUnitCode,
    string ResolvedOrgUnitName,
    int ResolvedOrgUnitDepth,
    int StepsCrawledUp,
    ChainOfCommandPositionDto ManagerPosition);

/// <summary>
/// Represents a single Organization Unit node in an upward OU leadership chain from a target OU to the root OU.
/// </summary>
/// <param name="StepsFromStart">Number of upward hops from the starting OU (0 = starting OU).</param>
/// <param name="OuDepth">Absolute depth of this OU in the organization tree (0 = root).</param>
/// <param name="OuRefId">Reference ID of the Organization Unit.</param>
/// <param name="OuCode">Code of the Organization Unit.</param>
/// <param name="OuName">Name of the Organization Unit.</param>
/// <param name="OuInternationalName">International name of the Organization Unit.</param>
/// <param name="OuIsActive">Whether the Organization Unit is active.</param>
/// <param name="ManagerPosition">Assigned manager position of this OU, if any (and not deleted).</param>
public sealed record OuLeadershipStepDto(
    int StepsFromStart,
    int OuDepth,
    string OuRefId,
    string OuCode,
    string OuName,
    string OuInternationalName,
    bool OuIsActive,
    ChainOfCommandPositionDto? ManagerPosition);

/// <summary>
/// Represents an active delegation between a delegator position (granting authority)
/// and a delegate position (receiving acting/approval authority).
/// </summary>
/// <param name="DelegationRefId">Reference ID of the PositionDelegation record.</param>
/// <param name="DelegationTypeKey">Delegation type key (ACTING_FULL, APPROVAL_ONLY, or SPECIFIC_DOCUMENTS).</param>
/// <param name="DocumentTypeScope">Comma-separated document type codes when <paramref name="DelegationTypeKey"/> is SPECIFIC_DOCUMENTS.</param>
/// <param name="Reason">Optional business reason recorded for the delegation.</param>
/// <param name="ValidFrom">UTC start timestamp of the delegation window.</param>
/// <param name="ValidTo">UTC end timestamp of the delegation window.</param>
/// <param name="DelegatorPosition">The position that granted the authority.</param>
/// <param name="DelegatePosition">The acting/substitute position that received the authority.</param>
public sealed record ResolvedDelegationDto(
    string DelegationRefId,
    string DelegationTypeKey,
    string? DocumentTypeScope,
    string? Reason,
    DateTimeOffset ValidFrom,
    DateTimeOffset ValidTo,
    ChainOfCommandPositionDto DelegatorPosition,
    ChainOfCommandPositionDto DelegatePosition);

/// <summary>
/// Represents the effective manager/approver resolved for a position at a point in time,
/// accounting for direct reporting, optional OU hierarchy fallback, and active position delegations.
/// </summary>
/// <param name="StepOrder">1-based step index when returned as part of a multi-step approval chain (1 for single resolution).</param>
/// <param name="ResolutionSource">How the nominal manager position was resolved (DirectReporting, DottedLine, or OrganizationUnitHierarchy).</param>
/// <param name="NominalManagerPosition">The original manager position in the hierarchy before delegation substitution.</param>
/// <param name="EffectiveApproverPosition">The position that should actually act/approve (the <see cref="ResolvedDelegationDto.DelegatePosition"/> if delegated; otherwise <paramref name="NominalManagerPosition"/>).</param>
/// <param name="IsDelegated">True if an active delegation substituted <paramref name="NominalManagerPosition"/> with a delegate position.</param>
/// <param name="ActiveDelegation">Details of the active delegation applied, if <paramref name="IsDelegated"/> is true.</param>
/// <param name="ResolvedFromOrgUnitRefId">When <paramref name="ResolutionSource"/> is <see cref="ManagerResolutionSource.OrganizationUnitHierarchy"/>, the OU ReferenceId where the manager was found.</param>
public sealed record EffectiveManagerResolutionDto(
    int StepOrder,
    ManagerResolutionSource ResolutionSource,
    ChainOfCommandPositionDto NominalManagerPosition,
    ChainOfCommandPositionDto EffectiveApproverPosition,
    bool IsDelegated,
    ResolvedDelegationDto? ActiveDelegation,
    string? ResolvedFromOrgUnitRefId = null);
