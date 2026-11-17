using FSH.Framework.Core.Exceptions;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Villsource.FSH.Exceptions;
using Villsource.FSH.Modules.Organization.Contracts.v1.Positions.Commands;
using Villsource.FSH.Modules.Organization.Data;

namespace Villsource.FSH.Modules.Organization.Features.v1.Positions.Commands;

public sealed class CreatePositionCommandHandler(OrganizationDbContext dbContext)
    : IRequestHandler<CreatePositionCommand, string>
{
    public async ValueTask<string> Handle(CreatePositionCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var normalizedCode = Domain.Positions.PositionRules.NormalizeAndValidateCode(request.Code);

        // Single DB round-trip: resolve LegalEntity + check duplicate Position code in that entity
        var leLookup = await dbContext.LegalEntities
                           .AsNoTracking()
                           .Where(le => le.ReferenceId == request.LegalEntityRefId)
                           .Select(le => new
                           {
                               le.Id,
                               CodeExists = dbContext.Positions.Any(p => p.LegalEntityId == le.Id && p.Code == normalizedCode)
                           })
                           .FirstOrDefaultAsync(cancellationToken)
                           .ConfigureAwait(false)
                       ?? throw new NotFoundException($"Legal entity '{request.LegalEntityRefId}' was not found.");

        if (leLookup.CodeExists)
        {
            throw new ConflictException(
                $"Position code '{normalizedCode}' already exists in legal entity '{request.LegalEntityRefId}'.");
        }

        var job = await dbContext.Jobs
                      .AsNoTracking()
                      .Where(j => j.ReferenceId == request.JobRefId)
                      .Select(j => new { j.Id, j.DefaultJobGradeId, j.IsActive })
                      .FirstOrDefaultAsync(cancellationToken)
                  ?? throw new NotFoundException($"Job '{request.JobRefId}' was not found.");

        if (!job.IsActive)
        {
            throw new InvalidOperationException($"Job '{request.JobRefId}' is inactive.");
        }

        var jobGradeId = job.DefaultJobGradeId;
        if (!string.IsNullOrWhiteSpace(request.JobGradeRefId))
        {
            var grade = await dbContext.JobGrades
                            .AsNoTracking()
                            .Where(g => g.ReferenceId == request.JobGradeRefId)
                            .Select(g => new { g.Id, g.IsActive })
                            .FirstOrDefaultAsync(cancellationToken)
                        ?? throw new NotFoundException($"Job grade '{request.JobGradeRefId}' was not found.");

            if (!grade.IsActive)
            {
                throw new InvalidOperationException($"Job grade '{request.JobGradeRefId}' is inactive.");
            }

            jobGradeId = grade.Id;
        }

        var (orgUnitId, defaultCostCenterId) = await PositionReferenceResolver.ResolveOrgUnitAsync(
            dbContext, leLookup.Id, request.OrgUnitRefId, cancellationToken).ConfigureAwait(false);

        var costCenterId = await PositionReferenceResolver.ResolveCostCenterIdAsync(
            dbContext, leLookup.Id, request.CostCenterRefId, cancellationToken).ConfigureAwait(false);

        costCenterId ??= defaultCostCenterId;

        var workLocationTaxBranchId = await PositionReferenceResolver.ResolveWorkLocationTaxBranchIdAsync(
            dbContext, leLookup.Id, request.WorkLocationTaxBranchRefId, cancellationToken).ConfigureAwait(false);

        var (reportsToPositionId, dottedLinePositionId) =
            await PositionReferenceResolver.ResolveReportingTargetsAsync(
                dbContext,
                leLookup.Id,
                request.ReportsToPositionRefId,
                request.DottedLinePositionRefId,
                request.IsActive,
                cancellationToken).ConfigureAwait(false);

        var position = Domain.Positions.Position.Create(
            legalEntityId: leLookup.Id,
            orgUnitId: orgUnitId,
            jobId: job.Id,
            jobGradeId: jobGradeId,
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
            validTo: request.ValidTo,
            isActive: request.IsActive);

        await dbContext.Positions.AddAsync(position, cancellationToken).ConfigureAwait(false);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return position.ReferenceId;
    }
}