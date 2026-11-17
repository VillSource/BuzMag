using FluentValidation;
using Villsource.FSH.Modules.Organization.Contracts.v1.Jobs.Queries;
using Villsource.Tool.UniqueKey;

namespace Villsource.FSH.Modules.Organization.Features.v1.Jobs;

public sealed class GetJobByRefIdQueryValidator : AbstractValidator<GetJobByRefIdQuery>
{
    public GetJobByRefIdQueryValidator() =>
        RuleFor(x => x.RefId).NotEmpty().MaximumLength(VillsourceId.KeySizes);
}