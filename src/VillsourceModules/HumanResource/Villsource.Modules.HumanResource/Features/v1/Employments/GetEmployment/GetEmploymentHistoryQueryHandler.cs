using Mediator;
using Microsoft.EntityFrameworkCore;
using Villsource.Modules.HumanResource.Contracts.Constants;
using Villsource.Modules.HumanResource.Contracts.Dtos;
using Villsource.Modules.HumanResource.Contracts.v1;
using Villsource.Modules.HumanResource.Data;
using Villsource.Modules.HumanResource.Domain;
using Villsource.Modules.HumanResource.Mappers;

namespace Villsource.Modules.HumanResource.Features.v1.Employments.GetEmployment;

public class GetEmploymentHistoryQueryHandler(HumanResourceDbContext dbContext)
    : IQueryHandler<GetEmploymentHistoryQuery, ICollection<EmploymentCycleDto>>
{
    public async ValueTask<ICollection<EmploymentCycleDto>> Handle(GetEmploymentHistoryQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        
        var rawEmployments = await dbContext.Employees.AsNoTracking()
            .Where(e => e.Ref == query.EmployeeRef)
            .SelectMany(e => e.Employments)
            .OrderBy(e => e.EffectiveFrom)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (rawEmployments.Count == 0)
            return [];
        
        var cycles = rawEmployments
            .GroupBy(e => e.Type)
            .SelectMany(BuildCyclesForType)
            .OrderBy(c => c.HireFrom)
            .ToList();

        return cycles;
    }

    private static IEnumerable<EmploymentCycleDto> BuildCyclesForType(IEnumerable<Employment> typeEmployments)
    {
        var currentCycleItems = new List<EmploymentDto>();

        foreach (var employment in typeEmployments)
        {
            var dto = MapToAuditedDto(employment);
            currentCycleItems.Add(dto);

            if (!IsTerminalStatus(employment.Status))
                continue;

            yield return new EmploymentCycleDto(
                HireFrom: currentCycleItems[0].EffectiveFrom,
                HireEnd: employment.EffectiveTo ?? employment.EffectiveFrom,
                Employments: [.. currentCycleItems]
            );

            currentCycleItems.Clear();
        }

        // Return remaining active cycle if any
        if (currentCycleItems.Count > 0)
        {
            yield return new EmploymentCycleDto(
                HireFrom: currentCycleItems[0].EffectiveFrom,
                HireEnd: currentCycleItems[^1].EffectiveTo,
                Employments: [.. currentCycleItems]
            );
        }
    }

    private static EmploymentDto MapToAuditedDto(Employment employment)
    {
        EmploymentDto dto = employment.ToDto();
        employment.MapAuditableFieldsTo(ref dto);
        return dto;
    }

    private static bool IsTerminalStatus(EmploymentStatus status) =>
        status == EmploymentStatus.Terminated ||
        status == EmploymentStatus.Retired ||
        status == EmploymentStatus.Deceased ||
        status == EmploymentStatus.Resigned;
}