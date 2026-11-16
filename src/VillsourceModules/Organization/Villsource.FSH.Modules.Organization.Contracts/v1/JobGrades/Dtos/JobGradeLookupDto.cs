namespace Villsource.FSH.Modules.Organization.Contracts.v1.JobGrades.Dtos;

// =============================================================================
// DTOs
// =============================================================================

/// <summary>
/// Lightweight projection for dropdown selectors and approval matrix configuration.
/// </summary>
public sealed record JobGradeLookupDto(
    string RefId,
    string Code,
    string Name,
    string InternationalName,
    int RankOrder);

// =============================================================================
// Queries
// =============================================================================

// =============================================================================
// Commands
// =============================================================================