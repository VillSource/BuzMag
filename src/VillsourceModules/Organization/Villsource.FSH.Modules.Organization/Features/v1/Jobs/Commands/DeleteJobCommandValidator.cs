using FluentValidation;
using Villsource.FSH.Modules.Organization.Contracts.v1.Jobs.Commands;
using Villsource.Tool.UniqueKey;

namespace Villsource.FSH.Modules.Organization.Features.v1.Jobs;

public sealed class DeleteJobCommandValidator : AbstractValidator<DeleteJobCommand>
{
    public DeleteJobCommandValidator() =>
        RuleFor(x => x.RefId).NotEmpty().MaximumLength(VillsourceId.KeySizes);
}