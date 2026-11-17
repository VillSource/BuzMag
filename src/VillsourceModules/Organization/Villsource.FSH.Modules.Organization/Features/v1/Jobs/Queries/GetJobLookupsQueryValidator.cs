using FluentValidation;
using Villsource.FSH.Modules.Organization.Contracts.v1.Jobs.Queries;
using Villsource.Tool.UniqueKey;

namespace Villsource.FSH.Modules.Organization.Features.v1.Jobs;

public sealed class GetJobLookupsQueryValidator : AbstractValidator<GetJobLookupsQuery>
{
    public GetJobLookupsQueryValidator() =>
        RuleFor(x => x.JobFamilyRefId).MaximumLength(VillsourceId.KeySizes).When(x => !string.IsNullOrWhiteSpace(x.JobFamilyRefId));
}