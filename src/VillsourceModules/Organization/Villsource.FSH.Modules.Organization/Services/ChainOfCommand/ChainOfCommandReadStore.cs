using FSH.Framework.Core.Exceptions;
using Microsoft.EntityFrameworkCore;
using Villsource.FSH.Modules.Organization.Contracts.Constants;
using Villsource.FSH.Modules.Organization.Contracts.Services.Dtos;
using Villsource.FSH.Modules.Organization.Data;
using Villsource.FSH.Modules.Organization.Domain.PositionDelegations;
using Position = Villsource.FSH.Modules.Organization.Domain.Positions.Position;

namespace Villsource.FSH.Modules.Organization.Services.ChainOfCommand;

internal sealed class ChainOfCommandReadStore(OrganizationDbContext dbContext)
{
    private sealed record PositionRow(
        Guid Id,
        Guid LegalEntityId,
        Guid OrgUnitId,
        Guid? ReportsToPositionId,
        Guid? DottedLinePositionId,
        string ReferenceId,
        string Code,
        string Title,
        string InternationalTitle,
        string LegalEntityRefId,
        string OrgUnitRefId,
        string OrgUnitCode,
        string OrgUnitName,
        string JobCode,
        string JobGradeCode,
        int JobGradeRankOrder,
        JobLevelCategory? JobLevelCategory,
        decimal? ApprovalLimitAmount,
        string? ApprovalLimitCurrency,
        bool IsPrimaryLeadership,
        bool IsActive,
        DateTimeOffset? ValidFrom,
        DateTimeOffset? ValidTo);

    public async Task<PositionProjection> GetPositionByRefIdAsync(
        string positionRefId,
        CancellationToken cancellationToken)
    {
        var normalized = positionRefId.Trim();
        var row = await ProjectPositionRows(
                dbContext.Positions
                    .AsNoTracking()
                    .Where(p => p.ReferenceId == normalized))
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Position '{positionRefId}' was not found.");

        return ToProjection(row);
    }

    public async Task<PositionProjection?> GetPositionByIdAsync(
        Guid positionId,
        CancellationToken cancellationToken)
    {
        var row = await ProjectPositionRows(
                dbContext.Positions
                    .AsNoTracking()
                    .Where(p => p.Id == positionId))
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        return row is null ? null : ToProjection(row);
    }

    public async Task<Dictionary<Guid, PositionProjection>> GetPositionsByIdsAsync(
        IReadOnlyCollection<Guid> positionIds,
        CancellationToken cancellationToken)
    {
        var rows = await ProjectPositionRows(
                dbContext.Positions
                    .AsNoTracking()
                    .Where(p => positionIds.Contains(p.Id)))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return rows.Select(ToProjection).ToDictionary(p => p.Id);
    }

    public async Task<Dictionary<Guid, PositionProjection>> GetLegalEntityPositionsMapAsync(
        Guid legalEntityId,
        CancellationToken cancellationToken)
    {
        var rows = await ProjectPositionRows(
                dbContext.Positions
                    .AsNoTracking()
                    .Where(p => p.LegalEntityId == legalEntityId))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return rows.Select(ToProjection).ToDictionary(p => p.Id);
    }

    public async Task<List<PositionProjection>> GetDirectReportsByManagerIdAsync(
        Guid managerPositionId,
        CancellationToken cancellationToken)
    {
        var rows = await ProjectPositionRows(
                dbContext.Positions
                    .AsNoTracking()
                    .Where(p => p.ReportsToPositionId == managerPositionId))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return rows.Select(ToProjection).ToList();
    }

    public async Task<OuRow?> GetOrgUnitByIdAsync(
        Guid orgUnitId,
        CancellationToken cancellationToken) =>
        await dbContext.OrganizationUnits
            .AsNoTracking()
            .Where(ou => ou.Id == orgUnitId)
            .Select(ou => new OuRow(
                ou.Id,
                ou.ReferenceId,
                ou.Code,
                ou.Name,
                ou.InternationalName,
                ou.Depth,
                ou.HierarchyPath,
                ou.IsActive,
                ou.ManagerPositionId))
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

