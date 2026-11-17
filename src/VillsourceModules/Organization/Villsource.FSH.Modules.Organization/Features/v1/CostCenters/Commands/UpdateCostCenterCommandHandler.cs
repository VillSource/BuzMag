using FSH.Framework.Core.Exceptions;
using Mediator;
using Microsoft.EntityFrameworkCore;
using System.Net;
using Villsource.FSH.Modules.Organization.Contracts.Constants;
using Villsource.FSH.Modules.Organization.Contracts.v1.CostCenters.Commands;
using Villsource.FSH.Modules.Organization.Data;
using Villsource.FSH.Modules.Organization.Domain.CostCenters;

namespace Villsource.FSH.Modules.Organization.Features.v1.CostCenters.Commands;

public sealed class UpdateCostCenterCommandHandler(
    OrganizationDbContext dbContext)
    : IRequestHandler<UpdateCostCenterCommand, string>
{
    public async ValueTask<string> Handle(
        UpdateCostCenterCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var costCenter = await dbContext.CostCenters
                             .FirstOrDefaultAsync(
                                 predicate: c => c.ReferenceId == request.RefId,
                                 cancellationToken: cancellationToken)
                         ?? throw new NotFoundException($"Cost center '{request.RefId}' was not found.");

        var normalizedCode = CostCenterRules.NormalizeAndValidateCode(request.Code);

        if (!string.Equals(costCenter.Code, normalizedCode, StringComparison.Ordinal))
        {
            var codeExists = await dbContext.CostCenters
                .AnyAsync(
                    predicate: c => c.LegalEntityId == costCenter.LegalEntityId &&
                                    c.Code == normalizedCode &&
                                    c.Id != costCenter.Id,
                    cancellationToken: cancellationToken);

            if (codeExists)
            {
                throw new CustomException(
                    $"Cost center code '{normalizedCode}' already exists in this legal entity.", [], HttpStatusCode.Conflict);
            }
        }

        var parentCostCenterId = await CostCenterReferenceResolver.ResolveParentCostCenterIdAsync(
            dbContext: dbContext,
            legalEntityId: costCenter.LegalEntityId,
            parentCostCenterRefId: request.ParentCostCenterRefId,
            requireActiveParent: costCenter.IsActive,
            cancellationToken: cancellationToken);

        if (parentCostCenterId.HasValue)
        {
            await CostCenterReferenceResolver.EnsureNoHierarchyCycleAsync(
                dbContext: dbContext,
                legalEntityId: costCenter.LegalEntityId,
                costCenterId: costCenter.Id,
                proposedParentId: parentCostCenterId.Value,
                cancellationToken: cancellationToken);
        }

        var defaultTaxBranchId = await CostCenterReferenceResolver.ResolveDefaultTaxBranchIdAsync(
            dbContext: dbContext,
            legalEntityId: costCenter.LegalEntityId,
            defaultTaxBranchRefId: request.DefaultTaxBranchRefId,
            cancellationToken: cancellationToken);

        Guid? responsiblePositionId = null;
        if (!string.IsNullOrWhiteSpace(request.ResponsiblePositionRefId))
        {
            var position = await dbContext.Positions
                .AsNoTracking()
                .Where(p => p.ReferenceId == request.ResponsiblePositionRefId)
                .Select(p => new { p.Id, p.LegalEntityId, p.IsActive })
                .FirstOrDefaultAsync(cancellationToken: cancellationToken)
                .ConfigureAwait(false)
                ?? throw new NotFoundException($"Position '{request.ResponsiblePositionRefId}' was not found.");

            if (position.LegalEntityId != costCenter.LegalEntityId)
            {
                throw new InvalidOperationException("Responsible position must belong to the same legal entity as the cost center.");
            }

            if (!position.IsActive)
            {
                throw new InvalidOperationException($"Responsible position '{request.ResponsiblePositionRefId}' is inactive.");
            }

            responsiblePositionId = position.Id;
        }

        Guid? orgUnitId = await CostCenterReferenceResolver
            .ResolveOrgUnitIdAsync(
                dbContext: dbContext,
                legalEntityId: costCenter.LegalEntityId,
                orgUnitRefId: request.OrgUnitRefId,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        costCenter.Update(
            code: normalizedCode,
            name: request.Name,
            internationalName: request.InternationalName,
            functionType: CostCenterFunctionType.FromKey(request.FunctionType),
            description: request.Description,
            parentCostCenterId: parentCostCenterId,
            defaultTaxBranchId: defaultTaxBranchId,
            responsiblePositionId: responsiblePositionId,
            orgUnitId: orgUnitId,
            validFrom: request.ValidFrom,
            validTo: request.ValidTo);

        await dbContext.SaveChangesAsync(cancellationToken: cancellationToken);

        return costCenter.ReferenceId;
    }
}

