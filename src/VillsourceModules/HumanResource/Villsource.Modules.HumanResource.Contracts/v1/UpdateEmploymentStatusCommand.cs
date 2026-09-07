using Mediator;
using Villsource.Modules.HumanResource.Contracts.Dtos;

namespace Villsource.Modules.HumanResource.Contracts.v1;

public sealed record UpdateEmploymentStatusCommand(
    string EmployeeRef,
    string Type, 
    string Status, 
    string? Note,
    DateTimeOffset EffectiveDate
) : ICommand<EmploymentDto>;