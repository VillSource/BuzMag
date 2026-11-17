using FluentValidation;
using FSH.Framework.Core.Exceptions;
using Microsoft.EntityFrameworkCore;
using Villsource.FSH.Modules.Organization.Data;
using Villsource.FSH.Modules.Organization.Domain.LegalEntities;

namespace Villsource.FSH.Modules.Organization.Features.v1.CostCenters;


internal static class CostCenterReferenceResolver
{
    public static async Task<Guid?> ResolveParentCostCenterIdAsync(
        OrganizationDbContext dbContext,
        Guid legalEntityId,
        string? parentCostCenterRefId,
        bool requireActiveParent,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(parentCostCenterRefId))
        {
            return null;
        }

        var parent = await dbContext.CostCenters
            .AsNoTracking()
            .Where(c => c.ReferenceId == parentCostCenterRefId)
            .Select(c => new { c.Id, c.LegalEntityId, c.IsActive })
            .FirstOrDefaultAsync(cancellationToken: cancellationToken)
            ?? throw new NotFoundException(
                $"Parent cost center '{parentCostCenterRefId}' was not found.");

        if (parent.LegalEntityId != legalEntityId)
        {
            throw new InvalidOperationException(
                "Parent cost center must belong to the same legal entity.");
        }

        if (requireActiveParent && !parent.IsActive)
        {
            throw new InvalidOperationException(
                "Cannot assign an inactive parent cost center to an active cost center.");
        }

        return parent.Id;
    }

    public static async Task<Guid?> ResolveDefaultTaxBranchIdAsync(
        OrganizationDbContext dbContext,
        Guid legalEntityId,
        string? defaultTaxBranchRefId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(defaultTaxBranchRefId))
        {
            return null;
        }

        var branch = await dbContext.Set<TaxBranch>()
            .AsNoTracking()
            .Where(b => b.LegalEntityId == legalEntityId && b.ReferenceId == defaultTaxBranchRefId)
            .Select(b => new { b.Id, b.IsActive })
            .FirstOrDefaultAsync(cancellationToken: cancellationToken)
            ?? throw new NotFoundException(
                $"Tax branch '{defaultTaxBranchRefId}' was not found in this legal entity.");

        if (!branch.IsActive)
        {
            throw new InvalidOperationException(
                $"Tax branch '{defaultTaxBranchRefId}' is closed and cannot be set as default.");
        }

        return branch.Id;
    }

    public static async Task<Guid?> ResolveOrgUnitIdAsync(
        OrganizationDbContext dbContext,
        Guid legalEntityId,
        string? orgUnitRefId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(orgUnitRefId))
        {
            return null;
        }

        string trimmedRefId = orgUnitRefId.Trim();

        var orgUnit = await dbContext.OrganizationUnits
            .AsNoTracking()
            .Where(ou => ou.ReferenceId == trimmedRefId)
            .Select(ou => new { ou.Id, ou.LegalEntityId, ou.IsActive })
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException(
                $"Organization unit '{trimmedRefId}' was not found.");

        if (orgUnit.LegalEntityId.HasValue && orgUnit.LegalEntityId.Value != legalEntityId)
        {
            throw new InvalidOperationException(
                $"Organization unit '{trimmedRefId}' does not belong to this legal entity.");
        }

        if (!orgUnit.IsActive)
        {
            throw new InvalidOperationException(
                $"Organization unit '{trimmedRefId}' is inactive and cannot be assigned.");
        }

        return orgUnit.Id;
    }

    public static async Task EnsureNoHierarchyCycleAsync(
        OrganizationDbContext dbContext,
        Guid legalEntityId,
        Guid costCenterId,
        Guid proposedParentId,
        CancellationToken cancellationToken)
    {
        if (costCenterId == proposedParentId)
        {
            throw new InvalidOperationException("A cost center cannot be its own parent.");
        }

        // Single-query lookup for all nodes in the same LegalEntity (avoids N+1 DB round-trips)
        var parentMap = await dbContext.CostCenters
            .AsNoTracking()
            .Where(c => c.LegalEntityId == legalEntityId && c.ParentCostCenterId != null)
            .Select(c => new { c.Id, ParentId = c.ParentCostCenterId!.Value })
            .ToDictionaryAsync(x => x.Id, x => x.ParentId, cancellationToken: cancellationToken);

        var visited = new HashSet<Guid> { costCenterId };
        Guid? cursor = proposedParentId;

        while (cursor.HasValue)
        {
            if (!visited.Add(cursor.Value))
            {
                throw new InvalidOperationException(
                    "Assigning this parent cost center would create a circular hierarchy.");
            }

            cursor = parentMap.TryGetValue(cursor.Value, out var nextParentId)
                ? nextParentId
                : null;
        }
    }
}

