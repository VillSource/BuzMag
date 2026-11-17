using FluentValidation;
using Villsource.FSH.Modules.Organization.Contracts.v1.LegalEntities.Commands;
using Villsource.Tool.UniqueKey;

namespace Villsource.FSH.Modules.Organization.Features.v1.LegalEntities;

public sealed class AddTaxBranchCommandValidator : AbstractValidator<AddTaxBranchCommand>
{
    public AddTaxBranchCommandValidator()
    {
        RuleFor(x => x.LegalEntityRefId).NotEmpty().MaximumLength(VillsourceId.KeySizes);
        RuleFor(x => x.BranchCode).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(250);
        RuleFor(x => x.InternationalName).NotEmpty().MaximumLength(250);
        RuleFor(x => x.RegisteredAddress).NotNull();
        RuleFor(x => x.TaxSubId).MaximumLength(30);
    }
}