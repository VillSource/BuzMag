using Villsource.FSH.Modules.Organization.Contracts.Services.Dtos;

namespace Villsource.FSH.Modules.Organization.Services.ChainOfCommand;

internal sealed class PositionHierarchyResolver(ChainOfCommandReadStore store)
{
    public async Task<ChainOfCommandPositionDto?> ResolveDirectManagerAsync(
        string positionRefId,
        bool climbToFirstActive,
        bool includeInactive,
        DateTimeOffset targetUtc,
        CancellationToken cancellationToken)
    {
        var start = await store.GetPositionByRefIdAsync(positionRefId, cancellationToken).ConfigureAwait(false);
        if (!start.ReportsToPositionId.HasValue)
        {
            return null;
        }

        if (!climbToFirstActive)
        {
            var direct = await store.GetPositionByIdAsync(start.ReportsToPositionId.Value, cancellationToken)
                .ConfigureAwait(false);

            return direct is not null && ChainOfCommandPredicates.IsPositionEffective(direct.Dto, targetUtc, includeInactive)
                ? direct.Dto
                : null;
        }

        var positions = await store.GetLegalEntityPositionsMapAsync(start.LegalEntityId, cancellationToken)
            .ConfigureAwait(false);

        return FindDirectManagerInMap(start, positions, climbToFirstActive: true, includeInactive, targetUtc)?.Dto;
    }

    public async Task<ChainOfCommandPositionDto?> ResolveDottedLineManagerAsync(
        string positionRefId,
        bool includeInactive,
        DateTimeOffset targetUtc,
        CancellationToken cancellationToken)
    {
        var start = await store.GetPositionByRefIdAsync(positionRefId, cancellationToken).ConfigureAwait(false);
        if (!start.DottedLinePositionId.HasValue)
        {
            return null;
        }

        var dotted = await store.GetPositionByIdAsync(start.DottedLinePositionId.Value, cancellationToken)
            .ConfigureAwait(false);

        return dotted is not null && ChainOfCommandPredicates.IsPositionEffective(dotted.Dto, targetUtc, includeInactive)
            ? dotted.Dto
            : null;
    }

    public async Task<IReadOnlyList<ChainOfCommandPositionDto>> ResolveReportingChainAsync(
        string positionRefId,
        int maxDepth,
        bool includeInactive,
        DateTimeOffset targetUtc,
        CancellationToken cancellationToken)
    {
        var start = await store.GetPositionByRefIdAsync(positionRefId, cancellationToken).ConfigureAwait(false);
        if (!start.ReportsToPositionId.HasValue)
        {
            return [];
        }

        var positions = await store.GetLegalEntityPositionsMapAsync(start.LegalEntityId, cancellationToken)
            .ConfigureAwait(false);

        var visited = new HashSet<Guid> { start.Id };
        return CollectDirectCandidates(start, positions, visited, maxDepth, targetUtc, includeInactive)
            .Select(c => c.Position.Dto)
            .ToList();
    }

    public async Task<IReadOnlyList<ChainOfCommandPositionDto>> GetDirectReportsAsync(
        string managerPositionRefId,
        bool includeInactive,
        DateTimeOffset targetUtc,
        CancellationToken cancellationToken)
    {
        var manager = await store.GetPositionByRefIdAsync(managerPositionRefId, cancellationToken).ConfigureAwait(false);
        var reports = await store.GetDirectReportsByManagerIdAsync(manager.Id, cancellationToken).ConfigureAwait(false);

        return reports
            .Where(p => ChainOfCommandPredicates.IsPositionEffective(p.Dto, targetUtc, includeInactive))
            .OrderByDescending(p => p.Dto.JobGradeRankOrder)
            .ThenBy(p => p.Dto.Code, StringComparer.Ordinal)
            .Select(p => p.Dto)
            .ToList();
    }

    public async Task<IReadOnlyList<ChainOfCommandPositionDto>> GetSubordinateSubtreeAsync(
        string managerPositionRefId,
        int maxDepth,
        bool includeInactive,
        DateTimeOffset targetUtc,
        CancellationToken cancellationToken)
    {
        var root = await store.GetPositionByRefIdAsync(managerPositionRefId, cancellationToken).ConfigureAwait(false);
        var allPositions = await store.GetLegalEntityPositionsMapAsync(root.LegalEntityId, cancellationToken)
            .ConfigureAwait(false);

        var childrenByManager = allPositions.Values
            .Where(p => p.ReportsToPositionId.HasValue)
            .GroupBy(p => p.ReportsToPositionId!.Value)
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(p => p.Dto.JobGradeRankOrder)
                    .ThenBy(p => p.Dto.Code, StringComparer.Ordinal)
                    .ToList());

        var results = new List<ChainOfCommandPositionDto>();
        var visited = new HashSet<Guid> { root.Id };
        var queue = new Queue<(Guid PositionId, int Depth)>();
        queue.Enqueue((root.Id, 0));

        while (queue.Count > 0)
        {
            var (currentId, depth) = queue.Dequeue();
            if (depth >= maxDepth || !childrenByManager.TryGetValue(currentId, out var children))
            {
                continue;
            }

            foreach (var child in children)
            {
                if (!visited.Add(child.Id))
                {
                    continue;
                }

                if (ChainOfCommandPredicates.IsPositionEffective(child.Dto, targetUtc, includeInactive))
                {
                    results.Add(child.Dto);
                }

                queue.Enqueue((child.Id, depth + 1));
            }
        }

        return results;
    }

    public static PositionProjection? FindDirectManagerInMap(
        PositionProjection start,
        IReadOnlyDictionary<Guid, PositionProjection> positions,
        bool climbToFirstActive,
        bool includeInactive,
        DateTimeOffset targetUtc)
    {
        if (!start.ReportsToPositionId.HasValue)
        {
            return null;
        }

        if (!climbToFirstActive)
        {
            return positions.TryGetValue(start.ReportsToPositionId.Value, out var direct) &&
                   ChainOfCommandPredicates.IsPositionEffective(direct.Dto, targetUtc, includeInactive)
                ? direct
                : null;
        }

        var visited = new HashSet<Guid> { start.Id };
        var cursor = start.ReportsToPositionId;

        while (cursor.HasValue && visited.Add(cursor.Value))
        {
            if (!positions.TryGetValue(cursor.Value, out var candidate))
            {
                break;
            }

            if (ChainOfCommandPredicates.IsPositionEffective(candidate.Dto, targetUtc, includeInactive))
            {
                return candidate;
            }

            cursor = candidate.ReportsToPositionId;
        }

        return null;
    }

    public static List<ManagerCandidate> CollectDirectCandidates(
        PositionProjection start,
        IReadOnlyDictionary<Guid, PositionProjection> positions,
        HashSet<Guid> visited,
        int maxDepth,
        DateTimeOffset targetUtc,
        bool includeInactive = false)
    {
        var candidates = new List<ManagerCandidate>();
        var cursor = start.ReportsToPositionId;

        while (cursor.HasValue && candidates.Count < maxDepth && visited.Add(cursor.Value))
        {
            if (!positions.TryGetValue(cursor.Value, out var manager))
            {
                break;
            }

            if (ChainOfCommandPredicates.IsPositionEffective(manager.Dto, targetUtc, includeInactive))
            {
                candidates.Add(new ManagerCandidate(ManagerResolutionSource.DirectReporting, manager, null));
            }

            cursor = manager.ReportsToPositionId;
        }

        return candidates;
    }
}
