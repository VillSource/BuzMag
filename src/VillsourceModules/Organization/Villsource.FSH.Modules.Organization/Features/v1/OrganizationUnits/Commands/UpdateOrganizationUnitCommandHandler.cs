using FSH.Framework.Core.Exceptions;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Villsource.FSH.Exceptions;
using Villsource.FSH.Modules.Organization.Contracts.v1.OrganizationUnits.Commands;
using Villsource.FSH.Modules.Organization.Data;

namespace Villsource.FSH.Modules.Organization.Features.v1.OrganizationUnits;

public sealed class UpdateOrganizationUnitCommandHandler(OrganizationDbContext dbContext)
    : IRequestHandler<UpdateOrganizationUnitCommand, string>
{
    public async ValueTask<string> Handle(
        UpdateOrganizationUnitCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var entity = await dbContext.OrganizationUnits
                         .FirstOrDefaultAsync(e => e.ReferenceId == request.RefId, cancellationToken)
                         .ConfigureAwait(false)
                     ?? throw new NotFoundException($"OrganizationUnit '{request.RefId}' was not found.");

        var normalizedCode = request.Code.Trim().ToUpperInvariant();
        if (entity.LegalEntityId.HasValue && !string.Equals(entity.Code, normalizedCode, StringComparison.Ordinal))
        {
            var codeExists = await dbContext.OrganizationUnits
                .AnyAsync(ou => ou.LegalEntityId == entity.LegalEntityId.Value && ou.Code == normalizedCode && ou.Id != entity.Id && !ou.IsDeleted, cancellationToken)
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

            if (entity.LegalEntityId.HasValue && costCenter.LegalEntityId != entity.LegalEntityId.Value)
            {
                throw new InvalidOperationException("Cost center must belong to the same legal entity as the organization unit.");
            }

            if (!costCenter.IsActive)
            {
                throw new InvalidOperationException($"Cost center '{request.DefaultCostCenterRefId}' is inactive.");
            }

            defaultCostCenterId = costCenter.Id;
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

        entity.Update(
            code: normalizedCode,
            name: request.Name,
            internationalName: request.InternationalName,
            organizationUnitTypeId: unitType.Id,
            description: request.Description,
            defaultCostCenterId: defaultCostCenterId);

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return entity.ReferenceId;
    }
}