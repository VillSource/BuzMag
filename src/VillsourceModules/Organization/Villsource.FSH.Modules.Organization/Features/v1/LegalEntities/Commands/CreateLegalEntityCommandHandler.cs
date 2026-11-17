using Mediator;
using Villsource.FSH.Modules.Organization.Contracts.v1.LegalEntities.Commands;
using Villsource.FSH.Modules.Organization.Data;
using Villsource.FSH.Modules.Organization.Domain.LegalEntities;

namespace Villsource.FSH.Modules.Organization.Features.v1.LegalEntities;

public sealed class CreateLegalEntityCommandHandler(
    OrganizationDbContext dbContext)
    : IRequestHandler<CreateLegalEntityCommand, string>
{
    public async ValueTask<string> Handle(
        CreateLegalEntityCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        CompanyProfile? profile = request.Profile is null
            ? null
            : new CompanyProfile
            {
                TradeName = request.Profile.TradeName,
                LogoUrl = request.Profile.LogoUrl,
                Website = request.Profile.Website,
                ContactEmail = request.Profile.ContactEmail,
                ContactPhone = request.Profile.ContactPhone
            };

        var legalEntity = LegalEntity.Create(
            code: request.Code,
            countryCode: request.CountryCode,
            legalName: request.LegalName,
            internationalName: request.InternationalName,
            taxId: request.TaxId,
            baseCurrency: request.BaseCurrency,
            registrationNumber: request.RegistrationNumber,
            profile: profile,
            headOfficeAddress: request.HeadOfficeAddress,
            headOfficeBranchCode: request.HeadOfficeBranchCode,
            headOfficeName: request.HeadOfficeName,
            headOfficeInternationalName: request.HeadOfficeInternationalName,
            headOfficeRegisteredDate: request.HeadOfficeRegisteredDate);

        await dbContext.LegalEntities.AddAsync(
            entity: legalEntity,
            cancellationToken: cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken: cancellationToken);

        return legalEntity.ReferenceId;
    }
}