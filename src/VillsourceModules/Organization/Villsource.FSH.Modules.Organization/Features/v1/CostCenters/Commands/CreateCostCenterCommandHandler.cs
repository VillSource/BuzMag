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

        var costCenter = CostCenter.Create(
            legalEntityId: legalEntityId,
            code: normalizedCode,
            name: request.Name,
            internationalName: request.InternationalName,
            functionType: CostCenterFunctionType.FromKey(request.FunctionType),
            description: request.Description,
            parentCostCenterId: parentCostCenterId,
            defaultTaxBranchId: defaultTaxBranchId,
            responsibleUserId: request.ResponsibleUserId,
            orgUnitId: request.OrgUnitId,
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

