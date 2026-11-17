using FluentValidation;
using FSH.Framework.Core.Exceptions;
using Mediator;
using Microsoft.EntityFrameworkCore;
using System.Net;
using Villsource.FSH.Modules.Organization.Contracts.Constants;
using Villsource.FSH.Modules.Organization.Contracts.v1.CostCenters.Commands;
using Villsource.FSH.Modules.Organization.Data;
using Villsource.FSH.Modules.Organization.Domain.CostCenters;

namespace Villsource.FSH.Modules.Organization.Features.v1.CostCenters.Commands;

public sealed class CreateCostCenterCommandHandler(
    OrganizationDbContext dbContext)
    : IRequestHandler<CreateCostCenterCommand, string>
{
    public async ValueTask<string> Handle(
        CreateCostCenterCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var legalEntityId = await dbContext.LegalEntities
            .AsNoTracking()
            .Where(le => le.ReferenceId == request.LegalEntityRefId)
            .Select(le => (Guid?)le.Id)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken)
            ?? throw new NotFoundException($"Legal entity '{request.LegalEntityRefId}' was not found.");

        var normalizedCode = CostCenterRules.NormalizeAndValidateCode(request.Code);

        var codeExists = await dbContext.CostCenters
            .AnyAsync(
                predicate: c => c.LegalEntityId == legalEntityId && c.Code == normalizedCode,
                cancellationToken: cancellationToken);

        if (codeExists)
        {
            throw new CustomException(
                $"Cost center code '{normalizedCode}' already exists in legal entity '{request.LegalEntityRefId}'.", [], HttpStatusCode.Conflict);
        }

        var parentCostCenterId = await CostCenterReferenceResolver.ResolveParentCostCenterIdAsync(
            dbContext: dbContext,
            legalEntityId: legalEntityId,
            parentCostCenterRefId: request.ParentCostCenterRefId,
            requireActiveParent: request.IsActive,
            cancellationToken: cancellationToken);

        var defaultTaxBranchId = await CostCenterReferenceResolver.ResolveDefaultTaxBranchIdAsync(
            dbContext: dbContext,
            legalEntityId: legalEntityId,
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

            if (position.LegalEntityId != legalEntityId)
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
                legalEntityId: legalEntityId,
                orgUnitRefId: request.OrgUnitRefId,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        var costCenter = CostCenter.Create(
            legalEntityId: legalEntityId,
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
            validTo: request.ValidTo,
            isActive: request.IsActive,
            isPostingBlocked: request.IsPostingBlocked);

        await dbContext.CostCenters.AddAsync(
            entity: costCenter,
            cancellationToken: cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken: cancellationToken);

        return costCenter.ReferenceId;
    }
}

