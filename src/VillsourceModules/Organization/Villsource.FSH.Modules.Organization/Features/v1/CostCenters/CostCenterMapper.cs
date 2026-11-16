using System.Diagnostics.CodeAnalysis;
using Riok.Mapperly.Abstractions;
using Villsource.FSH.Modules.Organization.Contracts.Constants;
using Villsource.FSH.Modules.Organization.Contracts.v1.CostCenters;
using Villsource.FSH.Modules.Organization.Contracts.v1.CostCenters.Dtos;
using Villsource.FSH.Modules.Organization.Domain.CostCenters;
using Villsource.FSH.Modules.Organization.Domain.LegalEntities;

namespace Villsource.FSH.Modules.Organization.Features.v1.CostCenters;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public static partial class CostCenterMapper
{
    [MapProperty(nameof(CostCenter.ReferenceId), nameof(CostCenterDetailDto.RefId))]
    [MapProperty([nameof(CostCenter.LegalEntity), nameof(LegalEntity.ReferenceId)], nameof(CostCenterDetailDto.LegalEntityRefId))]
    [MapProperty([nameof(CostCenter.LegalEntity), nameof(LegalEntity.Code)], nameof(CostCenterDetailDto.LegalEntityCode))]
    [MapProperty([nameof(CostCenter.LegalEntity), nameof(LegalEntity.LegalName)], nameof(CostCenterDetailDto.LegalEntityName))]
    [MapPropertyFromSource(nameof(CostCenterDetailDto.ChildCostCenters), Use = nameof(MapChildCostCenters))]
    [MapPropertyFromSource(nameof(CostCenterDetailDto.CreatedOnUtc), Use = nameof(GetCreatedOnUtc))]
    [MapPropertyFromSource(nameof(CostCenterDetailDto.LastModifiedOnUtc), Use = nameof(GetLastModifiedOnUtc))]
    public static partial CostCenterDetailDto ToDetailDto(this CostCenter entity);

    [MapProperty(nameof(CostCenter.ReferenceId), nameof(CostCenterNodeDto.RefId))]
    [MapProperty(nameof(CostCenter.FunctionType), nameof(CostCenterNodeDto.FunctionType), Use = nameof(MapFunctionTypeKey))]
    public static partial CostCenterNodeDto ToNodeDto(this CostCenter entity);

    [MapProperty(nameof(TaxBranch.ReferenceId), nameof(CostCenterTaxBranchDto.RefId))]
    public static partial CostCenterTaxBranchDto ToBranchDto(this TaxBranch branch);

    public static partial CostCenterFunctionTypeDto ToDto(this CostCenterFunctionType functionType);

    private static string MapFunctionTypeKey(CostCenterFunctionType functionType) => functionType.Key;

    private static DateTimeOffset GetCreatedOnUtc(CostCenter entity) => entity.CreatedOnUtc;

    private static DateTimeOffset? GetLastModifiedOnUtc(CostCenter entity) => entity.LastModifiedOnUtc;

    [SuppressMessage("Performance", "CA1859:Use concrete types when possible for improved performance")]
    private static IReadOnlyList<CostCenterNodeDto> MapChildCostCenters(CostCenter entity) =>
        entity.Children
            .Where(c => !c.IsDeleted)
            .OrderBy(c => c.Code)
            .Select(c => c.ToNodeDto())
            .ToList();
}