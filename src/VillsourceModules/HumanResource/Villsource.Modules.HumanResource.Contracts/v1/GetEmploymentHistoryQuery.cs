using Mediator;
using Villsource.Modules.HumanResource.Contracts.Dtos;

namespace Villsource.Modules.HumanResource.Contracts.v1;

public sealed record EmploymentCycleDto(
    DateTimeOffset HireFrom,
    DateTimeOffset? HireEnd,
    IReadOnlyList<EmploymentDto> Employments
);
public sealed record GetEmploymentHistoryQuery(string EmployeeRef) : IQuery<ICollection<EmploymentCycleDto>>;