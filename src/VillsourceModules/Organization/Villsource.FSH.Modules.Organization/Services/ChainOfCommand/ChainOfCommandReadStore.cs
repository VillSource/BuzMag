using FSH.Framework.Core.Exceptions;
using Microsoft.EntityFrameworkCore;
using Villsource.FSH.Modules.Organization.Contracts.Services.Dtos;
using Villsource.FSH.Modules.Organization.Data;
using Villsource.FSH.Modules.Organization.Domain.OrganizationUnits;
using Villsource.FSH.Modules.Organization.Domain.PositionDelegations;
using Position = Villsource.FSH.Modules.Organization.Domain.Positions.Position;

namespace Villsource.FSH.Modules.Organization.Services.ChainOfCommand;

internal sealed class ChainOfCommandReadStore(OrganizationDbContext dbContext)
{
    public async Task<PositionProjection> GetPositionByRefIdAsync(
        string positionRefId,
        CancellationToken cancellationToken)
    {
        var normalized = positionRefId.Trim();
        var rows = await LoadProjectionsAsync(
                dbContext.Positions
                    .AsNoTracking()
                    .Where(p => p.ReferenceId == normalized)
                    .Take(1),
                cancellationToken)
            .ConfigureAwait(false);

        return rows.FirstOrDefault()
            ?? throw new NotFoundException($"Position '{positionRefId}' was not found.");
    }

    public async Task<PositionProjection?> GetPositionByIdAsync(
        Guid positionId,
        CancellationToken cancellationToken)
    {
        var rows = await LoadProjectionsAsync(
                dbContext.Positions
                    .AsNoTracking()
                    .Where(p => p.Id == positionId)
                    .Take(1),
                cancellationToken)
            .ConfigureAwait(false);

        return rows.FirstOrDefault();
    }

    public async Task<Dictionary<Guid, PositionProjection>> GetPositionsByIdsAsync(
        IReadOnlyCollection<Guid> positionIds,
        CancellationToken cancellationToken)
    {
        if (positionIds.Count == 0)
        {
            return new Dictionary<Guid, PositionProjection>();
        }

        var rows = await LoadProjectionsAsync(
                dbContext.Positions
                    .AsNoTracking()
                    .Where(p => positionIds.Contains(p.Id)),
                cancellationToken)
            .ConfigureAwait(false);

        return rows.ToDictionary(p => p.Id);
    }

    public async Task<Dictionary<Guid, PositionProjection>> GetLegalEntityPositionsMapAsync(
        Guid legalEntityId,
        CancellationToken cancellationToken)
    {
        var rows = await LoadProjectionsAsync(
                dbContext.Positions
                    .AsNoTracking()
                    .Where(p => p.LegalEntityId == legalEntityId),
                cancellationToken)
            .ConfigureAwait(false);

        return rows.ToDictionary(p => p.Id);
    }

    public async Task<List<PositionProjection>> GetDirectReportsByManagerIdAsync(
        Guid managerPositionId,
        CancellationToken cancellationToken) =>
        await LoadProjectionsAsync(
                dbContext.Positions
                    .AsNoTracking()
                    .Where(p => p.ReportsToPositionId == managerPositionId),
                cancellationToken)
            .ConfigureAwait(false);

    public async Task<OuRow?> GetOrgUnitByIdAsync(
        Guid orgUnitId,
        CancellationToken cancellationToken)
    {
        var rows = await LoadOuRowsAsync(
                dbContext.OrganizationUnits
                    .AsNoTracking()
                    .Where(ou => ou.Id == orgUnitId)
                    .Take(1),
                cancellationToken)
            .ConfigureAwait(false);

        return rows.FirstOrDefault();
    }

    public async Task<OuRow> GetOrgUnitByRefIdAsync(
        string orgUnitRefId,
        CancellationToken cancellationToken)
    {
        var normalized = orgUnitRefId.Trim();
        var rows = await LoadOuRowsAsync(
                dbContext.OrganizationUnits
                    .AsNoTracking()
                    .Where(ou => ou.ReferenceId == normalized)
                    .Take(1),
                cancellationToken)
            .ConfigureAwait(false);

        return rows.FirstOrDefault()
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

        return await LoadOuRowsAsync(
                dbContext.OrganizationUnits
                    .AsNoTracking()
                    .Where(ou => ancestorRefIds.Contains(ou.ReferenceId))
                    .OrderByDescending(ou => ou.Depth),
                cancellationToken)
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

    private static async Task<List<PositionProjection>> LoadProjectionsAsync(
        IQueryable<Position> query,
        CancellationToken cancellationToken)
    {
        var rows = await query
            .Select(p => new
            {
                p.Id,
                p.LegalEntityId,
                p.OrgUnitId,
                p.ReportsToPositionId,
                p.DottedLinePositionId,
                p.ReferenceId,
                p.Code,
                p.Title,
                p.InternationalTitle,
                LegalEntityRefId = p.LegalEntity != null ? p.LegalEntity.ReferenceId : string.Empty,
                OrgUnitRefId = p.OrgUnit != null ? p.OrgUnit.ReferenceId : string.Empty,
                OrgUnitCode = p.OrgUnit != null ? p.OrgUnit.Code : string.Empty,
                OrgUnitName = p.OrgUnit != null ? p.OrgUnit.Name : string.Empty,
                JobCode = p.Job != null ? p.Job.Code : string.Empty,
                JobGradeCode = p.JobGrade != null ? p.JobGrade.Code : string.Empty,
                JobGradeRankOrder = p.JobGrade != null ? (int?)p.JobGrade.RankOrder : null,
                JobLevelCategory = p.JobGrade != null ? p.JobGrade.JobLevelCategory : null,
                p.ApprovalLimitAmount,
                p.ApprovalLimitCurrency,
                p.IsPrimaryLeadership,
                p.IsActive,
                p.ValidFrom,
                p.ValidTo
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return rows
            .Select(row => new PositionProjection(
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
                    JobGradeRankOrder: row.JobGradeRankOrder ?? 0,
                    JobLevelCategory: row.JobLevelCategory?.Key,
                    ApprovalLimitAmount: row.ApprovalLimitAmount,
                    ApprovalLimitCurrency: row.ApprovalLimitCurrency,
                    IsPrimaryLeadership: row.IsPrimaryLeadership,
                    IsActive: row.IsActive,
                    ValidFrom: row.ValidFrom,
                    ValidTo: row.ValidTo)))
            .ToList();
    }

    private static async Task<List<OuRow>> LoadOuRowsAsync(
        IQueryable<OrganizationUnit> query,
        CancellationToken cancellationToken)
    {
        var rows = await query
            .Select(ou => new
            {
                ou.Id,
                ou.ReferenceId,
                ou.Code,
                ou.Name,
                ou.InternationalName,
                ou.Depth,
                ou.HierarchyPath,
                ou.IsActive,
                ou.ManagerPositionId
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return rows
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
            .ToList();
    }
}
