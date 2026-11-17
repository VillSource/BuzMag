using FluentValidation;
using Villsource.FSH.Modules.Organization.Contracts.v1.Positions.Commands;
using Villsource.Tool.UniqueKey;

namespace Villsource.FSH.Modules.Organization.Features.v1.Positions.Commands;

public sealed class DeletePositionCommandValidator : AbstractValidator<DeletePositionCommand>
{
    public DeletePositionCommandValidator() =>
        RuleFor(x => x.RefId).NotEmpty().MaximumLength(VillsourceId.KeySizes);
}