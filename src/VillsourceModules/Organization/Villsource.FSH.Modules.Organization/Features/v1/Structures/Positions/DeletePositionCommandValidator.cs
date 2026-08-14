using FluentValidation;
using Villsource.FSH.Modules.Organization.Contracts.v1.Structures;

namespace Villsource.FSH.Modules.Organization.Features.v1.Structures.Positions;

public sealed class DeletePositionCommandValidator : AbstractValidator<DeletePositionCommand>
{
    public DeletePositionCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Position ID is required.");
    }
}
