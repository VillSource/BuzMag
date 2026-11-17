using FluentValidation;
using Villsource.FSH.Modules.Organization.Contracts.v1.JobFamilies.Commands;
using Villsource.FSH.Modules.Organization.Domain.JobFamilies;

namespace Villsource.FSH.Modules.Organization.Features.v1.JobFamilies.Commands;

public sealed class UpdateJobFamilyCommandValidator : AbstractValidator<UpdateJobFamilyCommand>
{
    public UpdateJobFamilyCommandValidator()
    {
        RuleFor(x => x.RefId).NotEmpty();

        RuleFor(x => x.Code)
            .NotEmpty()
            .MaximumLength(JobFamilyRules.MaxCodeLength)
            .Must(JobFamilyRules.IsValidCode)
            .WithMessage($"Code must be 2-{JobFamilyRules.MaxCodeLength} uppercase alphanumeric characters, hyphens, or underscores.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(JobFamilyRules.MaxNameLength);

        RuleFor(x => x.InternationalName)
            .NotEmpty()
            .MaximumLength(JobFamilyRules.MaxNameLength);

        RuleFor(x => x.Description)
            .MaximumLength(JobFamilyRules.MaxDescriptionLength)
            .When(x => x.Description is not null);
    }
}
