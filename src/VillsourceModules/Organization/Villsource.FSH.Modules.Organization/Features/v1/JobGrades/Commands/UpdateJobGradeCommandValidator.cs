using FluentValidation;
using Villsource.FSH.Modules.Organization.Contracts.v1.JobGrades.Commands;
using Villsource.FSH.Modules.Organization.Domain.JobGrades;
using Villsource.Tool.UniqueKey;

namespace Villsource.FSH.Modules.Organization.Features.v1.JobGrades.Commands
{
    public sealed class UpdateJobGradeCommandValidator : AbstractValidator<UpdateJobGradeCommand>
    {
        public UpdateJobGradeCommandValidator()
        {
            RuleFor(x => x.RefId).NotEmpty().MaximumLength(VillsourceId.KeySizes);

            RuleFor(x => x.Code)
                .NotEmpty()
                .MaximumLength(JobGradeRules.MaxCodeLength)
                .Must(JobGradeRules.IsValidCode)
                .WithMessage($"Code must be 1-{JobGradeRules.MaxCodeLength} uppercase alphanumeric characters, hyphens, or underscores.");

            RuleFor(x => x.Name).NotEmpty().MaximumLength(JobGradeRules.MaxNameLength);
            RuleFor(x => x.InternationalName).NotEmpty().MaximumLength(JobGradeRules.MaxNameLength);
            RuleFor(x => x.RankOrder).GreaterThanOrEqualTo(JobGradeRules.MinRankOrder);
            RuleFor(x => x.JobLevelCategory)
                .Must(c => JobGradeRules.AllowedCategories.Contains(c!, StringComparer.OrdinalIgnoreCase))
                .When(x => !string.IsNullOrWhiteSpace(x.JobLevelCategory))
                .WithMessage($"JobLevelCategory must be one of: {string.Join(", ", JobGradeRules.AllowedCategories)}.");
            RuleFor(x => x.Description).MaximumLength(JobGradeRules.MaxDescriptionLength);
        }
    }

}