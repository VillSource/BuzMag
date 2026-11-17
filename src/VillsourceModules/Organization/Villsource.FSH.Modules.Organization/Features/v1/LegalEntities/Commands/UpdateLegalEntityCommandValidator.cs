using FluentValidation;
using Villsource.FSH.Modules.Organization.Contracts.v1.LegalEntities.Commands;
using Villsource.Tool.UniqueKey;

namespace Villsource.FSH.Modules.Organization.Features.v1.LegalEntities;

public sealed class UpdateLegalEntityCommandValidator : AbstractValidator<UpdateLegalEntityCommand>
{
    public UpdateLegalEntityCommandValidator()
    {
        RuleFor(x => x.RefId).NotEmpty().MaximumLength(VillsourceId.KeySizes);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.LegalName).NotEmpty().MaximumLength(250);
        RuleFor(x => x.InternationalName).NotEmpty().MaximumLength(250);
        RuleFor(x => x.TaxId).NotEmpty().MaximumLength(30);
        RuleFor(x => x.BaseCurrency).NotEmpty().Length(3);
        RuleFor(x => x.RegistrationNumber).MaximumLength(50);
        RuleFor(x => x.FiscalYearStartMonth).InclusiveBetween(1, 12).When(x => x.FiscalYearStartMonth.HasValue);
        RuleFor(x => x.TimeZoneId).MaximumLength(60).When(x => !string.IsNullOrWhiteSpace(x.TimeZoneId));
    }
}