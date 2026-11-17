using Riok.Mapperly.Abstractions;
using System.Diagnostics.CodeAnalysis;
using Villsource.FSH.Modules.Organization.Contracts.v1.OrganizationUnits.Dtos;
using Villsource.FSH.Modules.Organization.Contracts.v1.OrganizationUnitTypes.Dtos;
using Villsource.FSH.Modules.Organization.Domain.OrganizationUnits;
using Villsource.FSH.Modules.Organization.Domain.OrganizationUnitTypes;

namespace Villsource.FSH.Modules.Organization.Features.v1.OrganizationUnits;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public static partial class OrganizationUnitMapper
{
    [MapProperty(nameof(OrganizationUnit.ReferenceId), nameof(OrganizationUnitDetailDto.RefId))]
    [MapPropertyFromSource(nameof(OrganizationUnitDetailDto.LegalEntityRefId), Use = nameof(GetLegalEntityRefId))]
    [MapPropertyFromSource(nameof(OrganizationUnitDetailDto.ParentRefId), Use = nameof(GetParentRefId))]
    [MapPropertyFromSource(nameof(OrganizationUnitDetailDto.ParentCode), Use = nameof(GetParentCode))]
    [MapPropertyFromSource(nameof(OrganizationUnitDetailDto.ParentName), Use = nameof(GetParentName))]
    [MapPropertyFromSource(nameof(OrganizationUnitDetailDto.DefaultCostCenterRefId), Use = nameof(GetDefaultCostCenterRefId))]
    [MapPropertyFromSource(nameof(OrganizationUnitDetailDto.ManagerPositionRefId), Use = nameof(GetManagerPositionRefId))]
    [MapPropertyFromSource(nameof(OrganizationUnitDetailDto.ManagerPositionCode), Use = nameof(GetManagerPositionCode))]
    [MapPropertyFromSource(nameof(OrganizationUnitDetailDto.ManagerPositionTitle), Use = nameof(GetManagerPositionTitle))]
    [MapPropertyFromSource(nameof(OrganizationUnitDetailDto.Children), Use = nameof(MapChildren))]
    [MapPropertyFromSource(nameof(OrganizationUnitDetailDto.CreatedOnUtc), Use = nameof(GetCreatedOnUtc))]
    [MapPropertyFromSource(nameof(OrganizationUnitDetailDto.LastModifiedOnUtc), Use = nameof(GetLastModifiedOnUtc))]
    public static partial OrganizationUnitDetailDto ToDetailDto(this OrganizationUnit entity);

    [MapProperty(nameof(OrganizationUnitType.ReferenceId), nameof(OrganizationUnitTypeLookupDto.RefId))]
    public static partial OrganizationUnitTypeLookupDto ToTypeLookupDto(this OrganizationUnitType type);

    [MapProperty(nameof(OrganizationUnit.ReferenceId), nameof(OrganizationUnitChildDto.RefId))]
    [MapPropertyFromSource(nameof(OrganizationUnitChildDto.OrganizationUnitTypeCode), Use = nameof(GetUnitTypeCode))]
    [MapPropertyFromSource(nameof(OrganizationUnitChildDto.OrganizationUnitTypeName), Use = nameof(GetUnitTypeName))]
    [MapPropertyFromSource(nameof(OrganizationUnitChildDto.ManagerPositionRefId), Use = nameof(GetManagerPositionRefId))]
    [MapPropertyFromSource(nameof(OrganizationUnitChildDto.ManagerPositionTitle), Use = nameof(GetManagerPositionTitle))]
    public static partial OrganizationUnitChildDto ToChildDto(this OrganizationUnit entity);

    private static string GetUnitTypeCode(OrganizationUnit entity) => entity.OrganizationUnitType?.Code ?? string.Empty;
    private static string GetUnitTypeName(OrganizationUnit entity) => entity.OrganizationUnitType?.Name ?? string.Empty;

    private static string? GetLegalEntityRefId(OrganizationUnit entity) => entity.LegalEntity?.ReferenceId;
    private static string? GetDefaultCostCenterRefId(OrganizationUnit entity) => entity.DefaultCostCenter?.ReferenceId;
    private static string? GetParentRefId(OrganizationUnit entity) => entity.Parent?.ReferenceId;
    private static string? GetParentCode(OrganizationUnit entity) => entity.Parent?.Code;
    private static string? GetParentName(OrganizationUnit entity) => entity.Parent?.Name;
    private static string? GetManagerPositionRefId(OrganizationUnit entity) => entity.ManagerPosition is { IsDeleted: false } pos ? pos.ReferenceId : null;
    private static string? GetManagerPositionCode(OrganizationUnit entity) => entity.ManagerPosition is { IsDeleted: false } pos ? pos.Code : null;
    private static string? GetManagerPositionTitle(OrganizationUnit entity) => entity.ManagerPosition is { IsDeleted: false } pos ? pos.Title : null;
    private static DateTimeOffset GetCreatedOnUtc(OrganizationUnit entity) => entity.CreatedOnUtc;
    private static DateTimeOffset? GetLastModifiedOnUtc(OrganizationUnit entity) => entity.LastModifiedOnUtc;

    [SuppressMessage("Performance", "CA1859:Use concrete types when possible for improved performance")]
    private static IReadOnlyList<OrganizationUnitChildDto> MapChildren(OrganizationUnit entity) =>
        entity.Children
            .Where(c => !c.IsDeleted)
            .OrderBy(c => c.Code)
            .Select(c => c.ToChildDto())
            .ToList();
}