using FluentValidation;
using Villsource.FSH.Modules.Organization.Contracts.v1.Jobs.Commands;
using Villsource.FSH.Modules.Organization.Domain.Jobs;
using Villsource.Tool.UniqueKey;

namespace Villsource.FSH.Modules.Organization.Features.v1.Jobs;

public sealed class UpdateJobCommandValidator : AbstractValidator<UpdateJobCommand>
{
    public UpdateJobCommandValidator()
    {
        RuleFor(x => x.RefId).NotEmpty().MaximumLength(VillsourceId.KeySizes);

        RuleFor(x => x.Code)
            .NotEmpty()
            .MaximumLength(JobRules.MaxCodeLength)
            .Must(JobRules.IsValidCode)
            .WithMessage($"Code must be 2-{JobRules.MaxCodeLength} uppercase alphanumeric characters, hyphens, or underscores.");

        RuleFor(x => x.Name).NotEmpty().MaximumLength(JobRules.MaxNameLength);
        RuleFor(x => x.InternationalName).NotEmpty().MaximumLength(JobRules.MaxNameLength);
        RuleFor(x => x.DefaultJobGradeRefId).NotEmpty().MaximumLength(VillsourceId.KeySizes);
        RuleFor(x => x.JobFamily).MaximumLength(JobRules.MaxJobFamilyLength);
        RuleFor(x => x.Description).MaximumLength(JobRules.MaxDescriptionLength);
    }
}