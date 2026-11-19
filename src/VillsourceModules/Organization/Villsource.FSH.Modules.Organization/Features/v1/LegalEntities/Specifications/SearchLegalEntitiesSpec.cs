using FSH.Framework.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Villsource.FSH.Modules.Organization.Contracts.v1.LegalEntities.Dtos;
using Villsource.FSH.Modules.Organization.Contracts.v1.LegalEntities.Queries;
using Villsource.FSH.Modules.Organization.Domain.LegalEntities;

namespace Villsource.FSH.Modules.Organization.Features.v1.LegalEntities.Specifications;

public sealed class SearchLegalEntitiesSpec : Specification<LegalEntity, LegalEntitySummaryDto>
{
    private static readonly IReadOnlyDictionary<string, Expression<Func<LegalEntity, object>>> SortMappings =
        new Dictionary<string, Expression<Func<LegalEntity, object>>>(StringComparer.OrdinalIgnoreCase)
        {
            ["code"] = e => e.Code,
            ["countryCode"] = e => e.CountryCode,
            ["legalName"] = e => e.LegalName,
            ["internationalName"] = e => e.InternationalName,
            ["taxId"] = e => e.TaxId,
            ["createdOnUtc"] = e => e.CreatedOnUtc
        };

    public SearchLegalEntitiesSpec(SearchLegalEntitiesQuery request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!string.IsNullOrWhiteSpace(request.CountryCode))
        {
            var countryCode = request.CountryCode.Trim().ToUpperInvariant();
            Where(e => e.CountryCode == countryCode);
        }

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var keyword = request.Keyword.Trim();
            Where(e =>
                EF.Functions.ILike(e.Code, $"%{keyword}%") ||
                EF.Functions.ILike(e.LegalName, $"%{keyword}%") ||
                EF.Functions.ILike(e.InternationalName, $"%{keyword}%") ||
                EF.Functions.ILike(e.TaxId, $"%{keyword}%") ||
                (e.Profile.TradeName != null && EF.Functions.ILike(e.Profile.TradeName, $"%{keyword}%")));
        }

        ApplySortingOverride(
            sortExpression: request.Sort,
            applyDefaultOrdering: () => OrderBy(e => e.Code),
            sortMappings: SortMappings);

        Select(e => new LegalEntitySummaryDto(
            e.ReferenceId,
            e.Code,
            e.CountryCode,
            e.LegalName,
            e.InternationalName,
            e.TaxId,
            e.RegistrationNumber,
            e.BaseCurrency,
            e.Profile.TradeName,
            e.Profile.LogoUrl,
            e.Branches.Count(b => b.IsActive && !b.IsDeleted)));
    }
}
