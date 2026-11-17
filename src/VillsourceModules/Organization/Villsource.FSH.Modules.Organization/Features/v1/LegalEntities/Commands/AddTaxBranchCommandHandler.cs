using FSH.Framework.Core.Exceptions;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Villsource.FSH.Modules.Organization.Contracts.v1.LegalEntities.Commands;
using Villsource.FSH.Modules.Organization.Data;

namespace Villsource.FSH.Modules.Organization.Features.v1.LegalEntities;

public sealed class AddTaxBranchCommandHandler(
    OrganizationDbContext dbContext)
    : IRequestHandler<AddTaxBranchCommand, string>
{
    public async ValueTask<string> Handle(
        AddTaxBranchCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var legalEntity = await dbContext.LegalEntities
                              .Include(e => e.Branches)
                              .FirstOrDefaultAsync(
                                  predicate: e => e.ReferenceId == request.LegalEntityRefId,
                                  cancellationToken: cancellationToken)
                          ?? throw new NotFoundException($"Legal entity '{request.LegalEntityRefId}' was not found.");

        var branch = legalEntity.AddBranch(
            branchCode: request.BranchCode,
            name: request.Name,
            internationalName: request.InternationalName,
            registeredAddress: request.RegisteredAddress,
            isHeadOffice: request.IsHeadOffice,
            taxSubId: request.TaxSubId,
            registeredDate: request.RegisteredDate,
            isActive: request.IsActive);

        await dbContext.SaveChangesAsync(cancellationToken: cancellationToken);

        return branch.ReferenceId;
    }
}