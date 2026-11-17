using FluentValidation;
using Villsource.FSH.Modules.Organization.Contracts.v1.Positions.Commands;
using Villsource.Tool.UniqueKey;

namespace Villsource.FSH.Modules.Organization.Features.v1.Positions.Commands;

public sealed class UpdatePositionCommandValidator : AbstractValidator<UpdatePositionCommand>
{
    public UpdatePositionCommandValidator()
    {
        RuleFor(x => x.RefId).NotEmpty().MaximumLength(VillsourceId.KeySizes);
        RuleFor(x => x.OrgUnitId).NotEmpty();
        RuleFor(x => x.JobRefId).NotEmpty().MaximumLength(VillsourceId.KeySizes);
        RuleFor(x => x.JobGradeRefId).NotEmpty().MaximumLength(VillsourceId.KeySizes);

        RuleFor(x => x.Code)
            .NotEmpty()
            .MaximumLength(Domain.Positions.PositionRules.MaxCodeLength)
            .Must(Domain.Positions.PositionRules.IsValidCode)
            .WithMessage($"Code must be 2-{Domain.Positions.PositionRules.MaxCodeLength} uppercase alphanumeric characters, hyphens, or underscores.");

        RuleFor(x => x.Title).NotEmpty().MaximumLength(Domain.Positions.PositionRules.MaxTitleLength);
        RuleFor(x => x.InternationalTitle).NotEmpty().MaximumLength(Domain.Positions.PositionRules.MaxTitleLength);
        RuleFor(x => x.HeadcountCapacity).GreaterThanOrEqualTo(Domain.Positions.PositionRules.MinHeadcountCapacity);

        RuleFor(x => x.CostCenterRefId).MaximumLength(VillsourceId.KeySizes)
            .When(x => !string.IsNullOrWhiteSpace(x.CostCenterRefId));
        RuleFor(x => x.ReportsToPositionRefId).MaximumLength(VillsourceId.KeySizes)
            .When(x => !string.IsNullOrWhiteSpace(x.ReportsToPositionRefId));
        RuleFor(x => x.DottedLinePositionRefId).MaximumLength(VillsourceId.KeySizes)
            .When(x => !string.IsNullOrWhiteSpace(x.DottedLinePositionRefId));

        RuleFor(x => x.ReportsToPositionRefId)
            .NotEqual(x => x.RefId)
            .WithMessage("A position cannot report to itself.")
            .When(x => !string.IsNullOrWhiteSpace(x.ReportsToPositionRefId));

        RuleFor(x => x.DottedLinePositionRefId)
            .NotEqual(x => x.RefId)
            .WithMessage("A position cannot have a dotted-line report to itself.")
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