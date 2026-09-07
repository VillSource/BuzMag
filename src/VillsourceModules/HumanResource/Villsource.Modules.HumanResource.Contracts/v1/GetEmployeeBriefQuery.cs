using Mediator;
using Villsource.Modules.HumanResource.Contracts.Dtos;

namespace Villsource.Modules.HumanResource.Contracts.v1;

public sealed record GetEmployeeBriefQuery(string EmployeeRef) : IQuery<EmployeeBriefDto>;