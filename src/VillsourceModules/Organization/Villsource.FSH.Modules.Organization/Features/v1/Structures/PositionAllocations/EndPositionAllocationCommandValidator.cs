using FluentValidation;
using Villsource.FSH.Modules.Organization.Contracts.v1.Structures;

namespace Villsource.FSH.Modules.Organization.Features.v1.Structures.PositionAllocations;

public sealed class EndPositionAllocationCommandValidator : AbstractValidator<EndPositionAllocationCommand>
{
    public EndPositionAllocationCommandValidator() => RuleFor(x => x.AllocationId).NotEmpty();
}