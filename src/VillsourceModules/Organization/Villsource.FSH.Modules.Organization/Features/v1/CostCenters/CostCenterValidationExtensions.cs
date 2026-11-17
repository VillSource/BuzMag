using FluentValidation;
using Villsource.FSH.Modules.Organization.Contracts.Constants;
using Villsource.FSH.Modules.Organization.Domain.CostCenters;
using Villsource.Tool.UniqueKey;

namespace Villsource.FSH.Modules.Organization.Features.v1.CostCenters;


internal static class CostCenterValidationExtensions
{
    extension<T>(IRuleBuilder<T, string> ruleBuilder)
    {
        public IRuleBuilderOptions<T, string> ValidCostCenterCode() =>
        ruleBuilder
            .NotEmpty()
            .MaximumLength(50)
            .Must(CostCenterRules.IsValidCode)
            .WithMessage("Code must be 2-50 alphanumeric characters, hyphens, or underscores.");

        public IRuleBuilderOptions<T, string> ValidFunctionTypeKey() =>
            ruleBuilder
                .NotEmpty()
                .Must(CostCenterFunctionType.IsValidKey)
                .WithMessage($"FunctionType must be one of: {CostCenterFunctionType.AllowedKeysDescription}.");

        public IRuleBuilderOptions<T, string> ValidRefId() =>
            ruleBuilder
                .NotEmpty()
                .MaximumLength(VillsourceId.KeySizes);
    }
}

