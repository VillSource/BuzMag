using FluentValidation;
using Villsource.FSH.Modules.Organization.Contracts.v1.JobFamilies.Commands;

namespace Villsource.FSH.Modules.Organization.Features.v1.JobFamilies.Commands;

public sealed class ActivateJobFamilyCommandValidator : AbstractValidator<ActivateJobFamilyCommand>
{
    public ActivateJobFamilyCommandValidator()
    {
        RuleFor(x => x.RefId).NotEmpty();
    }
}
