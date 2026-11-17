using FSH.Framework.Core.Exceptions;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Villsource.FSH.Modules.Organization.Contracts.v1.LegalEntities.Commands;
using Villsource.FSH.Modules.Organization.Data;

namespace Villsource.FSH.Modules.Organization.Features.v1.LegalEntities;

public sealed class CloseTaxBranchCommandHandler(
    OrganizationDbContext dbContext)
    : IRequestHandler<CloseTaxBranchCommand, string>
{
    public async ValueTask<string> Handle(
        CloseTaxBranchCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var legalEntity = await dbContext.LegalEntities
                              .Include(e => e.Branches)
                              .FirstOrDefaultAsync(
                                  predicate: e => e.ReferenceId == request.LegalEntityRefId,
                                  cancellationToken: cancellationToken)
                          ?? throw new NotFoundException($"Legal entity '{request.LegalEntityRefId}' was not found.");

        legalEntity.CloseBranch(
            branchRef: request.RefId,
            closedDate: request.ClosedDate);

        await dbContext.SaveChangesAsync(cancellationToken: cancellationToken);

        return request.RefId;
    }
}