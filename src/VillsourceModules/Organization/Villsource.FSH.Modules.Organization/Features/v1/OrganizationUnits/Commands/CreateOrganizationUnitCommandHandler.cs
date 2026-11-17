using FSH.Framework.Core.Exceptions;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Villsource.FSH.Exceptions;
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

        var effectiveLegalEntityId = legalEntityId ?? parent?.LegalEntityId;
        var normalizedCode = request.Code.Trim().ToUpperInvariant();

        if (effectiveLegalEntityId.HasValue)
        {
            var codeExists = await dbContext.OrganizationUnits
                .AnyAsync(ou => ou.LegalEntityId == effectiveLegalEntityId.Value && ou.Code == normalizedCode && !ou.IsDeleted, cancellationToken)
                .ConfigureAwait(false);

            if (codeExists)
            {
                throw new ConflictException($"Organization unit code '{normalizedCode}' already exists in this legal entity.");
            }
        }

        Guid? defaultCostCenterId = null;
        if (!string.IsNullOrWhiteSpace(request.DefaultCostCenterRefId))
        {
            var costCenter = await dbContext.CostCenters
                                 .Where(c => c.ReferenceId == request.DefaultCostCenterRefId)
                                 .Select(c => new { c.Id, c.LegalEntityId, c.IsActive })
                                 .FirstOrDefaultAsync(cancellationToken)
                                 .ConfigureAwait(false)
                             ?? throw new NotFoundException($"Cost center '{request.DefaultCostCenterRefId}' was not found.");

            if (effectiveLegalEntityId.HasValue && costCenter.LegalEntityId != effectiveLegalEntityId.Value)
            {
                throw new InvalidOperationException("Cost center must belong to the same legal entity as the organization unit.");
            }

            if (!costCenter.IsActive)
            {
                throw new InvalidOperationException($"Cost center '{request.DefaultCostCenterRefId}' is inactive.");
            }

            defaultCostCenterId = costCenter.Id;
        }

        Position? managerPosition = null;
        if (!string.IsNullOrWhiteSpace(request.ManagerPositionRefId))
        {
            managerPosition = await dbContext.Positions
                                  .FirstOrDefaultAsync(p => p.ReferenceId == request.ManagerPositionRefId, cancellationToken)
                                  .ConfigureAwait(false)
                              ?? throw new NotFoundException($"Manager Position '{request.ManagerPositionRefId}' was not found.");
        }

        var unitType = await dbContext.OrganizationUnitTypes
            .AsNoTracking()
            .Where(t => t.ReferenceId == request.OrganizationUnitTypeRefId)
            .Select(t => new { t.Id, t.IsActive })
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Organization unit type '{request.OrganizationUnitTypeRefId}' was not found.");

        if (!unitType.IsActive)
        {
            throw new InvalidOperationException($"Organization unit type '{request.OrganizationUnitTypeRefId}' is inactive.");
        }

        var entity = OrganizationUnit.Create(
            code: normalizedCode,
            name: request.Name,
            internationalName: request.InternationalName,
            organizationUnitTypeId: unitType.Id,
            description: request.Description,
            legalEntityId: legalEntityId,
            parent: parent,
            defaultCostCenterId: defaultCostCenterId,
            managerPosition: managerPosition,
            allowExternalManagerPosition: request.AllowExternalManagerPosition);

        await dbContext.OrganizationUnits.AddAsync(entity, cancellationToken).ConfigureAwait(false);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return entity.ReferenceId;
    }
}