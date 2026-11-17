using FSH.Framework.Core.Exceptions;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Villsource.FSH.Modules.Organization.Contracts.v1.Positions.Dtos;
using Villsource.FSH.Modules.Organization.Contracts.v1.Positions.Queries;
using Villsource.FSH.Modules.Organization.Data;

namespace Villsource.FSH.Modules.Organization.Features.v1.Positions.Queries;

public sealed class GetPositionSubtreeQueryHandler(OrganizationDbContext dbContext)
    : IRequestHandler<GetPositionSubtreeQuery, IReadOnlyList<PositionNodeDto>>
{
    public async ValueTask<IReadOnlyList<PositionNodeDto>> Handle(
        GetPositionSubtreeQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var root = await dbContext.Positions
                       .AsNoTracking()
                       .Where(p => p.ReferenceId == request.RefId)
                       .Select(p => new { p.Id, p.LegalEntityId })
                       .FirstOrDefaultAsync(cancellationToken)
                   ?? throw new NotFoundException($"Position '{request.RefId}' was not found.");

        // Single-query projection of all positions in the same LegalEntity (O(1) DB round-trip)
        var allPositions = await dbContext.Positions
            .AsNoTracking()
            .Where(p => p.LegalEntityId == root.LegalEntityId && p.ReportsToPositionId != null)
            .Select(p => new
            {
                p.Id,
                ManagerId = p.ReportsToPositionId!.Value,
                Node = new PositionNodeDto(
                    p.ReferenceId,
                    p.Code,
                    p.Title,
                    p.InternationalTitle,
                    p.JobGrade!.Code,
                    p.JobGrade!.RankOrder,
                    p.IsPrimaryLeadership,
                    p.IsActive)
            })
            .ToListAsync(cancellationToken);

        var childrenByManager = allPositions
            .GroupBy(x => x.ManagerId)
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(x => x.Node.JobGradeRankOrder)
                    .ThenBy(x => x.Node.Code)
                    .ToList());

        var results = new List<PositionNodeDto>();
        var visited = new HashSet<Guid> { root.Id };
        var queue = new Queue<(Guid PositionId, int Depth)>();
        queue.Enqueue((root.Id, 0));

        while (queue.Count > 0)
        {
            var (currentId, depth) = queue.Dequeue();
            if (depth >= request.MaxDepth || !childrenByManager.TryGetValue(currentId, out var children))
            {
                continue;
            }

            foreach (var child in children)
            {
                if (!visited.Add(child.Id))
                {
                    continue;
                }

                if (request.IncludeInactive || child.Node.IsActive)
                {
                    results.Add(child.Node);
                }

                queue.Enqueue((child.Id, depth + 1));
            }
        }

        return results;
    }
}