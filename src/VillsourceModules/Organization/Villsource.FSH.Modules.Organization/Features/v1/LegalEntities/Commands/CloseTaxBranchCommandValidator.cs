using FluentValidation;
using Villsource.FSH.Modules.Organization.Contracts.v1.LegalEntities.Commands;
using Villsource.Tool.UniqueKey;

namespace Villsource.FSH.Modules.Organization.Features.v1.LegalEntities;

public sealed class CloseTaxBranchCommandValidator : AbstractValidator<CloseTaxBranchCommand>
{
    public CloseTaxBranchCommandValidator()
    {
        RuleFor(x => x.LegalEntityRefId).NotEmpty().MaximumLength(VillsourceId.KeySizes);
        RuleFor(x => x.RefId).NotEmpty().MaximumLength(VillsourceId.KeySizes);
    }
}