using FSH.Framework.Core.Exceptions;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Villsource.FSH.Modules.Organization.Contracts.v1.Positions.Dtos;
using Villsource.FSH.Modules.Organization.Contracts.v1.Positions.Queries;
using Villsource.FSH.Modules.Organization.Data;

namespace Villsource.FSH.Modules.Organization.Features.v1.Positions.Queries;

public sealed class ResolveApprovalAuthorityQueryHandler(OrganizationDbContext dbContext)
    : IRequestHandler<ResolveApprovalAuthorityQuery, ApprovalChainDto>
{
    public async ValueTask<ApprovalChainDto> Handle(
        ResolveApprovalAuthorityQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var targetPosition = await dbContext.Positions
                                 .AsNoTracking()
                                 .Where(p => p.ReferenceId == request.PositionRefId && !p.IsDeleted)
                                 .Select(p => new { p.Id, p.ReferenceId, p.Code, p.Title, p.LegalEntityId, p.ReportsToPositionId })
                                 .FirstOrDefaultAsync(cancellationToken)
                                 .ConfigureAwait(false)
                             ?? throw new NotFoundException($"Position '{request.PositionRefId}' was not found.");

        var asOfUtc = (request.AsOf ?? DateTimeOffset.UtcNow).ToUniversalTime();

        // Load all active positions in the legal entity for traversal
        var positions = await dbContext.Positions
            .AsNoTracking()
            .Where(p => p.LegalEntityId == targetPosition.LegalEntityId && !p.IsDeleted)
            .Select(p => new
            {
                p.Id,
                p.ReferenceId,
                p.Code,
                p.Title,
                p.IsActive,
                p.ReportsToPositionId
            })
            .ToDictionaryAsync(p => p.Id, cancellationToken)
            .ConfigureAwait(false);

        // Load active delegations for positions in this legal entity
        var delegations = await dbContext.PositionDelegations
            .AsNoTracking()
            .Where(d => d.LegalEntityId == targetPosition.LegalEntityId &&
                        !d.IsDeleted &&
                        !d.IsRevoked &&
                        d.ValidFrom <= asOfUtc &&
                        d.ValidTo >= asOfUtc)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var chainNodes = new List<ApprovalNodeDto>();
        var visited = new HashSet<Guid> { targetPosition.Id };
        var currentManagerId = targetPosition.ReportsToPositionId;
        var step = 1;

        while (currentManagerId.HasValue && positions.TryGetValue(currentManagerId.Value, out var manager))
        {
            if (!visited.Add(manager.Id))
            {
                break; // Cycle safeguard
            }

            var isVacantOrInactive = !manager.IsActive;

            // Find matching active delegation
            var activeDelegation = delegations.FirstOrDefault(d =>
                d.DelegatorPositionId == manager.Id &&
                d.IsEffectiveAt(asOfUtc, request.DocumentTypeCode));

            string? effectiveRefId = null;
            string? effectiveCode = null;
            string? effectiveTitle = null;
            string? delegationRefId = null;
            string? delegationReason = null;
            var isDelegated = false;

            if (activeDelegation is not null && positions.TryGetValue(activeDelegation.DelegatePositionId, out var delegatePos))
            {
                isDelegated = true;
                effectiveRefId = delegatePos.ReferenceId;
                effectiveCode = delegatePos.Code;
                effectiveTitle = delegatePos.Title;
                delegationRefId = activeDelegation.ReferenceId;
                delegationReason = activeDelegation.Reason;
            }

            chainNodes.Add(new ApprovalNodeDto(
                StepOrder: step++,
                PositionRefId: manager.ReferenceId,
                PositionCode: manager.Code,
                PositionTitle: manager.Title,
                IsVacantOrInactive: isVacantOrInactive,
                IsDelegated: isDelegated,
                EffectiveApproverPositionRefId: effectiveRefId,
                EffectiveApproverPositionCode: effectiveCode,
                EffectiveApproverPositionTitle: effectiveTitle,
                DelegationRefId: delegationRefId,
                DelegationReason: delegationReason));

            currentManagerId = manager.ReportsToPositionId;
        }

        return new ApprovalChainDto(
            TargetPositionRefId: targetPosition.ReferenceId,
            TargetPositionCode: targetPosition.Code,
            TargetPositionTitle: targetPosition.Title,
            EvaluatedAtUtc: asOfUtc,
            DocumentTypeCode: request.DocumentTypeCode,
            Nodes: chainNodes);
    }
}
