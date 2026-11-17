using FSH.Framework.Core.Exceptions;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Villsource.FSH.Modules.Organization.Contracts.v1.OrganizationUnits.Dtos;
using Villsource.FSH.Modules.Organization.Contracts.v1.OrganizationUnits.Queries;
using Villsource.FSH.Modules.Organization.Data;

namespace Villsource.FSH.Modules.Organization.Features.v1.OrganizationUnits;

public sealed class GetOrganizationUnitLeadershipChainQueryHandler(OrganizationDbContext dbContext)
    : IRequestHandler<GetOrganizationUnitLeadershipChainQuery, OrganizationUnitLeadershipChainDto>
{
    public async ValueTask<OrganizationUnitLeadershipChainDto> Handle(
        GetOrganizationUnitLeadershipChainQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var target = await dbContext.OrganizationUnits
                         .AsNoTracking()
                         .Where(e => e.ReferenceId == request.RefId)
                         .Select(e => new { e.ReferenceId, e.Code, e.Name, e.Depth, e.HierarchyPath })
                         .FirstOrDefaultAsync(cancellationToken)
                         .ConfigureAwait(false)
                     ?? throw new NotFoundException($"OrganizationUnit '{request.RefId}' was not found.");

        var ancestorRefIds = target.HierarchyPath
            .Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var nodes = await dbContext.OrganizationUnits
            .AsNoTracking()
            .Where(e => ancestorRefIds.Contains(e.ReferenceId))
            .OrderByDescending(e => e.Depth)
            .Select(e => new OrganizationUnitLeadershipNodeDto(
                target.Depth - e.Depth,
                e.Depth,
                e.ReferenceId,
                e.Code,
                e.Name,
                e.IsActive,
                e.ManagerPosition != null && !e.ManagerPosition.IsDeleted ? e.ManagerPosition.ReferenceId : null,
                e.ManagerPosition != null && !e.ManagerPosition.IsDeleted ? e.ManagerPosition.Code : null,
                e.ManagerPosition != null && !e.ManagerPosition.IsDeleted ? e.ManagerPosition.Title : null,
                e.ManagerPosition != null && !e.ManagerPosition.IsDeleted ? e.ManagerPosition.IsActive : null))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new OrganizationUnitLeadershipChainDto(
            TargetOuRefId: target.ReferenceId,
            TargetOuCode: target.Code,
            TargetOuName: target.Name,
            Chain: nodes);
    }
}