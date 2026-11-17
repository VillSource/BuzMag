using FluentValidation;
using Villsource.FSH.Modules.Organization.Contracts.Constants;
using Villsource.FSH.Modules.Organization.Contracts.v1.JobGrades.Commands;
using Villsource.FSH.Modules.Organization.Domain.JobGrades;

namespace Villsource.FSH.Modules.Organization.Features.v1.JobGrades.Commands
{
    public sealed class CreateJobGradeCommandValidator : AbstractValidator<CreateJobGradeCommand>
    {
        public CreateJobGradeCommandValidator()
        {
            RuleFor(x => x.Code)
                .NotEmpty()
                .MaximumLength(JobGradeRules.MaxCodeLength)
                .Must(JobGradeRules.IsValidCode)
                .WithMessage($"Code must be 1-{JobGradeRules.MaxCodeLength} uppercase alphanumeric characters, hyphens, or underscores.");

            RuleFor(x => x.Name).NotEmpty().MaximumLength(JobGradeRules.MaxNameLength);
            RuleFor(x => x.InternationalName).NotEmpty().MaximumLength(JobGradeRules.MaxNameLength);
            RuleFor(x => x.RankOrder).GreaterThanOrEqualTo(JobGradeRules.MinRankOrder);
            RuleFor(x => x.JobLevelCategory)
                .Must(JobLevelCategory.IsValidKey)
                .When(x => !string.IsNullOrWhiteSpace(x.JobLevelCategory))
                .WithMessage($"JobLevelCategory must be one of: {JobLevelCategory.AllowedKeysDescription}.");
            RuleFor(x => x.Description).MaximumLength(JobGradeRules.MaxDescriptionLength);
        }
    }

}