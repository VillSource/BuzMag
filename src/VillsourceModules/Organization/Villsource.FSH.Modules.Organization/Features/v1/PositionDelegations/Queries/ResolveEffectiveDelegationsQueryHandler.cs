using Mediator;
using Microsoft.EntityFrameworkCore;
using Villsource.FSH.Modules.Organization.Contracts.v1.PositionDelegations.Dtos;
using Villsource.FSH.Modules.Organization.Contracts.v1.PositionDelegations.Queries;
using Villsource.FSH.Modules.Organization.Data;

namespace Villsource.FSH.Modules.Organization.Features.v1.PositionDelegations.Queries;

public sealed class ResolveEffectiveDelegationsQueryHandler(OrganizationDbContext dbContext)
    : IRequestHandler<ResolveEffectiveDelegationsQuery, IReadOnlyList<EffectiveDelegationDto>>
{
    public async ValueTask<IReadOnlyList<EffectiveDelegationDto>> Handle(
        ResolveEffectiveDelegationsQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var targetUtc = (request.AtUtc ?? DateTimeOffset.UtcNow).ToUniversalTime();

        var candidates = await dbContext.PositionDelegations
            .AsNoTracking()
            .Include("DelegatePosition.JobGrade")
            .Where(d => d.DelegatorPosition!.ReferenceId == request.DelegatorPositionRefId &&
                        !d.IsRevoked &&
                        d.ValidFrom <= targetUtc &&
                        d.ValidTo >= targetUtc &&
                        d.DelegatePosition!.IsActive)
            .OrderByDescending(d => d.ValidFrom)
            .ToListAsync(cancellationToken);

        return candidates
            .Where(d => d.IsEffectiveAt(targetUtc, request.DocumentTypeCode))
            .Select(d => new EffectiveDelegationDto(
                d.ReferenceId,
                d.DelegationType.Key,
                d.DocumentTypeScope,
                d.ValidFrom,
                d.ValidTo,
                Mappers.PositionDelegationMapper.ToPositionDto(d.DelegatePosition!)))
            .ToList();
    }
}