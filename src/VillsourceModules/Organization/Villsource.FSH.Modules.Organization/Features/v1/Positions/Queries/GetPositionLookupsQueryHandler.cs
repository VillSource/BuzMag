using Mediator;
using Microsoft.EntityFrameworkCore;
using Villsource.FSH.Modules.Organization.Contracts.v1.Positions.Dtos;
using Villsource.FSH.Modules.Organization.Contracts.v1.Positions.Queries;
using Villsource.FSH.Modules.Organization.Data;

namespace Villsource.FSH.Modules.Organization.Features.v1.Positions.Queries;

public sealed class GetPositionLookupsQueryHandler(OrganizationDbContext dbContext)
    : IRequestHandler<GetPositionLookupsQuery, IReadOnlyList<PositionLookupDto>>
{
    public async ValueTask<IReadOnlyList<PositionLookupDto>> Handle(
        GetPositionLookupsQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var query = dbContext.Positions.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.LegalEntityRefId))
        {
            var leRef = request.LegalEntityRefId.Trim();
            query = query.Where(p => p.LegalEntity!.ReferenceId == leRef);
        }

        if (request.OrgUnitId.HasValue)
        {
            query = query.Where(p => p.OrgUnitId == request.OrgUnitId.Value);
        }

        if (request.ActiveOnly)
        {
            query = query.Where(p => p.IsActive);
        }

        if (request.LeadershipOnly)
        {
            query = query.Where(p => p.IsPrimaryLeadership);
        }

        return await query
            .OrderByDescending(p => p.JobGrade!.RankOrder)
            .ThenBy(p => p.Code)
            .Select(p => new PositionLookupDto(
                p.ReferenceId,
                p.LegalEntity!.ReferenceId,
                p.OrgUnitId,
                p.Code,
                p.Title,
                p.InternationalTitle,
                p.JobGrade!.Code,
                p.JobGrade!.RankOrder,
                p.IsPrimaryLeadership))
            .ToListAsync(cancellationToken);
    }
}