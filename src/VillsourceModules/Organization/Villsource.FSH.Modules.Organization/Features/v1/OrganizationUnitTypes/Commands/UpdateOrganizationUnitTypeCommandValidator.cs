using FluentValidation;
using Villsource.FSH.Modules.Organization.Contracts.v1.OrganizationUnitTypes.Commands;
using Villsource.FSH.Modules.Organization.Domain.OrganizationUnitTypes;

namespace Villsource.FSH.Modules.Organization.Features.v1.OrganizationUnitTypes.Commands;

public sealed class UpdateOrganizationUnitTypeCommandValidator : AbstractValidator<UpdateOrganizationUnitTypeCommand>
{
    public UpdateOrganizationUnitTypeCommandValidator()
    {
        RuleFor(x => x.RefId).NotEmpty();

        RuleFor(x => x.Code)
            .NotEmpty()
            .MaximumLength(OrganizationUnitTypeRules.MaxCodeLength)
            .Must(OrganizationUnitTypeRules.IsValidCode)
            .WithMessage($"Code must be 1-{OrganizationUnitTypeRules.MaxCodeLength} uppercase alphanumeric characters, hyphens, or underscores.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(OrganizationUnitTypeRules.MaxNameLength);

        RuleFor(x => x.InternationalName)
            .NotEmpty()
            .MaximumLength(OrganizationUnitTypeRules.MaxNameLength);

        RuleFor(x => x.HierarchyLevel)
            .GreaterThanOrEqualTo(OrganizationUnitTypeRules.MinHierarchyLevel);

        RuleFor(x => x.Description)
            .MaximumLength(OrganizationUnitTypeRules.MaxDescriptionLength)
            .When(x => x.Description is not null);
    }
}
