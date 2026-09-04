using Mediator;
using Villsource.FSH.Modules.Organization.Contracts.Constants;

namespace Villsource.FSH.Modules.Organization.Contracts.v1.Structures;

public sealed record GetPositionTiersQuery() : IQuery<IReadOnlyCollection<PositionTier>>;