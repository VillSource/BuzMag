using Villsource.FSH.Modules.Organization.Contracts.Constants;
using Villsource.FSH.Modules.Organization.Contracts.Services;
using Villsource.FSH.Modules.Organization.Contracts.Services.Dtos;
using Villsource.FSH.Modules.Organization.Data;
using Villsource.FSH.Modules.Organization.Services.ChainOfCommand;

namespace Villsource.FSH.Modules.Organization.Services;

public sealed class ChainOfCommandService : IChainOfCommandService
{
    private const int DefaultEscalationMaxDepth = 50;

    private readonly ChainOfCommandReadStore _store;
    private readonly PositionHierarchyResolver _positionResolver;
    private readonly OrgUnitLeadershipResolver _ouResolver;
    private readonly DelegationResolver _delegationResolver;

    public ChainOfCommandService(OrganizationDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        _store = new ChainOfCommandReadStore(dbContext);
        _positionResolver = new PositionHierarchyResolver(_store);
        _ouResolver = new OrgUnitLeadershipResolver(_store);
        _delegationResolver = new DelegationResolver(_store);
    }

    public Task<ChainOfCommandPositionDto?> ResolveDirectManagerAsync(
        string positionRefId,
        bool climbToFirstActive = false,
        bool includeInactive = false,
        DateTimeOffset? asOfUtc = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(positionRefId);
        return _positionResolver.ResolveDirectManagerAsync(
            positionRefId,
            climbToFirstActive,
            includeInactive,
            ChainOfCommandPredicates.NormalizeUtc(asOfUtc),
            cancellationToken);
    }

    public Task<ChainOfCommandPositionDto?> ResolveDottedLineManagerAsync(
        string positionRefId,
        bool includeInactive = false,
        DateTimeOffset? asOfUtc = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(positionRefId);
        return _positionResolver.ResolveDottedLineManagerAsync(
            positionRefId,
            includeInactive,
            ChainOfCommandPredicates.NormalizeUtc(asOfUtc),
            cancellationToken);
    }

    public Task<OuManagerResolutionDto?> ResolveOuManagerFromPositionAsync(
        string positionRefId,
        bool excludeSelf = true,
        bool includeInactiveOu = false,
        DateTimeOffset? asOfUtc = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(positionRefId);
        return _ouResolver.ResolveFromPositionAsync(
            positionRefId,
            excludeSelf,
            includeInactiveOu,
            ChainOfCommandPredicates.NormalizeUtc(asOfUtc),
            cancellationToken);
    }

    public Task<OuManagerResolutionDto?> ResolveOuManagerFromOrgUnitAsync(
        string orgUnitRefId,
        bool startFromParent = false,
        bool includeInactiveOu = false,
        DateTimeOffset? asOfUtc = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(orgUnitRefId);
        return _ouResolver.ResolveFromOrgUnitAsync(
            orgUnitRefId,
            startFromParent,
            includeInactiveOu,
            ChainOfCommandPredicates.NormalizeUtc(asOfUtc),
            cancellationToken);
    }

    public Task<IReadOnlyList<ResolvedDelegationDto>> ResolveEffectiveDelegatesAsync(
        string delegatorPositionRefId,
        string? documentTypeCode = null,
        string? delegationTypeKey = null,
        DateTimeOffset? asOfUtc = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(delegatorPositionRefId);
        return _delegationResolver.ResolveEffectiveDelegatesAsync(
            delegatorPositionRefId,
            documentTypeCode,
            delegationTypeKey,
            ChainOfCommandPredicates.NormalizeUtc(asOfUtc),
            cancellationToken);
    }

    public Task<IReadOnlyList<ResolvedDelegationDto>> ResolveActiveDelegatorsAsync(
        string delegatePositionRefId,
        string? documentTypeCode = null,
        string? delegationTypeKey = null,
        DateTimeOffset? asOfUtc = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(delegatePositionRefId);
        return _delegationResolver.ResolveActiveDelegatorsAsync(
            delegatePositionRefId,
            documentTypeCode,
            delegationTypeKey,
            ChainOfCommandPredicates.NormalizeUtc(asOfUtc),
            cancellationToken);
    }

