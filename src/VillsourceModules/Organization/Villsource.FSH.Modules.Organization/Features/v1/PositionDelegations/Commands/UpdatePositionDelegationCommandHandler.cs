using FSH.Framework.Core.Exceptions;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Villsource.FSH.Exceptions;
using Villsource.FSH.Modules.Organization.Contracts.v1.PositionDelegations.Commands;
using Villsource.FSH.Modules.Organization.Data;
using Villsource.FSH.Modules.Organization.Domain.PositionDelegations;

namespace Villsource.FSH.Modules.Organization.Features.v1.PositionDelegations.Commands;

public sealed class UpdatePositionDelegationCommandHandler(OrganizationDbContext dbContext)
    : IRequestHandler<UpdatePositionDelegationCommand, string>
{
    public async ValueTask<string> Handle(
        UpdatePositionDelegationCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var delegationType = DelegationType.FromKey(request.DelegationType);
        var utcFrom = request.ValidFrom.ToUniversalTime();
        var utcTo = request.ValidTo.ToUniversalTime();

        // Single DB round-trip: fetch tracked entity + check overlapping same-type delegation
        var target = await dbContext.PositionDelegations
                         .Where(d => d.ReferenceId == request.RefId)
                         .Select(d => new
                         {
                             Entity = d,
                             HasOverlap = dbContext.PositionDelegations.Any(other =>
                                 other.DelegatorPositionId == d.DelegatorPositionId &&
                                 other.Id != d.Id &&
                                 !other.IsRevoked &&
                                 other.DelegationType == delegationType &&
                                 other.ValidFrom < utcTo &&
                                 other.ValidTo > utcFrom)
                         })
                         .FirstOrDefaultAsync(cancellationToken)
                     ?? throw new NotFoundException($"Position delegation '{request.RefId}' was not found.");

        if (target.HasOverlap)
        {
            throw new ConflictException(
                "An active delegation of the same type already overlaps with the specified validity period.");
        }

        var changed = target.Entity.Update(
            delegationType: delegationType,
            validFrom: utcFrom,
            validTo: utcTo,
            documentTypeScope: request.DocumentTypeScope,
            reason: request.Reason);

        if (changed)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return target.Entity.ReferenceId;
    }
}