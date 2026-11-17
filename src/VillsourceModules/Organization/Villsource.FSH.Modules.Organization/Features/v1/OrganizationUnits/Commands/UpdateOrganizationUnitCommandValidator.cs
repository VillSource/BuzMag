using FluentValidation;
using Villsource.FSH.Modules.Organization.Contracts.v1.OrganizationUnits.Commands;
using Villsource.FSH.Modules.Organization.Domain.OrganizationUnits;
using Villsource.Tool.UniqueKey;

namespace Villsource.FSH.Modules.Organization.Features.v1.OrganizationUnits;

public sealed class UpdateOrganizationUnitCommandValidator : AbstractValidator<UpdateOrganizationUnitCommand>
{
    public UpdateOrganizationUnitCommandValidator()
    {
        RuleFor(x => x.RefId).NotEmpty().MaximumLength(VillsourceId.KeySizes);

        RuleFor(x => x.Code)
            .NotEmpty()
            .MaximumLength(50)
            .Must(code => OrganizationUnitRules.CodeRegex().IsMatch(code.Trim().ToUpperInvariant()))
            .WithMessage("Code must be 2-50 uppercase alphanumeric characters, hyphens, or underscores.");

        RuleFor(x => x.Name).NotEmpty().MaximumLength(250);
        RuleFor(x => x.InternationalName).NotEmpty().MaximumLength(250);

        RuleFor(x => x.OrganizationUnitTypeRefId)
            .NotEmpty()
            .MaximumLength(VillsourceId.KeySizes);

        RuleFor(x => x.Description).MaximumLength(1000);
        RuleFor(x => x.DefaultCostCenterRefId).MaximumLength(VillsourceId.KeySizes).When(x => !string.IsNullOrWhiteSpace(x.DefaultCostCenterRefId));
    }
}