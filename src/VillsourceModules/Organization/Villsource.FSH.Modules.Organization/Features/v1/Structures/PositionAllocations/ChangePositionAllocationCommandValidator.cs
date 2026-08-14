using FluentValidation;
using Villsource.FSH.Modules.Organization.Contracts.v1.Structures.PositionAllocations;

namespace Villsource.FSH.Modules.Organization.Features.v1.Structures.PositionAllocations;

public sealed class ChangePositionAllocationCommandValidator : AbstractValidator<ChangePositionAllocationCommand>
{
    public ChangePositionAllocationCommandValidator()
    {
        RuleFor(x => x.AllocationId).NotEmpty();
        RuleFor(x => x.HeadCount).GreaterThan(0).When(x => x.HeadCount.HasValue);
    }
}