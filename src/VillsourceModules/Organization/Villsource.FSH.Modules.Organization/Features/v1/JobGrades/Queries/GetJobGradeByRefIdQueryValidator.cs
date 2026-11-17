using FluentValidation;
using Villsource.FSH.Modules.Organization.Contracts.v1.JobGrades.Queries;
using Villsource.Tool.UniqueKey;

namespace Villsource.FSH.Modules.Organization.Features.v1.JobGrades.Queries
{
    public sealed class GetJobGradeByRefIdQueryValidator : AbstractValidator<GetJobGradeByRefIdQuery>
    {
        public GetJobGradeByRefIdQueryValidator() =>
            RuleFor(x => x.RefId).NotEmpty().MaximumLength(VillsourceId.KeySizes);
    }

}