using FSH.Framework.Core.Exceptions;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Villsource.FSH.Modules.Organization.Contracts.v1.OrganizationUnits.Commands;
using Villsource.FSH.Modules.Organization.Data;
using Villsource.FSH.Modules.Organization.Domain.OrganizationUnits;
using Villsource.FSH.Modules.Organization.Features.v1.Positions;
using Position = Villsource.FSH.Modules.Organization.Domain.Positions.Position;

namespace Villsource.FSH.Modules.Organization.Features.v1.OrganizationUnits;

public sealed class CreateOrganizationUnitCommandHandler(OrganizationDbContext dbContext)
    : IRequestHandler<CreateOrganizationUnitCommand, string>
{
    public async ValueTask<string> Handle(
        CreateOrganizationUnitCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        OrganizationUnit? parent = null;
        if (!string.IsNullOrWhiteSpace(request.ParentRefId))
        {
            parent = await dbContext.OrganizationUnits
                         .FirstOrDefaultAsync(e => e.ReferenceId == request.ParentRefId, cancellationToken)
                         .ConfigureAwait(false)
                     ?? throw new NotFoundException($"Parent OrganizationUnit '{request.ParentRefId}' was not found.");
        }

        Guid? legalEntityId = null;
        if (!string.IsNullOrWhiteSpace(request.LegalEntityRefId))
        {
            var legalEntity = await dbContext.LegalEntities
                                  .Where(le => le.ReferenceId == request.LegalEntityRefId)
                                  .Select(le => new { le.Id })
                                  .FirstOrDefaultAsync(cancellationToken)
                                  .ConfigureAwait(false)
                              ?? throw new NotFoundException($"LegalEntity '{request.LegalEntityRefId}' was not found.");
            legalEntityId = legalEntity.Id;
        }

        Position? managerPosition = null;
        if (!string.IsNullOrWhiteSpace(request.ManagerPositionRefId))
        {
            managerPosition = await dbContext.Positions
                                  .FirstOrDefaultAsync(p => p.ReferenceId == request.ManagerPositionRefId, cancellationToken)
                                  .ConfigureAwait(false)
                              ?? throw new NotFoundException($"Manager Position '{request.ManagerPositionRefId}' was not found.");
        }

        var entity = OrganizationUnit.Create(
            code: request.Code,
            name: request.Name,
            internationalName: request.InternationalName,
            type: request.Type,
            description: request.Description,
            legalEntityId: legalEntityId,
            legalEntityRefId: request.LegalEntityRefId,
            parent: parent,
            costCenterCode: request.CostCenterCode,
            managerPosition: managerPosition,
            allowExternalManagerPosition: request.AllowExternalManagerPosition);

        await dbContext.OrganizationUnits.AddAsync(entity, cancellationToken).ConfigureAwait(false);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return entity.ReferenceId;
    }
}