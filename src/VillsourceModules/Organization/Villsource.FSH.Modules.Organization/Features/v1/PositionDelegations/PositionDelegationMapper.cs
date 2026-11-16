using Riok.Mapperly.Abstractions;
using Villsource.FSH.Modules.Organization.Contracts.v1.PositionDelegations;
using Villsource.FSH.Modules.Organization.Domain.JobGrades;
using Villsource.FSH.Modules.Organization.Domain.LegalEntities;
using Villsource.FSH.Modules.Organization.Domain.PositionDelegations;
using Villsource.FSH.Modules.Organization.Domain.v2.Positions;

namespace Villsource.FSH.Modules.Organization.Features.v1.PositionDelegations;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public static partial class PositionDelegationMapper
{
    [MapProperty(nameof(PositionDelegation.ReferenceId), nameof(PositionDelegationDetailDto.RefId))]
    [MapProperty([nameof(PositionDelegation.LegalEntity), nameof(LegalEntity.ReferenceId)], nameof(PositionDelegationDetailDto.LegalEntityRefId))]
    [MapProperty([nameof(PositionDelegation.LegalEntity), nameof(LegalEntity.LegalName)], nameof(PositionDelegationDetailDto.LegalEntityName))]
    [MapPropertyFromSource(nameof(PositionDelegationDetailDto.CreatedOnUtc), Use = nameof(GetCreatedOnUtc))]
    [MapPropertyFromSource(nameof(PositionDelegationDetailDto.LastModifiedOnUtc), Use = nameof(GetLastModifiedOnUtc))]
    public static partial PositionDelegationDetailDto ToDetailDto(this PositionDelegation entity);

    [MapProperty(nameof(Position.ReferenceId), nameof(DelegationPositionDto.RefId))]
    [MapProperty([nameof(Position.JobGrade), nameof(JobGrade.Code)], nameof(DelegationPositionDto.JobGradeCode))]
    [MapProperty([nameof(Position.JobGrade), nameof(JobGrade.RankOrder)], nameof(DelegationPositionDto.JobGradeRankOrder))]
    public static partial DelegationPositionDto ToPositionDto(this Villsource.FSH.Modules.Organization.Domain.v2.Positions.Position position);

    [MapPropertyFromSource(nameof(DelegationTypeDto.Key), Use = nameof(GetEnumKey))]
    public static partial DelegationTypeDto ToDto(this DelegationType delegationType);

    private static string GetEnumKey(DelegationType delegationType) => delegationType.Key;
    private static DateTimeOffset GetCreatedOnUtc(PositionDelegation entity) => entity.CreatedOnUtc;
    private static DateTimeOffset? GetLastModifiedOnUtc(PositionDelegation entity) => entity.LastModifiedOnUtc;
}