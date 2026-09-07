using Mediator;
using Villsource.Modules.HumanResource.Contracts.Dtos;

namespace Villsource.Modules.HumanResource.Contracts.v1;

public sealed record CreatePositionAssignmentCommand(
    string EmployeeRef,
    string? ManagerRef,
    string OrganizationUnitRef,
    string PositionRef,
    string PositionTier,
    string Type, 
    bool IsPrimary,
    DateTimeOffset EffectiveDate
) : ICommand<PositionAssignmentDto>;