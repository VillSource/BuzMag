using FSH.Framework.Core.Exceptions;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Villsource.FSH.Modules.Organization.Contracts.v1.OrganizationUnits.Dtos;
using Villsource.FSH.Modules.Organization.Contracts.v1.OrganizationUnits.Queries;
using Villsource.FSH.Modules.Organization.Data;

namespace Villsource.FSH.Modules.Organization.Features.v1.OrganizationUnits.Queries;

public sealed class GetOrganizationTreeQueryHandler(OrganizationDbContext dbContext)
    : IRequestHandler<GetOrganizationTreeQuery, IReadOnlyList<OrganizationTreeNodeDto>>
{
    public async ValueTask<IReadOnlyList<OrganizationTreeNodeDto>> Handle(
        GetOrganizationTreeQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var legalEntity = await dbContext.LegalEntities
                              .AsNoTracking()
                              .Where(le => le.ReferenceId == request.LegalEntityRefId)
                              .Select(le => new { le.Id })
                              .FirstOrDefaultAsync(cancellationToken)
                              .ConfigureAwait(false)
                          ?? throw new NotFoundException($"Legal entity '{request.LegalEntityRefId}' was not found.");

        var query = dbContext.OrganizationUnits
            .AsNoTracking()
            .Where(ou => ou.LegalEntityId == legalEntity.Id && !ou.IsDeleted);

        if (request.ActiveOnly)
        {
            query = query.Where(ou => ou.IsActive);
        }

        var units = await query
            .Select(ou => new
            {
                ou.Id,
                ou.ReferenceId,
                ou.Code,
                ou.Name,
                ou.InternationalName,
                ou.Type,
                ou.Level,
                ou.HierarchyPath,
                ou.IsActive,
                ou.ParentId,
                DefaultCostCenterRefId = ou.DefaultCostCenter != null ? ou.DefaultCostCenter.ReferenceId : null,
                ManagerPositionRefId = ou.ManagerPosition != null && !ou.ManagerPosition.IsDeleted ? ou.ManagerPosition.ReferenceId : null,
                ManagerPositionCode = ou.ManagerPosition != null && !ou.ManagerPosition.IsDeleted ? ou.ManagerPosition.Code : null,
                ManagerPositionTitle = ou.ManagerPosition != null && !ou.ManagerPosition.IsDeleted ? ou.ManagerPosition.Title : null
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var lookup = units.ToLookup(u => u.ParentId);

        IReadOnlyList<OrganizationTreeNodeDto> BuildSubtree(Guid? parentId, int currentDepth)
        {
            if (request.MaxDepth.HasValue && currentDepth > request.MaxDepth.Value)
            {
                return [];
            }

            return lookup[parentId]
                .OrderBy(u => u.Code)
                .Select(u => new OrganizationTreeNodeDto(
                    RefId: u.ReferenceId,
                    Code: u.Code,
                    Name: u.Name,
                    InternationalName: u.InternationalName,
                    Type: u.Type,
                    Level: u.Level,
                    HierarchyPath: u.HierarchyPath,
                    IsActive: u.IsActive,
                    DefaultCostCenterRefId: u.DefaultCostCenterRefId,
                    ManagerPositionRefId: u.ManagerPositionRefId,
                    ManagerPositionCode: u.ManagerPositionCode,
                    ManagerPositionTitle: u.ManagerPositionTitle,
                    Children: BuildSubtree(u.Id, currentDepth + 1)))
                .ToList();
        }

        return BuildSubtree(null, 0);
    }
}