    public async Task<EffectiveManagerResolutionDto?> ResolveEffectiveManagerAsync(
        string positionRefId,
        string? documentTypeCode = null,
        bool fallbackToOuManager = true,
        bool climbToFirstActive = true,
        DateTimeOffset? asOfUtc = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(positionRefId);
        var targetUtc = ChainOfCommandPredicates.NormalizeUtc(asOfUtc);

        var start = await _store.GetPositionByRefIdAsync(positionRefId, cancellationToken).ConfigureAwait(false);
        var positions = await _store.GetLegalEntityPositionsMapAsync(start.LegalEntityId, cancellationToken)
            .ConfigureAwait(false);

        ManagerCandidate? candidate = null;
        var direct = PositionHierarchyResolver.FindDirectManagerInMap(
            start, positions, climbToFirstActive, includeInactive: false, targetUtc);

        if (direct is not null)
        {
            candidate = new ManagerCandidate(ManagerResolutionSource.DirectReporting, direct, null);
        }
        else if (fallbackToOuManager)
        {
            var originOu = await _store.GetOrgUnitByIdAsync(start.OrgUnitId, cancellationToken).ConfigureAwait(false);
            if (originOu is not null)
            {
                var ouResolution = await _ouResolver.ResolveCoreAsync(
                    originOu,
                    excludePositionId: start.Id,
                    startFromParent: false,
                    includeInactiveOu: false,
                    targetUtc,
                    preloadedPositions: positions,
                    cancellationToken).ConfigureAwait(false);

                if (ouResolution is not null &&
                    positions.Values.FirstOrDefault(p =>
                        string.Equals(p.Dto.RefId, ouResolution.ManagerPosition.RefId, StringComparison.Ordinal))
                    is { } ouManager)
                {
                    candidate = new ManagerCandidate(
                        ManagerResolutionSource.OrganizationUnitHierarchy,
                        ouManager,
                        ouResolution.ResolvedOrgUnitRefId);
                }
            }
        }

        if (candidate is null)
        {
            return null;
        }

        var delegations = await _store.LoadActiveLegalEntityDelegationsAsync(start.LegalEntityId, targetUtc, cancellationToken)
            .ConfigureAwait(false);

        return DelegationResolver.BuildEffectiveManagerResolution(
            stepOrder: 1,
            candidate,
            positions,
            delegations,
            targetUtc,
            documentTypeCode);
    }

    public Task<IReadOnlyList<ChainOfCommandPositionDto>> ResolveReportingChainAsync(
        string positionRefId,
        int maxDepth = 15,
        bool includeInactive = false,
        DateTimeOffset? asOfUtc = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(positionRefId);
        ArgumentOutOfRangeException.ThrowIfLessThan(maxDepth, 1);

        return _positionResolver.ResolveReportingChainAsync(
            positionRefId,
            maxDepth,
            includeInactive,
            ChainOfCommandPredicates.NormalizeUtc(asOfUtc),
            cancellationToken);
    }

    public Task<IReadOnlyList<OuLeadershipStepDto>> ResolveOuLeadershipChainAsync(
        string orgUnitRefId,
        bool onlyUnitsWithManager = false,
        bool includeInactiveOu = false,
        DateTimeOffset? asOfUtc = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(orgUnitRefId);

        return _ouResolver.ResolveLeadershipChainAsync(
            orgUnitRefId,
            onlyUnitsWithManager,
            includeInactiveOu,
            ChainOfCommandPredicates.NormalizeUtc(asOfUtc),
            cancellationToken);
    }

