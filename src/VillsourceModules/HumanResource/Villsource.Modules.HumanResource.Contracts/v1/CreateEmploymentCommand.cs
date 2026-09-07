using Mediator;
using Villsource.Modules.HumanResource.Contracts.Dtos;

namespace Villsource.Modules.HumanResource.Contracts.v1;

public sealed record CreateEmploymentCommand(
    string EmployeeRef,
    string Type, 
    string? Note,
    DateTimeOffset EffectiveDate
) : ICommand<EmploymentDto>;