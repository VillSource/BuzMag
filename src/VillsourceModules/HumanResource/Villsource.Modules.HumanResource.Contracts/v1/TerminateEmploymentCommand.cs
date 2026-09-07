using Mediator;
using Villsource.Modules.HumanResource.Contracts.Dtos;

namespace Villsource.Modules.HumanResource.Contracts.v1;

public sealed record TerminateEmploymentCommand(
    string EmployeeRef,
    DateTimeOffset EffectiveDate,
    string? Note
) : ICommand<EmployeeDto>;