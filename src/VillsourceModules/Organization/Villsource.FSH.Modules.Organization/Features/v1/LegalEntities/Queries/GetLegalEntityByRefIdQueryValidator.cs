using FluentValidation;
using Villsource.FSH.Modules.Organization.Contracts.v1.LegalEntities.Queries;
using Villsource.Tool.UniqueKey;

namespace Villsource.FSH.Modules.Organization.Features.v1.LegalEntities;

public sealed class GetLegalEntityByRefIdQueryValidator : AbstractValidator<GetLegalEntityByRefIdQuery>
{
    public GetLegalEntityByRefIdQueryValidator()
    {
        RuleFor(x => x.RefId).NotEmpty().MaximumLength(VillsourceId.KeySizes);
    }
}