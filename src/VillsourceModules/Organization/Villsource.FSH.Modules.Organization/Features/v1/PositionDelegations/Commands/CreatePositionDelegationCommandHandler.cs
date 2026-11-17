using FSH.Framework.Core.Exceptions;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Villsource.FSH.Exceptions;
using Villsource.FSH.Modules.Organization.Contracts.v1.PositionDelegations.Commands;
using Villsource.FSH.Modules.Organization.Data;
using Villsource.FSH.Modules.Organization.Domain.PositionDelegations;

namespace Villsource.FSH.Modules.Organization.Features.v1.PositionDelegations.Commands;

public sealed class CreatePositionDelegationCommandHandler(OrganizationDbContext dbContext)
    : IRequestHandler<CreatePositionDelegationCommand, string>
{
    public async ValueTask<string> Handle(
        CreatePositionDelegationCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var legalEntityId = await dbContext.LegalEntities
                                .AsNoTracking()
                                .Where(le => le.ReferenceId == request.LegalEntityRefId)
                                .Select(le => (Guid?)le.Id)
                                .FirstOrDefaultAsync(cancellationToken)
                            ?? throw new NotFoundException($"Legal entity '{request.LegalEntityRefId}' was not found.");

        var positions = await dbContext.Positions
            .AsNoTracking()
            .Where(p => p.ReferenceId == request.DelegatorPositionRefId || p.ReferenceId == request.DelegatePositionRefId)
            .Select(p => new { p.Id, p.ReferenceId, p.LegalEntityId, p.IsActive })
            .ToListAsync(cancellationToken);

        var delegator = positions.FirstOrDefault(p => p.ReferenceId == request.DelegatorPositionRefId)
                        ?? throw new NotFoundException($"Delegator position '{request.DelegatorPositionRefId}' was not found.");

        var delegatePos = positions.FirstOrDefault(p => p.ReferenceId == request.DelegatePositionRefId)
                          ?? throw new NotFoundException($"Delegate position '{request.DelegatePositionRefId}' was not found.");

        if (delegator.LegalEntityId != legalEntityId || delegatePos.LegalEntityId != legalEntityId)
        {
            throw new InvalidOperationException("Both positions must belong to the specified legal entity.");
        }

        if (!delegator.IsActive || !delegatePos.IsActive)
        {
            throw new InvalidOperationException("Both delegator and delegate positions must be active.");
        }

        var delegationType = DelegationType.FromKey(request.DelegationType);
        var utcFrom = request.ValidFrom.ToUniversalTime();
        var utcTo = request.ValidTo.ToUniversalTime();

        // Single DB round-trip: detect both same-type overlap AND reciprocal ping-pong delegation overlap
        var conflictCheck = await dbContext.PositionDelegations
            .AsNoTracking()
            .Where(d => !d.IsRevoked &&
                        d.ValidFrom < utcTo &&
                        d.ValidTo > utcFrom &&
                        ((d.DelegatorPositionId == delegator.Id && d.DelegationType == delegationType) ||
                         (d.DelegatorPositionId == delegatePos.Id && d.DelegatePositionId == delegator.Id)))
            .Select(d => new { d.DelegatorPositionId })
            .FirstOrDefaultAsync(cancellationToken);

        if (conflictCheck is not null)
        {
            if (conflictCheck.DelegatorPositionId == delegator.Id)
            {
                throw new ConflictException(
                    "An active delegation of the same type already overlaps with the specified validity period.");
            }

            throw new InvalidOperationException(
                "Creating this delegation would form a reciprocal delegation loop during the overlapping period.");
        }

        var entity = PositionDelegation.Create(
            legalEntityId: legalEntityId,
            delegatorPositionId: delegator.Id,
            delegatePositionId: delegatePos.Id,
            delegationType: delegationType,
            validFrom: utcFrom,
            validTo: utcTo,
            documentTypeScope: request.DocumentTypeScope,
            reason: request.Reason);

        await dbContext.PositionDelegations.AddAsync(entity, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return entity.ReferenceId;
    }
}