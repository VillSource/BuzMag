using FluentValidation;
using Villsource.FSH.Modules.Organization.Contracts.v1.LegalEntities.Commands;
using Villsource.Tool.UniqueKey;

namespace Villsource.FSH.Modules.Organization.Features.v1.LegalEntities;

public sealed class DeleteLegalEntityCommandValidator : AbstractValidator<DeleteLegalEntityCommand>
{
    public DeleteLegalEntityCommandValidator()
    {
        RuleFor(x => x.RefId).NotEmpty().MaximumLength(VillsourceId.KeySizes);
    }
}