using Riok.Mapperly.Abstractions;
using System.Diagnostics.CodeAnalysis;
using Villsource.FSH.Modules.Organization.Contracts.v1.LegalEntities;
using Villsource.FSH.Modules.Organization.Domain.LegalEntities;

namespace Villsource.FSH.Modules.Organization.Features.v1.LegalEntities;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public static partial class LegalEntityMapper
{
    [MapProperty(nameof(LegalEntity.ReferenceId), nameof(LegalEntityDetailDto.RefId))]
    [MapPropertyFromSource(nameof(LegalEntityDetailDto.Branches), Use = nameof(MapBranches))]
    [MapPropertyFromSource(nameof(LegalEntityDetailDto.CreatedOnUtc), Use = nameof(GetCreatedOnUtc))]
    [MapPropertyFromSource(nameof(LegalEntityDetailDto.LastModifiedOnUtc), Use = nameof(GetLastModifiedOnUtc))]
    public static partial LegalEntityDetailDto ToDetailDto(this LegalEntity entity);

    public static partial CompanyProfileDto ToDto(this CompanyProfile profile);

    [MapProperty(nameof(TaxBranch.ReferenceId), nameof(TaxBranchDto.RefId))]
    public static partial TaxBranchDto ToDto(this TaxBranch branch, string legalEntityRefId);

    private static DateTimeOffset GetCreatedOnUtc(LegalEntity entity) => entity.CreatedOnUtc;

    private static DateTimeOffset? GetLastModifiedOnUtc(LegalEntity entity) => entity.LastModifiedOnUtc;

    [SuppressMessage("Performance", "CA1859:Use concrete types when possible for improved performance")]
    private static IReadOnlyList<TaxBranchDto> MapBranches(LegalEntity entity) =>
        entity.Branches
            .OrderByDescending(b => b.IsHeadOffice)
            .ThenBy(b => b.BranchCode)
            .Select(b => b.ToDto(entity.ReferenceId))
            .ToList();
}