    public async Task<OuRow> GetOrgUnitByRefIdAsync(
        string orgUnitRefId,
        CancellationToken cancellationToken)
    {
        var normalized = orgUnitRefId.Trim();
        return await dbContext.OrganizationUnits
            .AsNoTracking()
            .Where(ou => ou.ReferenceId == normalized)
            .Select(ou => new OuRow(
                ou.Id,
                ou.ReferenceId,
                ou.Code,
                ou.Name,
                ou.InternationalName,
                ou.Depth,
                ou.HierarchyPath,
                ou.IsActive,
                ou.ManagerPositionId))
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"OrganizationUnit '{orgUnitRefId}' was not found.");
    }

    public async Task<List<OuRow>> LoadAncestorOusAsync(
        string hierarchyPath,
        CancellationToken cancellationToken)
    {
        var ancestorRefIds = hierarchyPath
            .Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (ancestorRefIds.Length == 0)
        {
            return [];
        }

        return await dbContext.OrganizationUnits
            .AsNoTracking()
            .Where(ou => ancestorRefIds.Contains(ou.ReferenceId))
            .OrderByDescending(ou => ou.Depth)
            .Select(ou => new OuRow(
                ou.Id,
                ou.ReferenceId,
                ou.Code,
                ou.Name,
                ou.InternationalName,
                ou.Depth,
                ou.HierarchyPath,
                ou.IsActive,
                ou.ManagerPositionId))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<List<PositionDelegation>> LoadActiveLegalEntityDelegationsAsync(
        Guid legalEntityId,
        DateTimeOffset targetUtc,
        CancellationToken cancellationToken) =>
        await dbContext.PositionDelegations
            .AsNoTracking()
            .Where(d => d.LegalEntityId == legalEntityId &&
                        !d.IsRevoked &&
                        d.ValidFrom <= targetUtc &&
                        d.ValidTo >= targetUtc)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    public async Task<List<PositionDelegation>> LoadActiveDelegationsForDelegatorAsync(
        Guid delegatorPositionId,
        DateTimeOffset targetUtc,
        CancellationToken cancellationToken) =>
        await dbContext.PositionDelegations
            .AsNoTracking()
            .Where(d => d.DelegatorPositionId == delegatorPositionId &&
                        !d.IsRevoked &&
                        d.ValidFrom <= targetUtc &&
                        d.ValidTo >= targetUtc)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    public async Task<List<PositionDelegation>> LoadActiveDelegationsForDelegateAsync(
        Guid delegatePositionId,
        DateTimeOffset targetUtc,
        CancellationToken cancellationToken) =>
        await dbContext.PositionDelegations
            .AsNoTracking()
            .Where(d => d.DelegatePositionId == delegatePositionId &&
                        !d.IsRevoked &&
                        d.ValidFrom <= targetUtc &&
                        d.ValidTo >= targetUtc)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    public async Task<bool> HasActiveDelegationForAnyAsync(
        Guid legalEntityId,
        Guid delegatePositionId,
        IReadOnlyCollection<Guid> delegatorIds,
        DateTimeOffset targetUtc,
        CancellationToken cancellationToken)
    {
        var delegations = await dbContext.PositionDelegations
            .AsNoTracking()
            .Where(d => d.LegalEntityId == legalEntityId &&
                        d.DelegatePositionId == delegatePositionId &&
                        !d.IsRevoked &&
                        d.ValidFrom <= targetUtc &&
                        d.ValidTo >= targetUtc)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return delegations.Any(d =>
            delegatorIds.Contains(d.DelegatorPositionId) &&
            d.IsEffectiveAt(targetUtc, documentTypeCode: null));
    }

    private static IQueryable<PositionRow> ProjectPositionRows(IQueryable<Position> query) =>
        query.Select(p => new PositionRow(
            p.Id,
            p.LegalEntityId,
            p.OrgUnitId,
            p.ReportsToPositionId,
            p.DottedLinePositionId,
            p.ReferenceId,
            p.Code,
            p.Title,
            p.InternationalTitle,
            p.LegalEntity!.ReferenceId,
            p.OrgUnit!.ReferenceId,
            p.OrgUnit.Code,
            p.OrgUnit.Name,
            p.Job!.Code,
            p.JobGrade!.Code,
            p.JobGrade.RankOrder,
            p.JobGrade.JobLevelCategory,
            p.ApprovalLimitAmount,
            p.ApprovalLimitCurrency,
            p.IsPrimaryLeadership,
            p.IsActive,
            p.ValidFrom,
            p.ValidTo));

    private static PositionProjection ToProjection(PositionRow row) =>
        new(
            Id: row.Id,
            LegalEntityId: row.LegalEntityId,
            OrgUnitId: row.OrgUnitId,
            ReportsToPositionId: row.ReportsToPositionId,
            DottedLinePositionId: row.DottedLinePositionId,
            Dto: new ChainOfCommandPositionDto(
                RefId: row.ReferenceId,
                Code: row.Code,
                Title: row.Title,
                InternationalTitle: row.InternationalTitle,
                LegalEntityRefId: row.LegalEntityRefId,
                OrgUnitRefId: row.OrgUnitRefId,
                OrgUnitCode: row.OrgUnitCode,
                OrgUnitName: row.OrgUnitName,
                JobCode: row.JobCode,
                JobGradeCode: row.JobGradeCode,
                JobGradeRankOrder: row.JobGradeRankOrder,
                JobLevelCategory: row.JobLevelCategory?.Key,
                ApprovalLimitAmount: row.ApprovalLimitAmount,
                ApprovalLimitCurrency: row.ApprovalLimitCurrency,
                IsPrimaryLeadership: row.IsPrimaryLeadership,
                IsActive: row.IsActive,
                ValidFrom: row.ValidFrom,
                ValidTo: row.ValidTo));
}
