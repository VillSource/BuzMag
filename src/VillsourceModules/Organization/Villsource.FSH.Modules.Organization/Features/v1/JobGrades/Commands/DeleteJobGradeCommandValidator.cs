using FluentValidation;
using Villsource.FSH.Modules.Organization.Contracts.v1.JobGrades.Commands;
using Villsource.Tool.UniqueKey;

namespace Villsource.FSH.Modules.Organization.Features.v1.JobGrades.Commands
{
    public sealed class DeleteJobGradeCommandValidator : AbstractValidator<DeleteJobGradeCommand>
    {
        public DeleteJobGradeCommandValidator() =>
            RuleFor(x => x.RefId).NotEmpty().MaximumLength(VillsourceId.KeySizes);
    }

}