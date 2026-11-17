using FluentValidation;
using Villsource.FSH.Modules.Organization.Contracts.v1.JobFamilies.Commands;

namespace Villsource.FSH.Modules.Organization.Features.v1.JobFamilies.Commands;

public sealed class DeactivateJobFamilyCommandValidator : AbstractValidator<DeactivateJobFamilyCommand>
{
    public DeactivateJobFamilyCommandValidator()
    {
        RuleFor(x => x.RefId).NotEmpty();
    }
}
