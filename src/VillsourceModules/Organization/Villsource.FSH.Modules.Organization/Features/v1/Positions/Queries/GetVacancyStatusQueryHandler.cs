using FSH.Framework.Core.Exceptions;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Villsource.FSH.Modules.Organization.Contracts.v1.Positions.Dtos;
using Villsource.FSH.Modules.Organization.Contracts.v1.Positions.Queries;
using Villsource.FSH.Modules.Organization.Data;

namespace Villsource.FSH.Modules.Organization.Features.v1.Positions.Queries;

public sealed class GetVacancyStatusQueryHandler(OrganizationDbContext dbContext)
    : IRequestHandler<GetVacancyStatusQuery, IReadOnlyList<PositionVacancyDto>>
{
    public async ValueTask<IReadOnlyList<PositionVacancyDto>> Handle(
        GetVacancyStatusQuery request,
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

        Guid? orgUnitId = null;
        if (!string.IsNullOrWhiteSpace(request.OrgUnitRefId))
        {
            var ou = await dbContext.OrganizationUnits
                         .AsNoTracking()
                         .Where(o => o.ReferenceId == request.OrgUnitRefId)
                         .Select(o => new { o.Id, o.LegalEntityId })
                         .FirstOrDefaultAsync(cancellationToken)
                         .ConfigureAwait(false)
                     ?? throw new NotFoundException($"OrganizationUnit '{request.OrgUnitRefId}' was not found.");

            if (ou.LegalEntityId != legalEntity.Id)
            {
                throw new InvalidOperationException("Organization unit does not belong to the specified legal entity.");
            }

            orgUnitId = ou.Id;
        }

        var query = dbContext.Positions
            .AsNoTracking()
            .Where(p => p.LegalEntityId == legalEntity.Id && !p.IsDeleted);

        if (orgUnitId.HasValue)
        {
            query = query.Where(p => p.OrgUnitId == orgUnitId.Value);
        }

        if (request.ActiveOnly)
        {
            query = query.Where(p => p.IsActive);
        }

        var positions = await query
            .OrderBy(p => p.Code)
            .Select(p => new
            {
                p.ReferenceId,
                p.Code,
                p.Title,
                OrgUnitRefId = p.OrgUnit != null ? p.OrgUnit.ReferenceId : null,
                p.HeadcountCapacity,
                p.IsActive
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        // Note: FilledCount is currently 0 until integration with HR/Employee module is established.
        return positions.Select(p =>
        {
            const int filledCount = 0;
            var vacancyCount = Math.Max(0, p.HeadcountCapacity - filledCount);
            return new PositionVacancyDto(
                PositionRefId: p.ReferenceId,
                PositionCode: p.Code,
                Title: p.Title,
                OrgUnitRefId: p.OrgUnitRefId,
                HeadcountCapacity: p.HeadcountCapacity,
                FilledCount: filledCount,
                VacancyCount: vacancyCount,
                IsVacant: vacancyCount > 0,
                IsActive: p.IsActive);
        }).ToList();
    }
}
