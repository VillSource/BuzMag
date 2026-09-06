using Mediator;
using Villsource.Modules.HumanResource.Contracts.Dtos;

namespace Villsource.Modules.HumanResource.Contracts.v1;

public sealed record GetCurrentPositionsQuery(string EmployeeRef, bool? IsPrimary = null)
    : IQuery<ICollection<PositionAssignmentDto>>;