using FSH.Framework.Core.Context;
using FSH.Framework.Core.Exceptions;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Villsource.FSH.Modules.Organization.Contracts.v1.LegalEntities.Commands;
using Villsource.FSH.Modules.Organization.Data;

namespace Villsource.FSH.Modules.Organization.Features.v1.LegalEntities;

public sealed class RemoveTaxBranchCommandHandler(
    OrganizationDbContext dbContext,
    ICurrentUser currentUser)
    : IRequestHandler<RemoveTaxBranchCommand, string>
{
    public async ValueTask<string> Handle(
        RemoveTaxBranchCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var legalEntity = await dbContext.LegalEntities
                              .Include(e => e.Branches)
                              .FirstOrDefaultAsync(
                                  predicate: e => e.ReferenceId == request.LegalEntityRefId,
                                  cancellationToken: cancellationToken)
                              .ConfigureAwait(false)
                          ?? throw new NotFoundException($"Legal entity '{request.LegalEntityRefId}' was not found.");

        var branch = legalEntity.Branches.FirstOrDefault(b => b.ReferenceId == request.RefId)
                     ?? throw new NotFoundException($"Tax branch '{request.RefId}' was not found in legal entity '{request.LegalEntityRefId}'.");

        var hasCostCenterRef = await dbContext.CostCenters
            .AnyAsync(cc => cc.DefaultTaxBranchId == branch.Id && !cc.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (hasCostCenterRef)
        {
            throw new InvalidOperationException(
                $"Cannot remove tax branch '{request.RefId}' while cost centers reference it.");
        }

        legalEntity.RemoveBranch(
            branchRef: request.RefId,
            deletedBy: currentUser.GetUserId().ToString());

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return request.RefId;
    }
}