    public async Task<IReadOnlyList<EffectiveManagerResolutionDto>> ResolveApprovalChainAsync(
        string positionRefId,
        string? documentTypeCode = null,
        int maxDepth = 15,
        bool fallbackToOuManager = true,
        DateTimeOffset? asOfUtc = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(positionRefId);
        ArgumentOutOfRangeException.ThrowIfLessThan(maxDepth, 1);
        var targetUtc = ChainOfCommandPredicates.NormalizeUtc(asOfUtc);

        var start = await _store.GetPositionByRefIdAsync(positionRefId, cancellationToken).ConfigureAwait(false);
        var positions = await _store.GetLegalEntityPositionsMapAsync(start.LegalEntityId, cancellationToken)
            .ConfigureAwait(false);

        var visited = new HashSet<Guid> { start.Id };
        var candidates = PositionHierarchyResolver.CollectDirectCandidates(
            start, positions, visited, maxDepth, targetUtc);

        if (candidates.Count == 0 && fallbackToOuManager)
        {
            var ouCandidates = await _ouResolver.CollectFallbackCandidatesAsync(
                start, positions, visited, maxDepth, targetUtc, cancellationToken).ConfigureAwait(false);
            candidates.AddRange(ouCandidates);
        }

        if (candidates.Count == 0)
        {
            return [];
        }

        var delegations = await _store.LoadActiveLegalEntityDelegationsAsync(start.LegalEntityId, targetUtc, cancellationToken)
            .ConfigureAwait(false);

        var result = new List<EffectiveManagerResolutionDto>(candidates.Count);
        for (var i = 0; i < candidates.Count; i++)
        {
            result.Add(DelegationResolver.BuildEffectiveManagerResolution(
                stepOrder: i + 1,
                candidates[i],
                positions,
                delegations,
                targetUtc,
                documentTypeCode));
        }

        return result;
    }

    public Task<EffectiveManagerResolutionDto?> ResolveManagerByApprovalLimitAsync(
        string positionRefId,
        decimal requiredAmount,
        string currency,
        string? documentTypeCode = null,
        bool fallbackToOuManager = true,
        DateTimeOffset? asOfUtc = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(positionRefId);
        ArgumentOutOfRangeException.ThrowIfNegative(requiredAmount);
        ArgumentException.ThrowIfNullOrWhiteSpace(currency);

        var normalizedCurrency = currency.Trim().ToUpperInvariant();
        if (normalizedCurrency.Length != 3)
        {
            throw new ArgumentException("Currency must be a 3-letter ISO currency code.", nameof(currency));
        }

        return ResolveEscalatingManagerAsync(
            positionRefId,
            dto => ChainOfCommandPredicates.MatchesApprovalLimit(dto, requiredAmount, normalizedCurrency),
            documentTypeCode,
            fallbackToOuManager,
            asOfUtc,
            cancellationToken);
    }

    public Task<EffectiveManagerResolutionDto?> ResolveManagerByMinJobGradeAsync(
        string positionRefId,
        int? minRankOrder = null,
        string? minJobLevelCategory = null,
        string? documentTypeCode = null,
        bool fallbackToOuManager = true,
        DateTimeOffset? asOfUtc = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(positionRefId);
        if (!minRankOrder.HasValue && string.IsNullOrWhiteSpace(minJobLevelCategory))
        {
            throw new ArgumentException("At least one of minRankOrder or minJobLevelCategory must be specified.");
        }

        var requiredCategory = string.IsNullOrWhiteSpace(minJobLevelCategory)
            ? null
            : JobLevelCategory.FromKey(minJobLevelCategory);

        return ResolveEscalatingManagerAsync(
            positionRefId,
            dto => ChainOfCommandPredicates.MatchesGradeCriteria(dto, minRankOrder, requiredCategory),
            documentTypeCode,
            fallbackToOuManager,
            asOfUtc,
            cancellationToken);
    }

    public Task<IReadOnlyList<ChainOfCommandPositionDto>> GetDirectReportsAsync(
        string managerPositionRefId,
        bool includeInactive = false,
        DateTimeOffset? asOfUtc = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(managerPositionRefId);
        return _positionResolver.GetDirectReportsAsync(
            managerPositionRefId,
            includeInactive,
            ChainOfCommandPredicates.NormalizeUtc(asOfUtc),
            cancellationToken);
    }

    public Task<IReadOnlyList<ChainOfCommandPositionDto>> GetSubordinateSubtreeAsync(
        string managerPositionRefId,
        int maxDepth = 10,
        bool includeInactive = false,
        DateTimeOffset? asOfUtc = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(managerPositionRefId);
        ArgumentOutOfRangeException.ThrowIfLessThan(maxDepth, 1);

        return _positionResolver.GetSubordinateSubtreeAsync(
            managerPositionRefId,
            maxDepth,
            includeInactive,
            ChainOfCommandPredicates.NormalizeUtc(asOfUtc),
            cancellationToken);
    }

