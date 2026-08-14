using FluentValidation;
using Villsource.FSH.Modules.Organization.Contracts.v1.Structures;

namespace Villsource.FSH.Modules.Organization.Features.v1.Structures.Positions;

public sealed class GetPositionByIdQueryValidator : AbstractValidator<GetPositionByIdQuery>
{
    public GetPositionByIdQueryValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Position ID is required.");
    }
}
