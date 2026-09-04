using FluentValidation;
using Villsource.FSH.Modules.Organization.Contracts.v1.Structures;
using Villsource.Tool.UniqueKey;

namespace Villsource.FSH.Modules.Organization.Features.v1.Structures.PositionAllocations;

public sealed class AllocatePositionCommandValidator : AbstractValidator<AllocatePositionCommand>
{
    public AllocatePositionCommandValidator()
    {
        RuleFor(x => x.OrganizationUnitId).NotEmpty();
        RuleFor(x => x.OrganizationUnitId).IsVillsourceId();
        RuleFor(x => x.PositionId).NotEmpty();
        RuleFor(x => x.PositionId).IsVillsourceId();
        RuleFor(x => x.HeadCount).GreaterThan(0).When(x => x.HeadCount.HasValue);
    }
}