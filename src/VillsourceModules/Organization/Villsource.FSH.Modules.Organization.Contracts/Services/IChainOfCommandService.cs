using Villsource.FSH.Modules.Organization.Contracts.Services.Dtos;

namespace Villsource.FSH.Modules.Organization.Contracts.Services;

/// <summary>
/// Public cross-module service for resolving organizational reporting lines,
/// Organization Unit (OU) leadership hierarchy, acting/approval delegations,
/// and authority-based escalations (Chain of Command).
/// </summary>
public interface IChainOfCommandService
{
    /// <summary>
    /// Resolves the direct (solid-line) reporting manager position (<c>ReportsToPosition</c>) for a given position.
    /// </summary>
    /// <remarks>
    /// By default (<paramref name="climbToFirstActive"/> = <c>false</c>), only the immediate <c>ReportsToPosition</c>
    /// is checked; if it is inactive or outside its validity window at <paramref name="asOfUtc"/>, <c>null</c> is returned
    /// (unless <paramref name="includeInactive"/> is <c>true</c>).
    /// When <paramref name="climbToFirstActive"/> is <c>true</c>, this method walks up the <c>ReportsToPosition</c> chain
    /// until it finds the first position that is active and temporally valid at <paramref name="asOfUtc"/>.
    /// Throws <c>FSH.Framework.Core.Exceptions.NotFoundException</c> when <paramref name="positionRefId"/> does not exist.
    /// </remarks>
    /// <param name="positionRefId">The <c>ReferenceId</c> of the subordinate position to resolve the direct manager for.</param>
    /// <param name="climbToFirstActive">If <c>true</c>, climbs up the reporting line past inactive or expired manager positions until the first active manager position is found.</param>
    /// <param name="includeInactive">If <c>true</c>, returns the immediate manager position even if it is inactive or outside its validity dates.</param>
    /// <param name="asOfUtc">Optional UTC timestamp used to evaluate position <c>ValidFrom</c>/<c>ValidTo</c> dates (defaults to <see cref="DateTimeOffset.UtcNow"/>).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The resolved direct manager position, or <c>null</c> if the position has no direct manager (or none matching the active criteria).</returns>
    Task<ChainOfCommandPositionDto?> ResolveDirectManagerAsync(
        string positionRefId,
        bool climbToFirstActive = false,
        bool includeInactive = false,
        DateTimeOffset? asOfUtc = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Resolves the dotted-line (matrix / functional) manager position (<c>DottedLinePosition</c>) for a given position.
    /// </summary>
    /// <remarks>
    /// Evaluates whether the target position has a <c>DottedLinePositionId</c> assigned.
    /// Unless <paramref name="includeInactive"/> is <c>true</c>, the dotted-line manager position must be active
    /// and within its <c>ValidFrom</c>/<c>ValidTo</c> validity window at <paramref name="asOfUtc"/>.
    /// Throws <c>FSH.Framework.Core.Exceptions.NotFoundException</c> when <paramref name="positionRefId"/> does not exist.
    /// </remarks>
    /// <param name="positionRefId">The <c>ReferenceId</c> of the position whose dotted-line manager should be resolved.</param>
    /// <param name="includeInactive">If <c>true</c>, returns the dotted-line manager position even if it is currently inactive or expired.</param>
    /// <param name="asOfUtc">Optional UTC timestamp used to evaluate position validity dates (defaults to <see cref="DateTimeOffset.UtcNow"/>).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The resolved dotted-line manager position, or <c>null</c> if none is assigned or active.</returns>
    Task<ChainOfCommandPositionDto?> ResolveDottedLineManagerAsync(
        string positionRefId,
        bool includeInactive = false,
        DateTimeOffset? asOfUtc = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Resolves the Organization Unit (OU) manager for a given position by starting at the position's own OU
    /// and crawling up the ancestor OU hierarchy until the first active manager position is found.
    /// </summary>
    /// <remarks>
    /// When <paramref name="excludeSelf"/> is <c>true</c> (the default) and the starting position is itself
    /// the assigned <c>ManagerPosition</c> of its own OU (or an ancestor OU), that OU is skipped and the search
    /// continues crawling up to parent OUs so a manager does not resolve to themselves.
    /// Throws <c>FSH.Framework.Core.Exceptions.NotFoundException</c> when <paramref name="positionRefId"/> does not exist.
    /// </remarks>
    /// <param name="positionRefId">The <c>ReferenceId</c> of the starting position.</param>
    /// <param name="excludeSelf">If <c>true</c> (default), skips any OU whose <c>ManagerPositionId</c> equals the starting position's ID and continues climbing to parent OUs.</param>
    /// <param name="includeInactiveOu">If <c>true</c>, considers manager positions on inactive ancestor OUs; if <c>false</c> (default), skips inactive OUs.</param>
    /// <param name="asOfUtc">Optional UTC timestamp used to evaluate manager position validity dates (defaults to <see cref="DateTimeOffset.UtcNow"/>).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="OuManagerResolutionDto"/> describing the OU where the manager was found, the number of steps crawled up, and the manager position; or <c>null</c> if no manager is found up to the root OU.</returns>
    Task<OuManagerResolutionDto?> ResolveOuManagerFromPositionAsync(
        string positionRefId,
        bool excludeSelf = true,
        bool includeInactiveOu = false,
        DateTimeOffset? asOfUtc = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Resolves the Organization Unit (OU) manager by starting at a specified Organization Unit
    /// (or its immediate parent when <paramref name="startFromParent"/> is <c>true</c>) and crawling up
    /// the ancestor OU hierarchy until the first active manager position is found.
    /// </summary>
    /// <remarks>
    /// Throws <c>FSH.Framework.Core.Exceptions.NotFoundException</c> when <paramref name="orgUnitRefId"/> does not exist.
    /// </remarks>
    /// <param name="orgUnitRefId">The <c>ReferenceId</c> of the starting Organization Unit.</param>
    /// <param name="startFromParent">If <c>true</c>, ignores the manager of the starting OU itself and begins searching from its parent OU upward.</param>
    /// <param name="includeInactiveOu">If <c>true</c>, considers manager positions on inactive OUs; if <c>false</c> (default), skips inactive OUs.</param>
    /// <param name="asOfUtc">Optional UTC timestamp used to evaluate manager position validity dates (defaults to <see cref="DateTimeOffset.UtcNow"/>).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="OuManagerResolutionDto"/> with the resolved OU and manager position, or <c>null</c> if no manager is found up to the root OU.</returns>
    Task<OuManagerResolutionDto?> ResolveOuManagerFromOrgUnitAsync(
        string orgUnitRefId,
        bool startFromParent = false,
        bool includeInactiveOu = false,
        DateTimeOffset? asOfUtc = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Resolves all currently effective delegate (acting/substitute) positions that have been granted
    /// authority to act or approve on behalf of the specified delegator position.
    /// </summary>
    /// <remarks>
    /// Filters active, non-revoked <c>PositionDelegation</c> records where <c>DelegatorPosition.ReferenceId == delegatorPositionRefId</c>,
    /// the timestamp <paramref name="asOfUtc"/> falls within <c>[ValidFrom, ValidTo]</c>, the delegate position is active,
    /// and optional <paramref name="documentTypeCode"/> / <paramref name="delegationTypeKey"/> scopes match.
    /// Results are ordered by authority breadth (<c>ACTING_FULL</c>, then <c>APPROVAL_ONLY</c>, then <c>SPECIFIC_DOCUMENTS</c>)
    /// and then by <c>ValidFrom</c> descending.
    /// Throws <c>FSH.Framework.Core.Exceptions.NotFoundException</c> when <paramref name="delegatorPositionRefId"/> does not exist.
    /// </remarks>
    /// <param name="delegatorPositionRefId">The <c>ReferenceId</c> of the position that delegated its authority (the manager/principal position).</param>
    /// <param name="documentTypeCode">Optional document type code (e.g., <c>"PR"</c>, <c>"LEAVE"</c>) to evaluate document-scoped delegations.</param>
    /// <param name="delegationTypeKey">Optional delegation type filter (<c>"ACTING_FULL"</c>, <c>"APPROVAL_ONLY"</c>, or <c>"SPECIFIC_DOCUMENTS"</c>).</param>
    /// <param name="asOfUtc">Optional UTC timestamp at which to check delegation effectiveness (defaults to <see cref="DateTimeOffset.UtcNow"/>).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Ordered list of effective delegations where <see cref="ResolvedDelegationDto.DelegatePosition"/> is the acting position.</returns>
    Task<IReadOnlyList<ResolvedDelegationDto>> ResolveEffectiveDelegatesAsync(
        string delegatorPositionRefId,
        string? documentTypeCode = null,
        string? delegationTypeKey = null,
        DateTimeOffset? asOfUtc = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Resolves all active delegator (principal) positions whose authority is currently held by the specified delegate position.
    /// </summary>
    /// <remarks>
    /// Answers the reverse delegation question: "Which positions am I currently acting or approving on behalf of?"
    /// Filters non-revoked <c>PositionDelegation</c> records where <c>DelegatePosition.ReferenceId == delegatePositionRefId</c>
    /// and <paramref name="asOfUtc"/> is within <c>[ValidFrom, ValidTo]</c>.
    /// Throws <c>FSH.Framework.Core.Exceptions.NotFoundException</c> when <paramref name="delegatePositionRefId"/> does not exist.
    /// </remarks>
    /// <param name="delegatePositionRefId">The <c>ReferenceId</c> of the acting/substitute position receiving delegated authority.</param>
    /// <param name="documentTypeCode">Optional document type code to filter document-scoped delegations.</param>
    /// <param name="delegationTypeKey">Optional delegation type key filter (<c>"ACTING_FULL"</c>, <c>"APPROVAL_ONLY"</c>, or <c>"SPECIFIC_DOCUMENTS"</c>).</param>
    /// <param name="asOfUtc">Optional UTC timestamp at which to check delegation effectiveness (defaults to <see cref="DateTimeOffset.UtcNow"/>).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Ordered list of active delegations where <see cref="ResolvedDelegationDto.DelegatorPosition"/> is the principal position being represented.</returns>
    Task<IReadOnlyList<ResolvedDelegationDto>> ResolveActiveDelegatorsAsync(
        string delegatePositionRefId,
        string? documentTypeCode = null,
        string? delegationTypeKey = null,
        DateTimeOffset? asOfUtc = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Resolves the single effective manager/approver for a position by combining direct manager resolution,
    /// optional Organization Unit manager crawl-up fallback, and automatic delegation substitution.
    /// </summary>
    /// <remarks>
    /// Resolution sequence:
    /// <list type="number">
    /// <item><description>Attempts to resolve the active direct reporting manager (<c>ReportsToPosition</c>, climbing past inactive positions when <paramref name="climbToFirstActive"/> is <c>true</c>).</description></item>
    /// <item><description>If no direct manager is found and <paramref name="fallbackToOuManager"/> is <c>true</c>, crawls up the Organization Unit hierarchy (<c>excludeSelf: true</c>) to find the first active OU manager.</description></item>
    /// <item><description>Checks if the resolved nominal manager position has an active delegation at <paramref name="asOfUtc"/> (if <paramref name="documentTypeCode"/> is specified, document-scoped delegations matching that code are eligible; otherwise only <c>ACTING_FULL</c> and <c>APPROVAL_ONLY</c> delegations substitute automatically). If so, sets <see cref="EffectiveManagerResolutionDto.EffectiveApproverPosition"/> to the top-priority active delegate position.</description></item>
    /// </list>
    /// Throws <c>FSH.Framework.Core.Exceptions.NotFoundException</c> when <paramref name="positionRefId"/> does not exist.
    /// </remarks>
    /// <param name="positionRefId">The <c>ReferenceId</c> of the requester/subordinate position.</param>
    /// <param name="documentTypeCode">Optional document type code used to match document-specific delegations.</param>
    /// <param name="fallbackToOuManager">If <c>true</c> (default), falls back to crawling up the OU hierarchy when the position has no active direct reporting manager.</param>
    /// <param name="climbToFirstActive">If <c>true</c> (default), climbs past inactive positions in the direct reporting line.</param>
    /// <param name="asOfUtc">Optional UTC timestamp used for validity and delegation checks (defaults to <see cref="DateTimeOffset.UtcNow"/>).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The resolved nominal and effective approver position details, or <c>null</c> if no manager exists in either the reporting line or OU hierarchy.</returns>
    Task<EffectiveManagerResolutionDto?> ResolveEffectiveManagerAsync(
        string positionRefId,
        string? documentTypeCode = null,
        bool fallbackToOuManager = true,
        bool climbToFirstActive = true,
        DateTimeOffset? asOfUtc = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Resolves the upward direct reporting chain of command from a starting position up to the top of the reporting tree.
    /// </summary>
    /// <remarks>
    /// Throws <c>FSH.Framework.Core.Exceptions.NotFoundException</c> when <paramref name="positionRefId"/> does not exist.
    /// </remarks>
    /// <param name="positionRefId">The <c>ReferenceId</c> of the starting position.</param>
    /// <param name="maxDepth">Maximum number of upward reporting hops to traverse (defaults to 15).</param>
    /// <param name="includeInactive">If <c>true</c>, includes inactive/expired positions in the returned chain; if <c>false</c> (default), skips inactive positions while continuing upward traversal.</param>
    /// <param name="asOfUtc">Optional UTC timestamp used to evaluate position validity windows (defaults to <see cref="DateTimeOffset.UtcNow"/>).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Ordered list of manager positions from immediate supervisor (index 0) up to the top-most position.</returns>
    Task<IReadOnlyList<ChainOfCommandPositionDto>> ResolveReportingChainAsync(
        string positionRefId,
        int maxDepth = 15,
        bool includeInactive = false,
        DateTimeOffset? asOfUtc = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Resolves the upward Organization Unit (OU) leadership chain from a starting OU up to the root OU.
    /// </summary>
    /// <remarks>
    /// Throws <c>FSH.Framework.Core.Exceptions.NotFoundException</c> when <paramref name="orgUnitRefId"/> does not exist.
    /// </remarks>
    /// <param name="orgUnitRefId">The <c>ReferenceId</c> of the starting Organization Unit.</param>
    /// <param name="onlyUnitsWithManager">If <c>true</c>, filters the returned chain to only OUs that have an active manager position assigned.</param>
    /// <param name="includeInactiveOu">If <c>true</c>, includes inactive ancestor OUs; if <c>false</c> (default), excludes inactive OUs.</param>
    /// <param name="asOfUtc">Optional UTC timestamp used to evaluate manager position validity windows (defaults to <see cref="DateTimeOffset.UtcNow"/>).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Ordered list of OU leadership steps from the starting OU (or nearest matching OU) up to the root OU.</returns>
    Task<IReadOnlyList<OuLeadershipStepDto>> ResolveOuLeadershipChainAsync(
        string orgUnitRefId,
        bool onlyUnitsWithManager = false,
        bool includeInactiveOu = false,
        DateTimeOffset? asOfUtc = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Resolves the multi-step upward approval chain for a position, automatically evaluating active delegations
    /// at every step and optionally falling back to the OU leadership chain when the direct reporting line has no active managers.
    /// </summary>
    /// <remarks>
    /// Throws <c>FSH.Framework.Core.Exceptions.NotFoundException</c> when <paramref name="positionRefId"/> does not exist.
    /// </remarks>
    /// <param name="positionRefId">The <c>ReferenceId</c> of the target/requester position.</param>
    /// <param name="documentTypeCode">Optional document type code used to evaluate document-scoped delegations at each step.</param>
    /// <param name="maxDepth">Maximum number of approval steps to resolve (defaults to 15).</param>
    /// <param name="fallbackToOuManager">If <c>true</c>, falls back to crawling up the OU hierarchy if the starting position has no active direct reporting chain.</param>
    /// <param name="asOfUtc">Optional UTC timestamp used to evaluate position and delegation validity (defaults to <see cref="DateTimeOffset.UtcNow"/>).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Ordered list of effective approval steps from first-line approver upward.</returns>
    Task<IReadOnlyList<EffectiveManagerResolutionDto>> ResolveApprovalChainAsync(
        string positionRefId,
        string? documentTypeCode = null,
        int maxDepth = 15,
        bool fallbackToOuManager = true,
        DateTimeOffset? asOfUtc = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Crawls up the chain of command from a starting position until finding the first active manager position
    /// whose financial approval limit (<c>ApprovalLimitAmount</c>) is greater than or equal to <paramref name="requiredAmount"/>
    /// in the specified <paramref name="currency"/>, including active delegation resolution.
    /// </summary>
    /// <remarks>
    /// Throws <c>FSH.Framework.Core.Exceptions.NotFoundException</c> when <paramref name="positionRefId"/> does not exist.
    /// </remarks>
    /// <param name="positionRefId">The <c>ReferenceId</c> of the starting position.</param>
    /// <param name="requiredAmount">Minimum approval limit amount required (must be non-negative).</param>
    /// <param name="currency">3-letter ISO currency code (case-insensitive, e.g., <c>"THB"</c>, <c>"USD"</c>).</param>
    /// <param name="documentTypeCode">Optional document type code used to resolve active delegations for the qualifying manager.</param>
    /// <param name="fallbackToOuManager">If <c>true</c> (default), also checks the OU leadership hierarchy if the direct reporting chain does not yield a qualifying manager.</param>
    /// <param name="asOfUtc">Optional UTC timestamp for validity and delegation evaluation (defaults to <see cref="DateTimeOffset.UtcNow"/>).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The first qualifying manager (and effective delegate, if any) meeting the approval limit, or <c>null</c> if no position in the chain has sufficient limit.</returns>
    Task<EffectiveManagerResolutionDto?> ResolveManagerByApprovalLimitAsync(
        string positionRefId,
        decimal requiredAmount,
        string currency,
        string? documentTypeCode = null,
        bool fallbackToOuManager = true,
        DateTimeOffset? asOfUtc = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Crawls up the chain of command from a starting position until finding the first active manager position
    /// that meets the specified minimum <c>JobGrade.RankOrder</c> and/or minimum <c>JobGrade.JobLevelCategory</c>.
    /// </summary>
    /// <remarks>
    /// Throws <c>FSH.Framework.Core.Exceptions.NotFoundException</c> when <paramref name="positionRefId"/> does not exist.
    /// </remarks>
    /// <param name="positionRefId">The <c>ReferenceId</c> of the starting position.</param>
    /// <param name="minRankOrder">Optional minimum <c>JobGrade.RankOrder</c> required.</param>
    /// <param name="minJobLevelCategory">Optional minimum <c>JobLevelCategory</c> key required (<c>"OPERATIONAL"</c>, <c>"SUPERVISORY"</c>, <c>"MANAGEMENT"</c>, or <c>"EXECUTIVE"</c>).</param>
    /// <param name="documentTypeCode">Optional document type code used to resolve active delegations for the qualifying manager.</param>
    /// <param name="fallbackToOuManager">If <c>true</c> (default), also searches up the OU leadership hierarchy if the direct reporting chain does not yield a qualifying manager.</param>
    /// <param name="asOfUtc">Optional UTC timestamp for validity and delegation evaluation (defaults to <see cref="DateTimeOffset.UtcNow"/>).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The first qualifying manager (and effective delegate, if any) meeting the grade/category threshold, or <c>null</c> if none is found.</returns>
    Task<EffectiveManagerResolutionDto?> ResolveManagerByMinJobGradeAsync(
        string positionRefId,
        int? minRankOrder = null,
        string? minJobLevelCategory = null,
        string? documentTypeCode = null,
        bool fallbackToOuManager = true,
        DateTimeOffset? asOfUtc = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all immediate direct report positions (<c>ReportsToPositionId == manager.Id</c>) for a given manager position.
    /// </summary>
    /// <remarks>
    /// Throws <c>FSH.Framework.Core.Exceptions.NotFoundException</c> when <paramref name="managerPositionRefId"/> does not exist.
    /// </remarks>
    /// <param name="managerPositionRefId">The <c>ReferenceId</c> of the manager position.</param>
    /// <param name="includeInactive">If <c>true</c>, includes inactive direct report positions; if <c>false</c> (default), returns only active positions.</param>
    /// <param name="asOfUtc">Optional UTC timestamp used to evaluate position validity dates (defaults to <see cref="DateTimeOffset.UtcNow"/>).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of direct report positions ordered by <c>JobGradeRankOrder</c> descending, then <c>Code</c> ascending.</returns>
    Task<IReadOnlyList<ChainOfCommandPositionDto>> GetDirectReportsAsync(
        string managerPositionRefId,
        bool includeInactive = false,
        DateTimeOffset? asOfUtc = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the downward subordinate position subtree under a given manager position using breadth-first traversal.
    /// </summary>
    /// <remarks>
    /// Throws <c>FSH.Framework.Core.Exceptions.NotFoundException</c> when <paramref name="managerPositionRefId"/> does not exist.
    /// </remarks>
    /// <param name="managerPositionRefId">The <c>ReferenceId</c> of the root manager position.</param>
    /// <param name="maxDepth">Maximum downward depth levels to traverse (defaults to 10).</param>
    /// <param name="includeInactive">If <c>true</c>, includes inactive subordinate positions; if <c>false</c> (default), returns only active positions.</param>
    /// <param name="asOfUtc">Optional UTC timestamp used to evaluate position validity dates (defaults to <see cref="DateTimeOffset.UtcNow"/>).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Flat list of all subordinate positions in breadth-first order.</returns>
    Task<IReadOnlyList<ChainOfCommandPositionDto>> GetSubordinateSubtreeAsync(
        string managerPositionRefId,
        int maxDepth = 10,
        bool includeInactive = false,
        DateTimeOffset? asOfUtc = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines whether <paramref name="supervisorPositionRefId"/> holds supervisory authority over
    /// <paramref name="subordinatePositionRefId"/> within the chain of command.
    /// </summary>
    /// <remarks>
    /// Throws <c>FSH.Framework.Core.Exceptions.NotFoundException</c> when either position <c>ReferenceId</c> does not exist.
    /// </remarks>
    /// <param name="supervisorPositionRefId">The <c>ReferenceId</c> of the candidate supervisor/manager position.</param>
    /// <param name="subordinatePositionRefId">The <c>ReferenceId</c> of the subordinate position.</param>
    /// <param name="includeDottedLine">If <c>true</c>, also checks the subordinate's immediate dotted-line manager.</param>
    /// <param name="includeOuHierarchy">If <c>true</c> (default), also checks ancestor OU manager positions above the subordinate.</param>
    /// <param name="includeActiveDelegations">If <c>true</c> (default), returns <c>true</c> when <paramref name="supervisorPositionRefId"/> holds an active delegation for any manager in the subordinate's chain of command.</param>
    /// <param name="asOfUtc">Optional UTC timestamp used to evaluate validity and active delegations (defaults to <see cref="DateTimeOffset.UtcNow"/>).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><c>true</c> if <paramref name="supervisorPositionRefId"/> is in the upward chain of command of <paramref name="subordinatePositionRefId"/>; otherwise <c>false</c>.</returns>
    Task<bool> IsInChainOfCommandAsync(
        string supervisorPositionRefId,
        string subordinatePositionRefId,
        bool includeDottedLine = false,
        bool includeOuHierarchy = true,
        bool includeActiveDelegations = true,
        DateTimeOffset? asOfUtc = null,
        CancellationToken cancellationToken = default);
}
