using FluentValidation;
using Villsource.FSH.Modules.Organization.Contracts.v1.Positions.Queries;
using Villsource.Tool.UniqueKey;

namespace Villsource.FSH.Modules.Organization.Features.v1.Positions.Queries;

public sealed class ResolveApprovalAuthorityQueryValidator : AbstractValidator<ResolveApprovalAuthorityQuery>
{
    public ResolveApprovalAuthorityQueryValidator()
    {
        RuleFor(x => x.PositionRefId).NotEmpty().MaximumLength(VillsourceId.KeySizes);
        RuleFor(x => x.DocumentTypeCode).MaximumLength(50).When(x => !string.IsNullOrWhiteSpace(x.DocumentTypeCode));
    }
}
