using FluentValidation;
using Villsource.FSH.Modules.Organization.Contracts.v1.CostCenters.Commands;
using Villsource.Tool.UniqueKey;

namespace Villsource.FSH.Modules.Organization.Features.v1.CostCenters.Commands;

public sealed class CreateCostCenterCommandValidator : AbstractValidator<CreateCostCenterCommand>
{
    public CreateCostCenterCommandValidator()
    {
        RuleFor(x => x.LegalEntityRefId).ValidRefId();
        RuleFor(x => x.Code).ValidCostCenterCode();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(250);
        RuleFor(x => x.InternationalName).NotEmpty().MaximumLength(250);
        RuleFor(x => x.Description).MaximumLength(1000);
        RuleFor(x => x.FunctionType).ValidFunctionTypeKey();

        RuleFor(x => x.ParentCostCenterRefId)
            .MaximumLength(VillsourceId.KeySizes)
            .When(x => !string.IsNullOrWhiteSpace(x.ParentCostCenterRefId));

        RuleFor(x => x.DefaultTaxBranchRefId)
            .MaximumLength(VillsourceId.KeySizes)
            .When(x => !string.IsNullOrWhiteSpace(x.DefaultTaxBranchRefId));

        RuleFor(x => x.OrgUnitRefId)
            .MaximumLength(VillsourceId.KeySizes)
            .When(x => !string.IsNullOrWhiteSpace(x.OrgUnitRefId));

        RuleFor(x => x.ValidTo)
            .GreaterThanOrEqualTo(x => x.ValidFrom!.Value)
            .When(x => x.ValidFrom.HasValue && x.ValidTo.HasValue)
            .WithMessage("ValidTo must be greater than or equal to ValidFrom.");
    }
}
