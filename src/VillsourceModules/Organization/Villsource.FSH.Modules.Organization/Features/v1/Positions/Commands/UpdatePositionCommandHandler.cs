using FSH.Framework.Core.Exceptions;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Villsource.FSH.Exceptions;
using Villsource.FSH.Modules.Organization.Contracts.v1.Positions.Commands;
using Villsource.FSH.Modules.Organization.Data;

namespace Villsource.FSH.Modules.Organization.Features.v1.Positions.Commands;

public sealed class UpdatePositionCommandHandler(OrganizationDbContext dbContext)
    : IRequestHandler<UpdatePositionCommand, string>
{
    public async ValueTask<string> Handle(UpdatePositionCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var normalizedCode = Domain.Positions.PositionRules.NormalizeAndValidateCode(request.Code);

        // Single DB round-trip: load tracked Position + scalar checks for duplicate Code, Job, and JobGrade
        var target = await dbContext.Positions
                         .Where(p => p.ReferenceId == request.RefId)
                         .Select(p => new
                         {
                             Entity = p,
                             CodeExists = p.Code != normalizedCode &&
                                          dbContext.Positions.Any(other =>
                                              other.LegalEntityId == p.LegalEntityId &&
                                              other.Code == normalizedCode &&
                                              other.Id != p.Id),
                             JobId = dbContext.Jobs
                                 .Where(j => j.ReferenceId == request.JobRefId)
                                 .Select(j => (Guid?)j.Id)
                                 .FirstOrDefault(),
                             JobIsActive = dbContext.Jobs
                                 .Where(j => j.ReferenceId == request.JobRefId)
                                 .Select(j => (bool?)j.IsActive)
                                 .FirstOrDefault(),
                             GradeId = dbContext.JobGrades
                                 .Where(g => g.ReferenceId == request.JobGradeRefId)
                                 .Select(g => (Guid?)g.Id)
                                 .FirstOrDefault(),
                             GradeIsActive = dbContext.JobGrades
                                 .Where(g => g.ReferenceId == request.JobGradeRefId)
                                 .Select(g => (bool?)g.IsActive)
                                 .FirstOrDefault()
                         })
                         .FirstOrDefaultAsync(cancellationToken)
                         .ConfigureAwait(false)
                     ?? throw new NotFoundException($"Position '{request.RefId}' was not found.");

        if (target.CodeExists)
        {
            throw new ConflictException($"Position code '{normalizedCode}' already exists in this legal entity.");
        }

        if (!target.JobId.HasValue)
        {
            throw new NotFoundException($"Job '{request.JobRefId}' was not found.");
        }

        if (target.JobIsActive != true)
        {
            throw new InvalidOperationException($"Job '{request.JobRefId}' is inactive.");
        }

        if (!target.GradeId.HasValue)
        {
            throw new NotFoundException($"Job grade '{request.JobGradeRefId}' was not found.");
        }

        if (target.GradeIsActive != true)
        {
            throw new InvalidOperationException($"Job grade '{request.JobGradeRefId}' is inactive.");
        }

        var position = target.Entity;

        var (orgUnitId, defaultCostCenterId) = await PositionReferenceResolver.ResolveOrgUnitAsync(
            dbContext, position.LegalEntityId, request.OrgUnitRefId, cancellationToken).ConfigureAwait(false);

        // If position is leaving its previous OU, clear its manager role on the old OU unless reassigned
        if (position.OrgUnitId != orgUnitId)
        {
            var oldManagedOu = await dbContext.OrganizationUnits
                .FirstOrDefaultAsync(ou => ou.Id == position.OrgUnitId && ou.ManagerPositionId == position.Id && !ou.IsDeleted, cancellationToken)
                .ConfigureAwait(false);

            oldManagedOu?.AssignManagerPosition(null);
        }

        var costCenterId = await PositionReferenceResolver.ResolveCostCenterIdAsync(
            dbContext, position.LegalEntityId, request.CostCenterRefId, cancellationToken).ConfigureAwait(false);

        costCenterId ??= defaultCostCenterId;

        var workLocationTaxBranchId = await PositionReferenceResolver.ResolveWorkLocationTaxBranchIdAsync(
            dbContext, position.LegalEntityId, request.WorkLocationTaxBranchRefId, cancellationToken).ConfigureAwait(false);

        var (reportsToPositionId, dottedLinePositionId) =
            await PositionReferenceResolver.ResolveReportingTargetsAsync(
                dbContext,
                position.LegalEntityId,
                request.ReportsToPositionRefId,
                request.DottedLinePositionRefId,
                position.IsActive,
                cancellationToken).ConfigureAwait(false);

        if (reportsToPositionId.HasValue && reportsToPositionId != position.ReportsToPositionId)
        {
            await PositionReferenceResolver.EnsureNoReportingCycleAsync(
                dbContext, position.LegalEntityId, position.Id, reportsToPositionId.Value, cancellationToken).ConfigureAwait(false);
        }

        var changed = position.Update(
            orgUnitId: orgUnitId,
            jobId: target.JobId.Value,
            jobGradeId: target.GradeId.Value,
            code: normalizedCode,
            title: request.Title,
            internationalTitle: request.InternationalTitle,
            description: request.Description,
            costCenterId: costCenterId,
            workLocationTaxBranchId: workLocationTaxBranchId,
            approvalLimitAmount: request.ApprovalLimitAmount,
            approvalLimitCurrency: request.ApprovalLimitCurrency,
            reportsToPositionId: reportsToPositionId,
            dottedLinePositionId: dottedLinePositionId,
            headcountCapacity: request.HeadcountCapacity,
            isPrimaryLeadership: request.IsPrimaryLeadership,
            validFrom: request.ValidFrom,
            validTo: request.ValidTo);

        if (changed)
        {
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        return position.ReferenceId;
    }
}