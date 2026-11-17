namespace Villsource.FSH.Modules.Organization.Contracts.v1.Positions.Dtos;

/// <summary>
/// Lightweight projection for dropdown selectors (reporting manager, org unit head, delegation).
/// </summary>
public sealed record PositionLookupDto(
    string RefId,
    string LegalEntityRefId,
    string? OrgUnitRefId,
    string Code,
    string Title,
    string InternationalTitle,
    string JobGradeCode,
    int JobGradeRankOrder,
    bool IsPrimaryLeadership);