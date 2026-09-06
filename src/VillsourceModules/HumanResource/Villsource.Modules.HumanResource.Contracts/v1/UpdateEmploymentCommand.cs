using Mediator;
using Villsource.Modules.HumanResource.Contracts.Dtos;

namespace Villsource.Modules.HumanResource.Contracts.v1;

public sealed record UpdateEmploymentCommand(
    string EmployeeRef,
    string Type, 
    string Status, 
    string? Note,
    DateTimeOffset EffectiveFrom
) : ICommand<EmploymentDto>;