using FSH.Framework.Core.Exceptions;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Villsource.FSH.Modules.Organization.Contracts.v1.Positions.Dtos;
using Villsource.FSH.Modules.Organization.Contracts.v1.Positions.Queries;
using Villsource.FSH.Modules.Organization.Data;

namespace Villsource.FSH.Modules.Organization.Features.v1.Positions.Queries;

public sealed class GetPositionReportingChainQueryHandler(OrganizationDbContext dbContext)
    : IRequestHandler<GetPositionReportingChainQuery, IReadOnlyList<PositionNodeDto>>
{
    public async ValueTask<IReadOnlyList<PositionNodeDto>> Handle(
        GetPositionReportingChainQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var start = await dbContext.Positions
                        .AsNoTracking()
                        .Where(p => p.ReferenceId == request.RefId)
                        .Select(p => new { p.LegalEntityId, p.ReportsToPositionId })
                        .FirstOrDefaultAsync(cancellationToken)
                    ?? throw new NotFoundException($"Position '{request.RefId}' was not found.");

        if (!start.ReportsToPositionId.HasValue)
        {
            return [];
        }

        // Single-query projection of positions in the same LegalEntity (O(1) DB round-trip)
        var lookup = await dbContext.Positions
            .AsNoTracking()
            .Where(p => p.LegalEntityId == start.LegalEntityId)
            .Select(p => new
            {
                p.Id,
                p.ReportsToPositionId,
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
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        var chain = new List<PositionNodeDto>();
        var visited = new HashSet<Guid>();
        var cursor = start.ReportsToPositionId;

        while (cursor.HasValue && chain.Count < request.MaxDepth && visited.Add(cursor.Value))
        {
            if (!lookup.TryGetValue(cursor.Value, out var current))
            {
                break;
            }

            if (request.IncludeInactive || current.Node.IsActive)
            {
                chain.Add(current.Node);
            }

            cursor = current.ReportsToPositionId;
        }

        return chain;
    }
}