using FSH.Framework.Core.Exceptions;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Villsource.FSH.Exceptions;
using Villsource.FSH.Modules.Organization.Contracts.v1.PositionDelegations.Commands;
using Villsource.FSH.Modules.Organization.Data;
using Villsource.FSH.Modules.Organization.Domain.PositionDelegations;

namespace Villsource.FSH.Modules.Organization.Features.v1.PositionDelegations.Commands;

public sealed class CreatePositionDelegationCommandHandler(
    OrganizationDbContext dbContext,
    ILogger<CreatePositionDelegationCommandHandler> logger)
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
                                .ConfigureAwait(false)
                            ?? throw new NotFoundException($"Legal entity '{request.LegalEntityRefId}' was not found.");

        var positions = await dbContext.Positions
            .AsNoTracking()
            .Where(p => p.ReferenceId == request.DelegatorPositionRefId || p.ReferenceId == request.DelegatePositionRefId)
            .Select(p => new { p.Id, p.ReferenceId, p.Code, p.LegalEntityId, p.IsActive, p.HeadcountCapacity })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

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
        await DocumentTypeScopeValidator
            .EnsureRegisteredAsync(dbContext, delegationType, request.DocumentTypeScope, cancellationToken)
            .ConfigureAwait(false);

        var utcFrom = request.ValidFrom.ToUniversalTime();
        var utcTo = request.ValidTo.ToUniversalTime();

        // Multi-node transitive loop detection: check if delegatePos already delegates (directly or indirectly) to delegator
        var activeDelegations = await dbContext.PositionDelegations
            .AsNoTracking()
            .Where(d => d.LegalEntityId == legalEntityId &&
                        !d.IsRevoked &&
                        !d.IsDeleted &&
                        d.ValidFrom < utcTo &&
                        d.ValidTo > utcFrom)
            .Select(d => new { d.DelegatorPositionId, d.DelegatePositionId })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var visited = new HashSet<Guid> { delegator.Id };
        var queue = new Queue<Guid>();
        queue.Enqueue(delegatePos.Id);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (!visited.Add(current))
            {
                throw new InvalidOperationException(
                    "Creating this delegation would form a circular/transitive delegation loop during the overlapping period.");
            }

            foreach (var d in activeDelegations.Where(x => x.DelegatorPositionId == current))
            {
                queue.Enqueue(d.DelegatePositionId);
            }
        }

        // Check overlapping delegations for the delegator
        var existingOverlaps = await dbContext.PositionDelegations
            .AsNoTracking()
            .Where(d => d.DelegatorPositionId == delegator.Id &&
                        !d.IsRevoked &&
                        !d.IsDeleted &&
                        d.ValidFrom < utcTo &&
                        d.ValidTo > utcFrom)
            .Select(d => new { d.DelegationType, d.DocumentTypeScope })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (delegationType == DelegationType.SpecificDocuments)
        {
            var newDocScopes = (request.DocumentTypeScope ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(s => s.ToUpperInvariant())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var existing in existingOverlaps)
            {
                if (existing.DelegationType != DelegationType.SpecificDocuments)
                {
                    throw new ConflictException(
                        "An active delegation with broader authority already overlaps with the specified validity period.");
                }

                var existingDocs = (existing.DocumentTypeScope ?? string.Empty)
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(s => s.ToUpperInvariant());

                if (existingDocs.Any(d => newDocScopes.Contains(d)))
                {
                    throw new ConflictException(
                        "An active delegation for one or more of the specified document types already overlaps with the validity period.");
                }
            }
        }
        else
        {
            if (existingOverlaps.Count > 0)
            {
                throw new ConflictException(
                    "An active delegation already overlaps with the specified validity period.");
            }
        }

        if (delegator.HeadcountCapacity > 1)
        {
            logger.LogWarning(
                "Position delegation created for pool position {Code} with HeadcountCapacity {Capacity}. Specific individual resolution may be ambiguous.",
                delegator.Code,
                delegator.HeadcountCapacity);
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

        await dbContext.PositionDelegations.AddAsync(entity, cancellationToken).ConfigureAwait(false);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return entity.ReferenceId;
    }
}