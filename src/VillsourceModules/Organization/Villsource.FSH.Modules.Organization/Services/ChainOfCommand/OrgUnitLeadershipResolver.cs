using Villsource.FSH.Modules.Organization.Contracts.Services.Dtos;

namespace Villsource.FSH.Modules.Organization.Services.ChainOfCommand;

internal sealed class OrgUnitLeadershipResolver(ChainOfCommandReadStore store)
{
    public async Task<OuManagerResolutionDto?> ResolveFromPositionAsync(
        string positionRefId,
        bool excludeSelf,
        bool includeInactiveOu,
        DateTimeOffset targetUtc,
        CancellationToken cancellationToken)
    {
        var position = await store.GetPositionByRefIdAsync(positionRefId, cancellationToken).ConfigureAwait(false);
        var originOu = await store.GetOrgUnitByIdAsync(position.OrgUnitId, cancellationToken).ConfigureAwait(false);
        if (originOu is null)
        {
            return null;
        }

        return await ResolveCoreAsync(
            originOu,
            excludePositionId: excludeSelf ? position.Id : null,
            startFromParent: false,
            includeInactiveOu,
            targetUtc,
            preloadedPositions: null,
            cancellationToken).ConfigureAwait(false);
    }

    public async Task<OuManagerResolutionDto?> ResolveFromOrgUnitAsync(
        string orgUnitRefId,
        bool startFromParent,
        bool includeInactiveOu,
        DateTimeOffset targetUtc,
        CancellationToken cancellationToken)
    {
        var originOu = await store.GetOrgUnitByRefIdAsync(orgUnitRefId, cancellationToken).ConfigureAwait(false);

        return await ResolveCoreAsync(
            originOu,
            excludePositionId: null,
            startFromParent,
            includeInactiveOu,
            targetUtc,
            preloadedPositions: null,
            cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<OuLeadershipStepDto>> ResolveLeadershipChainAsync(
        string orgUnitRefId,
        bool onlyUnitsWithManager,
        bool includeInactiveOu,
        DateTimeOffset targetUtc,
        CancellationToken cancellationToken)
    {
        var originOu = await store.GetOrgUnitByRefIdAsync(orgUnitRefId, cancellationToken).ConfigureAwait(false);
        var ancestors = await store.LoadAncestorOusAsync(originOu.HierarchyPath, cancellationToken).ConfigureAwait(false);
        var managerPositions = await LoadOuManagersMapAsync(ancestors, cancellationToken).ConfigureAwait(false);

        var result = new List<OuLeadershipStepDto>();

        foreach (var ou in ancestors)
        {
            if (!includeInactiveOu && !ou.IsActive)
            {
                continue;
            }

            ChainOfCommandPositionDto? managerDto = null;
            if (ou.ManagerPositionId.HasValue &&
                managerPositions.TryGetValue(ou.ManagerPositionId.Value, out var managerPos) &&
                ChainOfCommandPredicates.IsPositionEffective(managerPos.Dto, targetUtc))
            {
                managerDto = managerPos.Dto;
            }

            if (onlyUnitsWithManager && managerDto is null)
            {
                continue;
            }

            result.Add(new OuLeadershipStepDto(
                StepsFromStart: originOu.Depth - ou.Depth,
                OuDepth: ou.Depth,
                OuRefId: ou.ReferenceId,
                OuCode: ou.Code,
                OuName: ou.Name,
                OuInternationalName: ou.InternationalName,
                OuIsActive: ou.IsActive,
                ManagerPosition: managerDto));
        }

        return result;
    }

    public async Task<OuManagerResolutionDto?> ResolveCoreAsync(
        OuRow originOu,
        Guid? excludePositionId,
        bool startFromParent,
        bool includeInactiveOu,
        DateTimeOffset targetUtc,
        IReadOnlyDictionary<Guid, PositionProjection>? preloadedPositions,
        CancellationToken cancellationToken)
    {
        var ancestors = await store.LoadAncestorOusAsync(originOu.HierarchyPath, cancellationToken).ConfigureAwait(false);
        var positionsMap = preloadedPositions
            ?? await LoadOuManagersMapAsync(ancestors, cancellationToken).ConfigureAwait(false);

        foreach (var ou in ancestors)
        {
            if (startFromParent && ou.Id == originOu.Id)
            {
                continue;
            }

            if (!includeInactiveOu && !ou.IsActive)
            {
                continue;
            }

            if (!ou.ManagerPositionId.HasValue)
            {
                continue;
            }

            if (excludePositionId.HasValue && ou.ManagerPositionId.Value == excludePositionId.Value)
            {
                continue;
            }

            if (positionsMap.TryGetValue(ou.ManagerPositionId.Value, out var managerPos) &&
                ChainOfCommandPredicates.IsPositionEffective(managerPos.Dto, targetUtc))
            {
                return new OuManagerResolutionDto(
                    OriginOrgUnitRefId: originOu.ReferenceId,
                    ResolvedOrgUnitRefId: ou.ReferenceId,
                    ResolvedOrgUnitCode: ou.Code,
                    ResolvedOrgUnitName: ou.Name,
                    ResolvedOrgUnitDepth: ou.Depth,
                    StepsCrawledUp: originOu.Depth - ou.Depth,
                    ManagerPosition: managerPos.Dto);
            }
        }

        return null;
    }

    public async Task<List<ManagerCandidate>> CollectFallbackCandidatesAsync(
        PositionProjection start,
        IReadOnlyDictionary<Guid, PositionProjection> positions,
        HashSet<Guid> visited,
        int maxDepth,
        DateTimeOffset targetUtc,
        CancellationToken cancellationToken)
    {
        var candidates = new List<ManagerCandidate>();
        var originOu = await store.GetOrgUnitByIdAsync(start.OrgUnitId, cancellationToken).ConfigureAwait(false);
        if (originOu is null)
        {
            return candidates;
        }

        var ancestors = await store.LoadAncestorOusAsync(originOu.HierarchyPath, cancellationToken).ConfigureAwait(false);
        foreach (var ou in ancestors)
        {
            if (candidates.Count >= maxDepth)
            {
                break;
            }

            if (!ou.IsActive || !ou.ManagerPositionId.HasValue)
            {
                continue;
            }

            var managerId = ou.ManagerPositionId.Value;
            if (!visited.Add(managerId))
            {
                continue;
            }

            if (positions.TryGetValue(managerId, out var ouManager) &&
                ChainOfCommandPredicates.IsPositionEffective(ouManager.Dto, targetUtc))
            {
                candidates.Add(new ManagerCandidate(
                    ManagerResolutionSource.OrganizationUnitHierarchy,
                    ouManager,
                    ou.ReferenceId));
            }
        }

        return candidates;
    }

    private async Task<Dictionary<Guid, PositionProjection>> LoadOuManagersMapAsync(
        IReadOnlyList<OuRow> ancestors,
        CancellationToken cancellationToken)
    {
        var managerIds = ancestors
            .Where(ou => ou.ManagerPositionId.HasValue)
            .Select(ou => ou.ManagerPositionId!.Value)
            .Distinct()
            .ToList();

        return managerIds.Count > 0
            ? await store.GetPositionsByIdsAsync(managerIds, cancellationToken).ConfigureAwait(false)
            : new Dictionary<Guid, PositionProjection>();
    }
}
