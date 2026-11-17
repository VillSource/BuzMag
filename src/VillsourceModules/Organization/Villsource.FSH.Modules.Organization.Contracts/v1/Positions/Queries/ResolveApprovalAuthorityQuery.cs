using Mediator;
using Villsource.FSH.Modules.Organization.Contracts.v1.Positions.Dtos;

namespace Villsource.FSH.Modules.Organization.Contracts.v1.Positions.Queries;

public sealed record ResolveApprovalAuthorityQuery(
    string PositionRefId,
    string? DocumentTypeCode = null,
    DateTimeOffset? AsOf = null) : IRequest<ApprovalChainDto>;
