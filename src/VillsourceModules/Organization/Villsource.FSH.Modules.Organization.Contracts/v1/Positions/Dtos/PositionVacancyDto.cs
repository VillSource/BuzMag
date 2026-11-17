namespace Villsource.FSH.Modules.Organization.Contracts.v1.Positions.Dtos;

public sealed record PositionVacancyDto(
    string PositionRefId,
    string PositionCode,
    string Title,
    string? OrgUnitRefId,
    int HeadcountCapacity,
    int FilledCount,
    int VacancyCount,
    bool IsVacant,
    bool IsActive);
