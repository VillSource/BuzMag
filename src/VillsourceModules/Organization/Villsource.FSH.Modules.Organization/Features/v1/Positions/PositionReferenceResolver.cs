using FSH.Framework.Core.Exceptions;
using Microsoft.EntityFrameworkCore;
using Villsource.FSH.Modules.Organization.Data;

namespace Villsource.FSH.Modules.Organization.Features.v1.Positions;

internal static class PositionReferenceResolver
{
    public static async Task<Guid?> ResolveCostCenterIdAsync(
        OrganizationDbContext dbContext,
        Guid legalEntityId,
        string? costCenterRefId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(costCenterRefId))
        {
            return null;
        }

        var cc = await dbContext.CostCenters
                     .AsNoTracking()
                     .Where(c => c.ReferenceId == costCenterRefId)
                     .Select(c => new { c.Id, c.LegalEntityId, c.IsActive })
                     .FirstOrDefaultAsync(cancellationToken)
                 ?? throw new NotFoundException($"Cost center '{costCenterRefId}' was not found.");

        if (cc.LegalEntityId != legalEntityId)
        {
            throw new InvalidOperationException("Cost center must belong to the same legal entity.");
        }

        if (!cc.IsActive)
        {
            throw new InvalidOperationException($"Cost center '{costCenterRefId}' is inactive.");
        }

        return cc.Id;
    }

    /// <summary>
    /// Resolves both ReportsToPositionRefId and DottedLinePositionRefId in a single database query.
    /// </summary>
    public static async Task<(Guid? ReportsToId, Guid? DottedLineId)> ResolveReportingTargetsAsync(
        OrganizationDbContext dbContext,
        Guid legalEntityId,
        string? reportsToRefId,
        string? dottedLineRefId,
        bool requireActiveTarget,
        CancellationToken cancellationToken)
    {
        var hasReportsTo = !string.IsNullOrWhiteSpace(reportsToRefId);
        var hasDottedLine = !string.IsNullOrWhiteSpace(dottedLineRefId);

        if (!hasReportsTo && !hasDottedLine)
        {
            return (null, null);
        }

        var targetRefIds = new List<string>(2);
        if (hasReportsTo) targetRefIds.Add(reportsToRefId!.Trim());
        if (hasDottedLine) targetRefIds.Add(dottedLineRefId!.Trim());

        var targets = await dbContext.Positions
            .AsNoTracking()
            .Where(p => targetRefIds.Contains(p.ReferenceId))
            .Select(p => new { p.Id, p.ReferenceId, p.LegalEntityId, p.IsActive })
            .ToDictionaryAsync(p => p.ReferenceId, StringComparer.Ordinal, cancellationToken);

        Guid? resolvedReportsToId = null;
        if (hasReportsTo)
        {
            var key = reportsToRefId!.Trim();
            if (!targets.TryGetValue(key, out var reportsTo))
            {
                throw new NotFoundException($"Reporting position '{key}' was not found.");
            }

            if (reportsTo.LegalEntityId != legalEntityId)
            {
                throw new InvalidOperationException("Reporting position must belong to the same legal entity.");
            }

            if (requireActiveTarget && !reportsTo.IsActive)
            {
                throw new InvalidOperationException($"Reporting position '{key}' is inactive.");
            }

            resolvedReportsToId = reportsTo.Id;
        }

        Guid? resolvedDottedLineId = null;
        if (hasDottedLine)
        {
            var key = dottedLineRefId!.Trim();
            if (!targets.TryGetValue(key, out var dottedLine))
            {
                throw new NotFoundException($"Dotted-line position '{key}' was not found.");
            }

            if (dottedLine.LegalEntityId != legalEntityId)
            {
                throw new InvalidOperationException("Dotted-line position must belong to the same legal entity.");
            }

            if (requireActiveTarget && !dottedLine.IsActive)
            {
                throw new InvalidOperationException($"Dotted-line position '{key}' is inactive.");
            }

            resolvedDottedLineId = dottedLine.Id;
        }

        return (resolvedReportsToId, resolvedDottedLineId);
    }

    public static async Task EnsureNoReportingCycleAsync(
        OrganizationDbContext dbContext,
        Guid legalEntityId,
        Guid positionId,
        Guid proposedReportsToId,
        CancellationToken cancellationToken)
    {
        if (positionId == proposedReportsToId)
        {
            throw new InvalidOperationException("A position cannot report to itself.");
        }

        var reportingMap = await dbContext.Positions
            .AsNoTracking()
            .Where(p => p.LegalEntityId == legalEntityId && p.ReportsToPositionId != null)
            .Select(p => new { p.Id, ManagerId = p.ReportsToPositionId!.Value })
            .ToDictionaryAsync(x => x.Id, x => x.ManagerId, cancellationToken);

        var visited = new HashSet<Guid> { positionId };
        Guid? cursor = proposedReportsToId;

        while (cursor.HasValue)
        {
            if (!visited.Add(cursor.Value))
            {
                throw new InvalidOperationException(
                    "Assigning this reporting position would create a circular reporting hierarchy.");
            }

            cursor = reportingMap.TryGetValue(cursor.Value, out var nextManagerId)
                ? nextManagerId
                : null;
        }
    }
}