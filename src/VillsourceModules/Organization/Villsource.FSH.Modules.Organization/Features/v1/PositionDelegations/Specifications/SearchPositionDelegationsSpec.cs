using FSH.Framework.Persistence;
using System.Linq.Expressions;
using Villsource.FSH.Modules.Organization.Contracts.v1.PositionDelegations.Dtos;
using Villsource.FSH.Modules.Organization.Contracts.v1.PositionDelegations.Queries;
using Villsource.FSH.Modules.Organization.Domain.PositionDelegations;

namespace Villsource.FSH.Modules.Organization.Features.v1.PositionDelegations.Specifications;

public sealed class SearchPositionDelegationsSpec : Specification<PositionDelegation, PositionDelegationSummaryDto>
{
    private static readonly IReadOnlyDictionary<string, Expression<Func<PositionDelegation, object>>> SortMappings =
        new Dictionary<string, Expression<Func<PositionDelegation, object>>>(StringComparer.OrdinalIgnoreCase)
        {
            ["validFrom"] = d => d.ValidFrom,
            ["validTo"] = d => d.ValidTo,
            ["isRevoked"] = d => d.IsRevoked,
            ["createdOnUtc"] = d => d.CreatedOnUtc
        };

    public SearchPositionDelegationsSpec(SearchPositionDelegationsQuery request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!string.IsNullOrWhiteSpace(request.LegalEntityRefId))
        {
            var leRef = request.LegalEntityRefId.Trim();
            Where(d => d.LegalEntity!.ReferenceId == leRef);
        }

        if (!string.IsNullOrWhiteSpace(request.DelegatorPositionRefId))
        {
            var delegatorRef = request.DelegatorPositionRefId.Trim();
            Where(d => d.DelegatorPosition!.ReferenceId == delegatorRef);
        }

        if (!string.IsNullOrWhiteSpace(request.DelegatePositionRefId))
        {
            var delegateRef = request.DelegatePositionRefId.Trim();
            Where(d => d.DelegatePosition!.ReferenceId == delegateRef);
        }

        if (request.IsRevoked.HasValue)
        {
            Where(d => d.IsRevoked == request.IsRevoked.Value);
        }

        if (request.EffectiveAtUtc.HasValue)
        {
            var utcTarget = request.EffectiveAtUtc.Value.ToUniversalTime();
            Where(d => !d.IsRevoked && d.ValidFrom <= utcTarget && d.ValidTo >= utcTarget);
        }

        ApplySortingOverride(
            sortExpression: request.Sort,
            applyDefaultOrdering: () => OrderByDescending(d => d.ValidFrom),
            sortMappings: SortMappings);

        Select(d => new PositionDelegationSummaryDto(
            d.ReferenceId,
            d.LegalEntity!.ReferenceId,
            d.DelegatorPosition!.ReferenceId,
            d.DelegatorPosition!.Code,
            d.DelegatorPosition!.Title,
            d.DelegatePosition!.ReferenceId,
            d.DelegatePosition!.Code,
            d.DelegatePosition!.Title,
            (string)d.DelegationType,
            d.DocumentTypeScope,
            d.ValidFrom,
            d.ValidTo,
            d.IsRevoked));
    }
}