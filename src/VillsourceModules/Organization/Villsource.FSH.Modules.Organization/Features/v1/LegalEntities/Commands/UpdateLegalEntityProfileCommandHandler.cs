using FSH.Framework.Core.Exceptions;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Villsource.FSH.Modules.Organization.Contracts.v1.LegalEntities.Commands;
using Villsource.FSH.Modules.Organization.Data;
using Villsource.FSH.Modules.Organization.Domain.LegalEntities;

namespace Villsource.FSH.Modules.Organization.Features.v1.LegalEntities;

public sealed class UpdateLegalEntityProfileCommandHandler(
    OrganizationDbContext dbContext)
    : IRequestHandler<UpdateLegalEntityProfileCommand, string>
{
    public async ValueTask<string> Handle(
        UpdateLegalEntityProfileCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var legalEntity = await dbContext.LegalEntities
                              .FirstOrDefaultAsync(
                                  predicate: e => e.ReferenceId == request.RefId,
                                  cancellationToken: cancellationToken)
                          ?? throw new NotFoundException($"Legal entity '{request.RefId}' was not found.");

        var profile = new CompanyProfile
        {
            TradeName = request.TradeName,
            LogoUrl = request.LogoUrl,
            Website = request.Website,
            ContactEmail = request.ContactEmail,
            ContactPhone = request.ContactPhone
        };

        legalEntity.UpdateProfile(profile: profile);

        await dbContext.SaveChangesAsync(cancellationToken: cancellationToken);

        return legalEntity.ReferenceId;
    }
}