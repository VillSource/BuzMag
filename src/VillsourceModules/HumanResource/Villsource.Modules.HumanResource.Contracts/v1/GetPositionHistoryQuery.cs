using Mediator;
using Villsource.Modules.HumanResource.Contracts.Dtos;

namespace Villsource.Modules.HumanResource.Contracts.v1;

public sealed record GetPositionHistoryQuery(string EmployeeRef) : IQuery<ICollection<PositionAssignmentDto>>;