    public async Task<bool> IsInChainOfCommandAsync(
        string supervisorPositionRefId,
        string subordinatePositionRefId,
        bool includeDottedLine = false,
        bool includeOuHierarchy = true,
        bool includeActiveDelegations = true,
        DateTimeOffset? asOfUtc = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(supervisorPositionRefId);
        ArgumentException.ThrowIfNullOrWhiteSpace(subordinatePositionRefId);
        var targetUtc = ChainOfCommandPredicates.NormalizeUtc(asOfUtc);

        var supervisor = await _store.GetPositionByRefIdAsync(supervisorPositionRefId, cancellationToken).ConfigureAwait(false);
        var subordinate = await _store.GetPositionByRefIdAsync(subordinatePositionRefId, cancellationToken).ConfigureAwait(false);

        if (supervisor.Id == subordinate.Id ||
            supervisor.LegalEntityId != subordinate.LegalEntityId ||
            !ChainOfCommandPredicates.IsPositionEffective(supervisor.Dto, targetUtc))
        {
            return false;
        }

        var positions = await _store.GetLegalEntityPositionsMapAsync(subordinate.LegalEntityId, cancellationToken)
            .ConfigureAwait(false);

        var visited = new HashSet<Guid> { subordinate.Id };
        var supervisoryChainIds = PositionHierarchyResolver.CollectDirectCandidates(
                subordinate,
                positions,
                visited,
                maxDepth: positions.Count,
                targetUtc)
            .Select(c => c.Position.Id)
            .ToHashSet();

        if (includeDottedLine &&
            subordinate.DottedLinePositionId.HasValue &&
            positions.TryGetValue(subordinate.DottedLinePositionId.Value, out var dotted) &&
            ChainOfCommandPredicates.IsPositionEffective(dotted.Dto, targetUtc))
        {
            supervisoryChainIds.Add(dotted.Id);
        }

        if (includeOuHierarchy)
        {
            var ouCandidates = await _ouResolver.CollectFallbackCandidatesAsync(
                subordinate,
                positions,
                visited: [subordinate.Id],
                maxDepth: positions.Count,
                targetUtc,
                cancellationToken).ConfigureAwait(false);

            foreach (var ouCandidate in ouCandidates)
            {
                supervisoryChainIds.Add(ouCandidate.Position.Id);
            }
        }

        if (supervisoryChainIds.Contains(supervisor.Id))
        {
            return true;
        }

        if (!includeActiveDelegations || supervisoryChainIds.Count == 0)
        {
            return false;
        }

        return await _store.HasActiveDelegationForAnyAsync(
            subordinate.LegalEntityId,
            supervisor.Id,
            supervisoryChainIds,
            targetUtc,
            cancellationToken).ConfigureAwait(false);
    }

    private async Task<EffectiveManagerResolutionDto?> ResolveEscalatingManagerAsync(
        string positionRefId,
        Func<ChainOfCommandPositionDto, bool> predicate,
        string? documentTypeCode,
        bool fallbackToOuManager,
        DateTimeOffset? asOfUtc,
        CancellationToken cancellationToken)
    {
        var targetUtc = ChainOfCommandPredicates.NormalizeUtc(asOfUtc);
        var start = await _store.GetPositionByRefIdAsync(positionRefId, cancellationToken).ConfigureAwait(false);
        var positions = await _store.GetLegalEntityPositionsMapAsync(start.LegalEntityId, cancellationToken)
            .ConfigureAwait(false);

        var visited = new HashSet<Guid> { start.Id };
        var candidates = PositionHierarchyResolver.CollectDirectCandidates(
            start, positions, visited, DefaultEscalationMaxDepth, targetUtc);

        if (fallbackToOuManager)
        {
            var ouCandidates = await _ouResolver.CollectFallbackCandidatesAsync(
                start, positions, visited, DefaultEscalationMaxDepth, targetUtc, cancellationToken).ConfigureAwait(false);
            candidates.AddRange(ouCandidates);
        }

        var match = candidates.FirstOrDefault(c => predicate(c.Position.Dto));
        if (match is null)
        {
            return null;
        }

        var delegations = await _store.LoadActiveLegalEntityDelegationsAsync(start.LegalEntityId, targetUtc, cancellationToken)
            .ConfigureAwait(false);

        return DelegationResolver.BuildEffectiveManagerResolution(
            stepOrder: 1,
            match,
            positions,
            delegations,
            targetUtc,
            documentTypeCode);
    }
}
