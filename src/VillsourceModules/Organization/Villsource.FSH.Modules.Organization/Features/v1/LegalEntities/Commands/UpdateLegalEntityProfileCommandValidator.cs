using FluentValidation;
using Villsource.FSH.Modules.Organization.Contracts.v1.LegalEntities.Commands;
using Villsource.Tool.UniqueKey;

namespace Villsource.FSH.Modules.Organization.Features.v1.LegalEntities;

public sealed class UpdateLegalEntityProfileCommandValidator : AbstractValidator<UpdateLegalEntityProfileCommand>
{
    public UpdateLegalEntityProfileCommandValidator()
    {
        RuleFor(x => x.RefId).NotEmpty().MaximumLength(VillsourceId.KeySizes);
        RuleFor(x => x.TradeName).MaximumLength(250);
        RuleFor(x => x.LogoUrl).MaximumLength(500);
        RuleFor(x => x.Website).MaximumLength(250);
        RuleFor(x => x.ContactEmail).EmailAddress().MaximumLength(256)
            .When(x => !string.IsNullOrWhiteSpace(x.ContactEmail));
        RuleFor(x => x.ContactPhone).MaximumLength(50);
    }
}