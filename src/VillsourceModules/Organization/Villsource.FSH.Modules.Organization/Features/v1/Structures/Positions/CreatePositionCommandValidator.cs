using FluentValidation;
using StackExchange.Redis;
using Villsource.FSH.Modules.Organization.Contracts.Constants;
using Villsource.FSH.Modules.Organization.Contracts.v1.Structures;

namespace Villsource.FSH.Modules.Organization.Features.v1.Structures.Positions;

public sealed class CreatePositionCommandValidator : AbstractValidator<CreatePositionCommand>
{
    public CreatePositionCommandValidator()
    {
        RuleFor(x => x.Code).NotEmpty().WithMessage("Code cannot be empty.");
        RuleFor(x => x.Code).MaximumLength(10).WithMessage("Code cannot exceed 10 characters.");

        RuleFor(x => x.Name).NotEmpty().WithMessage("Name cannot be empty.");
        RuleFor(x => x.Name).MaximumLength(50).WithMessage("Name cannot exceed 50 characters.");

        RuleFor(x => x.Description).MaximumLength(150).WithMessage("Description cannot exceed 150 characters.");

        RuleFor(x => x.PositionTier)
            .ForEach(i =>
                i.Must(rule => PositionTier.TryGet(rule,out _))
                    .WithMessage("PositionTier is not a valid tier"));
    }
}
