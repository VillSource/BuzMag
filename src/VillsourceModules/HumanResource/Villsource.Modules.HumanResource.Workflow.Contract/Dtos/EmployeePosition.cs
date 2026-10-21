namespace Villsource.Modules.HumanResource.Workflow.Contract.Dtos;

public record EmployeePosition(
    string OrganizationUnitRef,
    string PositionRef,
    string PositionTierName,
    int PositionTier);