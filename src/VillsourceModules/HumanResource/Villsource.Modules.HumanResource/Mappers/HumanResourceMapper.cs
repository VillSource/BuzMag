using FSH.Framework.Core.Domain;
using Villsource.Modules.HumanResource.Contracts.Dtos;
using Villsource.Modules.HumanResource.Contracts.v1;

namespace Villsource.Modules.HumanResource.Mappers;

using Riok.Mapperly.Abstractions;
using Domain;
using Villsource.Modules.HumanResource.Contracts.Constants;
using Villsource.FSH.Modules.Organization.Contracts.Constants;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public static partial class HumanResourceMapper
{
    public static partial EmployeeDto ToDetailDto(this Employee employee);
    public static partial EmployeeBriefDto ToBriefDto(this Employee employee);
    public static partial EmploymentDto ToDto(this Employment employment);
    public static partial PositionAssignmentDto ToDto(this PositionAssignment positionAssignment);

    public static partial ICollection<EmployeeDto> ToDto(this IEnumerable<Employee> employees);
    public static partial ICollection<EmploymentDto> ToDto(this IEnumerable<Employment> employments);
    public static partial ICollection<PositionAssignmentDto> ToDto(this IEnumerable<PositionAssignment> assignments);

    public static partial IQueryable<EmployeeDto> ProjectToDetailDto(this IQueryable<Employee> query);
    public static partial IQueryable<EmployeeBriefDto> ProjectToBriefDto(this IQueryable<Employee> query);
    public static partial IQueryable<EmploymentDto> ProjectToDto(this IQueryable<Employment> query);
    public static partial IQueryable<PositionAssignmentDto> ProjectToDto(this IQueryable<PositionAssignment> query);

    public static void MapAuditFieldsTo<T>(this IAuditableEntity source, ref T target) where T : AuditableDto
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(target);

        target.CreatedBy = source.CreatedBy;
        target.CreatedOnUtc = source.CreatedOnUtc;
        target.LastModifiedBy = source.LastModifiedBy;
        target.LastModifiedOnUtc = source.LastModifiedOnUtc;
    }

    public static void MapSoftDeleteFieldsTo<T>(this ISoftDeletable source, ref T target) where T : AuditableDto
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(target);

        target.DeletedBy = source.DeletedBy;
        target.DeletedOnUtc = source.DeletedOnUtc;
        target.IsDeleted = source.IsDeleted;
    }

    public static void MapAuditableFieldsTo<T, TSource>(this TSource source, ref T target)
        where T : AuditableDto
        where TSource : IAuditableEntity, ISoftDeletable
    {
        source.MapAuditFieldsTo(ref target);
        source.MapSoftDeleteFieldsTo(ref target);
    }

    internal static string MapEmploymentStatusToString(EmploymentStatus? status)
        => status?.Key ?? EmploymentStatus.None.Key;

    internal static string MapEmploymentTypeToString(EmploymentType? type)
        => type?.Key ?? EmploymentType.None.Key;

    internal static string MapPositionAssignmentTypeToString(PositionAssignmentType? type)
        => type?.Key ?? PositionAssignmentType.None.Key;

    internal static string MapPositionTierToString(PositionTier? tier)
        => tier?.Key ?? PositionTier.None.Key;
}