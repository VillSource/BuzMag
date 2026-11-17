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
        await DocumentTypeScopeValidator
            .EnsureRegisteredAsync(dbContext, delegationType, request.DocumentTypeScope, cancellationToken)
            .ConfigureAwait(false);

        var utcFrom = request.ValidFrom.ToUniversalTime();
        var utcTo = request.ValidTo.ToUniversalTime();

        // Fetch tracked entity + other overlapping delegations
        var target = await dbContext.PositionDelegations
                         .Where(d => d.ReferenceId == request.RefId)
                         .Select(d => new
                         {
                             Entity = d,
                             Overlaps = dbContext.PositionDelegations
                                 .Where(other =>
                                     other.DelegatorPositionId == d.DelegatorPositionId &&
                                     other.Id != d.Id &&
                                     !other.IsRevoked &&
                                     !other.IsDeleted &&
                                     other.ValidFrom < utcTo &&
                                     other.ValidTo > utcFrom)
                                 .Select(other => new { other.DelegationType, other.DocumentTypeScope })
                                 .ToList()
                         })
                         .FirstOrDefaultAsync(cancellationToken)
                         .ConfigureAwait(false)
                     ?? throw new NotFoundException($"Position delegation '{request.RefId}' was not found.");

        if (delegationType == DelegationType.SpecificDocuments)
        {
            var newDocScopes = (request.DocumentTypeScope ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(s => s.ToUpperInvariant())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var existing in target.Overlaps)
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
            if (target.Overlaps.Count > 0)
            {
                throw new ConflictException(
                    "An active delegation already overlaps with the specified validity period.");
            }
        }

        var changed = target.Entity.Update(
            delegationType: delegationType,
            validFrom: utcFrom,
            validTo: utcTo,
            documentTypeScope: request.DocumentTypeScope,
            reason: request.Reason);

        if (changed)
        {
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        return target.Entity.ReferenceId;
    }
}