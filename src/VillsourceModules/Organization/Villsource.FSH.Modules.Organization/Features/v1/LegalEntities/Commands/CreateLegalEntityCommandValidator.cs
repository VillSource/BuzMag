using FluentValidation;
using Villsource.FSH.Modules.Organization.Contracts.v1.LegalEntities.Commands;

namespace Villsource.FSH.Modules.Organization.Features.v1.LegalEntities;

public sealed class CreateLegalEntityCommandValidator : AbstractValidator<CreateLegalEntityCommand>
{
    public CreateLegalEntityCommandValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.CountryCode).NotEmpty().Length(2);
        RuleFor(x => x.LegalName).NotEmpty().MaximumLength(250);
        RuleFor(x => x.InternationalName).NotEmpty().MaximumLength(250);
        RuleFor(x => x.TaxId).NotEmpty().MaximumLength(30);
        RuleFor(x => x.BaseCurrency).NotEmpty().Length(3);
        RuleFor(x => x.RegistrationNumber).MaximumLength(50);

        When(x => x.Profile is not null, () =>
        {
            RuleFor(x => x.Profile!.TradeName).MaximumLength(250);
            RuleFor(x => x.Profile!.LogoUrl).MaximumLength(500);
            RuleFor(x => x.Profile!.Website).MaximumLength(250);
            RuleFor(x => x.Profile!.ContactEmail).EmailAddress().MaximumLength(256)
                .When(x => !string.IsNullOrWhiteSpace(x.Profile!.ContactEmail));
            RuleFor(x => x.Profile!.ContactPhone).MaximumLength(50);
        });

        When(x => x.HeadOfficeAddress is not null, () =>
        {
            RuleFor(x => x.HeadOfficeBranchCode).MaximumLength(20);
            RuleFor(x => x.HeadOfficeName).MaximumLength(250);
            RuleFor(x => x.HeadOfficeInternationalName).MaximumLength(250);
        });
    }
}