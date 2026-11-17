using FluentValidation;
using Villsource.FSH.Modules.Organization.Contracts.v1.Jobs.Queries;
using Villsource.FSH.Modules.Organization.Domain.Jobs;

namespace Villsource.FSH.Modules.Organization.Features.v1.Jobs;

public sealed class GetJobLookupsQueryValidator : AbstractValidator<GetJobLookupsQuery>
{
    public GetJobLookupsQueryValidator() =>
        RuleFor(x => x.JobFamily).MaximumLength(JobRules.MaxJobFamilyLength);
}