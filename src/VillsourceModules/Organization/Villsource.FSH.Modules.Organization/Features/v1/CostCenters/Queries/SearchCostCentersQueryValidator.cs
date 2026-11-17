using FluentValidation;
using Villsource.FSH.Modules.Organization.Contracts.Constants;
using Villsource.FSH.Modules.Organization.Contracts.v1.CostCenters.Queries;
using Villsource.Tool.UniqueKey;

namespace Villsource.FSH.Modules.Organization.Features.v1.CostCenters.Queries;

public sealed class SearchCostCentersQueryValidator : AbstractValidator<SearchCostCentersQuery>
{
    public SearchCostCentersQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1)
            .When(x => x.PageNumber.HasValue);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .When(x => x.PageSize.HasValue);

        RuleFor(x => x.Keyword)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.Keyword));

        RuleFor(x => x.Sort)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.Sort));

        RuleFor(x => x.LegalEntityRefId)
            .MaximumLength(VillsourceId.KeySizes)
            .When(x => !string.IsNullOrWhiteSpace(x.LegalEntityRefId));

        RuleFor(x => x.ParentCostCenterRefId)
            .MaximumLength(VillsourceId.KeySizes)
            .When(x => !string.IsNullOrWhiteSpace(x.ParentCostCenterRefId));

        RuleFor(x => x.OrgUnitRefId)
            .MaximumLength(VillsourceId.KeySizes)
            .When(x => !string.IsNullOrWhiteSpace(x.OrgUnitRefId));

        RuleFor(x => x.FunctionType)
            .Must(CostCenterFunctionType.IsValidKey)
            .WithMessage($"FunctionType must be one of: {CostCenterFunctionType.AllowedKeysDescription}.")
            .When(x => !string.IsNullOrWhiteSpace(x.FunctionType));
    }
}

