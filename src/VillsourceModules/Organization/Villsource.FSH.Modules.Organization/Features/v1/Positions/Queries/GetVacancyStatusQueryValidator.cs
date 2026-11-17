using FluentValidation;
using Villsource.FSH.Modules.Organization.Contracts.v1.Positions.Queries;
using Villsource.Tool.UniqueKey;

namespace Villsource.FSH.Modules.Organization.Features.v1.Positions.Queries;

public sealed class GetVacancyStatusQueryValidator : AbstractValidator<GetVacancyStatusQuery>
{
    public GetVacancyStatusQueryValidator()
    {
        RuleFor(x => x.LegalEntityRefId).NotEmpty().MaximumLength(VillsourceId.KeySizes);
        RuleFor(x => x.OrgUnitRefId).MaximumLength(VillsourceId.KeySizes).When(x => !string.IsNullOrWhiteSpace(x.OrgUnitRefId));
    }
}
