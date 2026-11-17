using Riok.Mapperly.Abstractions;
using System.Diagnostics.CodeAnalysis;
using Villsource.FSH.Modules.Organization.Contracts.v1.Positions.Dtos;
using Villsource.FSH.Modules.Organization.Domain.CostCenters;
using Villsource.FSH.Modules.Organization.Domain.JobGrades;
using Villsource.FSH.Modules.Organization.Domain.Jobs;
using Villsource.FSH.Modules.Organization.Domain.LegalEntities;

namespace Villsource.FSH.Modules.Organization.Features.v1.Positions.Mappers;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public static partial class PositionMapper
{
    [MapProperty(nameof(Domain.Positions.Position.ReferenceId), nameof(PositionDetailDto.RefId))]
    [MapProperty([nameof(Domain.Positions.Position.LegalEntity), nameof(LegalEntity.ReferenceId)], nameof(PositionDetailDto.LegalEntityRefId))]
    [MapProperty([nameof(Domain.Positions.Position.LegalEntity), nameof(LegalEntity.Code)], nameof(PositionDetailDto.LegalEntityCode))]
    [MapProperty([nameof(Domain.Positions.Position.LegalEntity), nameof(LegalEntity.LegalName)], nameof(PositionDetailDto.LegalEntityName))]
    [MapPropertyFromSource(nameof(PositionDetailDto.DirectReports), Use = nameof(MapDirectReports))]
    [MapPropertyFromSource(nameof(PositionDetailDto.CreatedOnUtc), Use = nameof(GetCreatedOnUtc))]
    [MapPropertyFromSource(nameof(PositionDetailDto.LastModifiedOnUtc), Use = nameof(GetLastModifiedOnUtc))]
    public static partial PositionDetailDto ToDetailDto(this Domain.Positions.Position entity);

    [MapProperty(nameof(Domain.Positions.Position.ReferenceId), nameof(PositionNodeDto.RefId))]
    [MapProperty([nameof(Domain.Positions.Position.JobGrade), nameof(JobGrade.Code)], nameof(PositionNodeDto.JobGradeCode))]
    [MapProperty([nameof(Domain.Positions.Position.JobGrade), nameof(JobGrade.RankOrder)], nameof(PositionNodeDto.JobGradeRankOrder))]
    public static partial PositionNodeDto ToNodeDto(this Domain.Positions.Position entity);

    [MapProperty(nameof(Job.ReferenceId), nameof(PositionJobDto.RefId))]
    public static partial PositionJobDto ToJobDto(this Job job);

    [MapProperty(nameof(JobGrade.ReferenceId), nameof(PositionJobGradeDto.RefId))]
    public static partial PositionJobGradeDto ToJobGradeDto(this JobGrade grade);

    [MapProperty(nameof(CostCenter.ReferenceId), nameof(PositionCostCenterDto.RefId))]
    public static partial PositionCostCenterDto ToCostCenterDto(this CostCenter costCenter);

    private static DateTimeOffset GetCreatedOnUtc(Domain.Positions.Position entity) => entity.CreatedOnUtc;
    private static DateTimeOffset? GetLastModifiedOnUtc(Domain.Positions.Position entity) => entity.LastModifiedOnUtc;

    [SuppressMessage("Performance", "CA1859:Use concrete types when possible for improved performance")]
    private static IReadOnlyList<PositionNodeDto> MapDirectReports(Domain.Positions.Position entity) =>
        entity.DirectReports
            .Where(p => !p.IsDeleted)
            .OrderByDescending(p => p.JobGrade?.RankOrder ?? 0)
            .ThenBy(p => p.Code)
            .Select(p => p.ToNodeDto())
            .ToList();
}