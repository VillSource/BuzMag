using Mediator;
using Villsource.Modules.HumanResource.Contracts.Dtos;

namespace Villsource.Modules.HumanResource.Contracts.v1;

public sealed record AssignPositionCommand(
    string EmployeeRef,
    string? ManagerRef,
    string OrganizationUnitRef,
    string PositionRef,
    string PositionTier,
    string Type, 
    bool IsPrimary,
    DateTimeOffset EffectiveFrom,
    DateTimeOffset? EffectiveTo
) : ICommand<PositionAssignmentDto>;