using FSH.Framework.Core.Exceptions;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Villsource.FSH.Modules.Organization.Contracts.v1.LegalEntities.Commands;
using Villsource.FSH.Modules.Organization.Data;

namespace Villsource.FSH.Modules.Organization.Features.v1.LegalEntities;

public sealed class UpdateLegalEntityCommandHandler(
    OrganizationDbContext dbContext)
    : IRequestHandler<UpdateLegalEntityCommand, string>
{
    public async ValueTask<string> Handle(
        UpdateLegalEntityCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var legalEntity = await dbContext.LegalEntities
                              .FirstOrDefaultAsync(
                                  predicate: e => e.ReferenceId == request.RefId,
                                  cancellationToken: cancellationToken)
                              .ConfigureAwait(false)
                          ?? throw new NotFoundException($"Legal entity '{request.RefId}' was not found.");

        legalEntity.Update(
            code: request.Code,
            legalName: request.LegalName,
            internationalName: request.InternationalName,
            taxId: request.TaxId,
            baseCurrency: request.BaseCurrency,
            registrationNumber: request.RegistrationNumber,
            fiscalYearStartMonth: request.FiscalYearStartMonth,
            timeZoneId: request.TimeZoneId);

        await dbContext.SaveChangesAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

        return legalEntity.ReferenceId;
    }
}