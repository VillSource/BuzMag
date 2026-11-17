using FluentValidation;
using Villsource.FSH.Modules.Organization.Contracts.v1.Positions.Commands;
using Villsource.Tool.UniqueKey;

namespace Villsource.FSH.Modules.Organization.Features.v1.Positions.Commands;

public sealed class CreatePositionCommandValidator : AbstractValidator<CreatePositionCommand>
{
    public CreatePositionCommandValidator()
    {
        RuleFor(x => x.LegalEntityRefId).NotEmpty().MaximumLength(VillsourceId.KeySizes);
        RuleFor(x => x.OrgUnitRefId).NotEmpty().MaximumLength(VillsourceId.KeySizes);
        RuleFor(x => x.JobRefId).NotEmpty().MaximumLength(VillsourceId.KeySizes);

        RuleFor(x => x.Code)
            .NotEmpty()
            .MaximumLength(Domain.Positions.PositionRules.MaxCodeLength)
            .Must(Domain.Positions.PositionRules.IsValidCode)
            .WithMessage($"Code must be 2-{Domain.Positions.PositionRules.MaxCodeLength} uppercase alphanumeric characters, hyphens, or underscores.");

        RuleFor(x => x.Title).NotEmpty().MaximumLength(Domain.Positions.PositionRules.MaxTitleLength);
        RuleFor(x => x.InternationalTitle).NotEmpty().MaximumLength(Domain.Positions.PositionRules.MaxTitleLength);
        RuleFor(x => x.Description).MaximumLength(Domain.Positions.PositionRules.MaxDescriptionLength)
            .When(x => !string.IsNullOrWhiteSpace(x.Description));
        RuleFor(x => x.HeadcountCapacity).GreaterThanOrEqualTo(Domain.Positions.PositionRules.MinHeadcountCapacity);

        RuleFor(x => x.JobGradeRefId).MaximumLength(VillsourceId.KeySizes)
            .When(x => !string.IsNullOrWhiteSpace(x.JobGradeRefId));
        RuleFor(x => x.CostCenterRefId).MaximumLength(VillsourceId.KeySizes)
            .When(x => !string.IsNullOrWhiteSpace(x.CostCenterRefId));
        RuleFor(x => x.WorkLocationTaxBranchRefId).MaximumLength(VillsourceId.KeySizes)
            .When(x => !string.IsNullOrWhiteSpace(x.WorkLocationTaxBranchRefId));
        RuleFor(x => x.ApprovalLimitAmount).GreaterThanOrEqualTo(0)
            .When(x => x.ApprovalLimitAmount.HasValue);
        RuleFor(x => x.ApprovalLimitCurrency).Length(Domain.Positions.PositionRules.MaxCurrencyLength)
            .When(x => !string.IsNullOrWhiteSpace(x.ApprovalLimitCurrency));
        RuleFor(x => x.ReportsToPositionRefId).MaximumLength(VillsourceId.KeySizes)
            .When(x => !string.IsNullOrWhiteSpace(x.ReportsToPositionRefId));
        RuleFor(x => x.DottedLinePositionRefId).MaximumLength(VillsourceId.KeySizes)
            .When(x => !string.IsNullOrWhiteSpace(x.DottedLinePositionRefId));

        RuleFor(x => x.DottedLinePositionRefId)
            .NotEqual(x => x.ReportsToPositionRefId)
            .WithMessage("Dotted-line reporting position cannot be the same as the primary reporting position.")
            .When(x => !string.IsNullOrWhiteSpace(x.ReportsToPositionRefId) &&
                       !string.IsNullOrWhiteSpace(x.DottedLinePositionRefId));

        RuleFor(x => x.ValidTo)
            .GreaterThanOrEqualTo(x => x.ValidFrom!.Value)
            .When(x => x.ValidFrom.HasValue && x.ValidTo.HasValue);
    }
}