using Villsource.FSH.Modules.Organization.Contracts.Services.Dtos;
using Villsource.FSH.Modules.Organization.Domain.PositionDelegations;

namespace Villsource.FSH.Modules.Organization.Services.ChainOfCommand;

internal sealed class DelegationResolver(ChainOfCommandReadStore store)
{
    public async Task<IReadOnlyList<ResolvedDelegationDto>> ResolveEffectiveDelegatesAsync(
        string delegatorPositionRefId,
        string? documentTypeCode,
        string? delegationTypeKey,
        DateTimeOffset targetUtc,
        CancellationToken cancellationToken)
    {
        var typeFilter = ParseOptionalDelegationType(delegationTypeKey);
        var delegator = await store.GetPositionByRefIdAsync(delegatorPositionRefId, cancellationToken)
            .ConfigureAwait(false);

        var delegations = await store.LoadActiveDelegationsForDelegatorAsync(delegator.Id, targetUtc, cancellationToken)
            .ConfigureAwait(false);

        if (delegations.Count == 0)
        {
            return [];
        }

        var delegateIds = delegations.Select(d => d.DelegatePositionId).Distinct().ToList();
        var delegatePositions = await store.GetPositionsByIdsAsync(delegateIds, cancellationToken).ConfigureAwait(false);

        return MapAndOrderDelegations(
            delegations,
            delegatorId => delegatorId == delegator.Id ? delegator : null,
            delegateId => delegatePositions.GetValueOrDefault(delegateId),
            targetUtc,
            documentTypeCode,
            typeFilter,
            requireDocumentCodeForScoped: false);
    }

    public async Task<IReadOnlyList<ResolvedDelegationDto>> ResolveActiveDelegatorsAsync(
        string delegatePositionRefId,
        string? documentTypeCode,
        string? delegationTypeKey,
        DateTimeOffset targetUtc,
        CancellationToken cancellationToken)
    {
        var typeFilter = ParseOptionalDelegationType(delegationTypeKey);
        var delegatePos = await store.GetPositionByRefIdAsync(delegatePositionRefId, cancellationToken)
            .ConfigureAwait(false);

        var delegations = await store.LoadActiveDelegationsForDelegateAsync(delegatePos.Id, targetUtc, cancellationToken)
            .ConfigureAwait(false);

        if (delegations.Count == 0)
        {
            return [];
        }

        var delegatorIds = delegations.Select(d => d.DelegatorPositionId).Distinct().ToList();
        var delegatorPositions = await store.GetPositionsByIdsAsync(delegatorIds, cancellationToken).ConfigureAwait(false);

        return MapAndOrderDelegations(
            delegations,
            delegatorId => delegatorPositions.GetValueOrDefault(delegatorId),
            delegateId => delegateId == delegatePos.Id ? delegatePos : null,
            targetUtc,
            documentTypeCode,
            typeFilter,
            requireDocumentCodeForScoped: false);
    }

    public static EffectiveManagerResolutionDto BuildEffectiveManagerResolution(
        int stepOrder,
        ManagerCandidate candidate,
        IReadOnlyDictionary<Guid, PositionProjection> positions,
        IReadOnlyList<PositionDelegation> delegations,
        DateTimeOffset targetUtc,
        string? documentTypeCode)
    {
        var managerDelegations = delegations
            .Where(d => d.DelegatorPositionId == candidate.Position.Id)
            .ToList();

        ResolvedDelegationDto? primaryDelegation = null;
        if (managerDelegations.Count > 0)
        {
            primaryDelegation = MapAndOrderDelegations(
                managerDelegations,
                _ => candidate.Position,
                delegateId => positions.GetValueOrDefault(delegateId),
                targetUtc,
                documentTypeCode,
                typeFilter: null,
                requireDocumentCodeForScoped: true).FirstOrDefault();
        }

        var isDelegated = primaryDelegation is not null;
        var effectiveApprover = primaryDelegation?.DelegatePosition ?? candidate.Position.Dto;

        return new EffectiveManagerResolutionDto(
            StepOrder: stepOrder,
            ResolutionSource: candidate.Source,
            NominalManagerPosition: candidate.Position.Dto,
            EffectiveApproverPosition: effectiveApprover,
            IsDelegated: isDelegated,
            ActiveDelegation: primaryDelegation,
            ResolvedFromOrgUnitRefId: candidate.ResolvedFromOrgUnitRefId);
    }

    private static DelegationType? ParseOptionalDelegationType(string? delegationTypeKey) =>
        string.IsNullOrWhiteSpace(delegationTypeKey) ? null : DelegationType.FromKey(delegationTypeKey);

    private static int GetDelegationPriority(DelegationType delegationType)
    {
        if (delegationType == DelegationType.ActingFull)
        {
            return 1;
        }

        if (delegationType == DelegationType.ApprovalOnly)
        {
            return 2;
        }

        return 3;
    }

    private static bool IsDelegationApplicable(
        PositionDelegation delegation,
        DateTimeOffset targetUtc,
        string? documentTypeCode,
        bool requireDocumentCodeForScoped)
    {
        if (requireDocumentCodeForScoped &&
            delegation.DelegationType.RequiresDocumentScope &&
            string.IsNullOrWhiteSpace(documentTypeCode))
        {
            return false;
        }

        return delegation.IsEffectiveAt(targetUtc, documentTypeCode);
    }

    private static List<ResolvedDelegationDto> MapAndOrderDelegations(
        IReadOnlyList<PositionDelegation> delegations,
        Func<Guid, PositionProjection?> resolveDelegator,
        Func<Guid, PositionProjection?> resolveDelegate,
        DateTimeOffset targetUtc,
        string? documentTypeCode,
        DelegationType? typeFilter,
        bool requireDocumentCodeForScoped)
    {
        var items = new List<(int Priority, DateTimeOffset ValidFrom, ResolvedDelegationDto Dto)>();

        foreach (var delegation in delegations)
        {
            if (typeFilter is not null && delegation.DelegationType != typeFilter)
            {
                continue;
            }

            if (!IsDelegationApplicable(delegation, targetUtc, documentTypeCode, requireDocumentCodeForScoped))
            {
                continue;
            }

            var delegator = resolveDelegator(delegation.DelegatorPositionId);
            var delegatePos = resolveDelegate(delegation.DelegatePositionId);
            if (delegator is null ||
                delegatePos is null ||
                !ChainOfCommandPredicates.IsPositionEffective(delegator.Dto, targetUtc) ||
                !ChainOfCommandPredicates.IsPositionEffective(delegatePos.Dto, targetUtc))
            {
                continue;
            }

            items.Add((
                GetDelegationPriority(delegation.DelegationType),
                delegation.ValidFrom,
                new ResolvedDelegationDto(
                    DelegationRefId: delegation.ReferenceId,
                    DelegationTypeKey: delegation.DelegationType.Key,
                    DocumentTypeScope: delegation.DocumentTypeScope,
                    Reason: delegation.Reason,
                    ValidFrom: delegation.ValidFrom,
                    ValidTo: delegation.ValidTo,
                    DelegatorPosition: delegator.Dto,
                    DelegatePosition: delegatePos.Dto)));
        }

        return items
            .OrderBy(x => x.Priority)
            .ThenByDescending(x => x.ValidFrom)
            .Select(x => x.Dto)
            .ToList();
    }
}
