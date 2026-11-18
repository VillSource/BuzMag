using Villsource.FSH.Modules.Organization.Contracts.Constants;
using Villsource.FSH.Modules.Organization.Contracts.Services.Dtos;

namespace Villsource.FSH.Modules.Organization.Services.ChainOfCommand;

internal sealed record PositionProjection(
    Guid Id,
    Guid LegalEntityId,
    Guid OrgUnitId,
    Guid? ReportsToPositionId,
    Guid? DottedLinePositionId,
    ChainOfCommandPositionDto Dto);

internal sealed record OuRow(
    Guid Id,
    string ReferenceId,
    string Code,
    string Name,
    string InternationalName,
    int Depth,
    string HierarchyPath,
    bool IsActive,
    Guid? ManagerPositionId);

internal sealed record ManagerCandidate(
    ManagerResolutionSource Source,
    PositionProjection Position,
    string? ResolvedFromOrgUnitRefId);

internal static class ChainOfCommandPredicates
{
    public static DateTimeOffset NormalizeUtc(DateTimeOffset? asOfUtc) =>
        (asOfUtc ?? DateTimeOffset.UtcNow).ToUniversalTime();

    public static bool IsPositionEffective(
        ChainOfCommandPositionDto dto,
        DateTimeOffset targetUtc,
        bool includeInactive = false)
    {
        if (includeInactive)
        {
            return true;
        }

        if (!dto.IsActive)
        {
            return false;
        }

        if (dto.ValidFrom.HasValue && targetUtc < dto.ValidFrom.Value)
        {
            return false;
        }

        if (dto.ValidTo.HasValue && targetUtc > dto.ValidTo.Value)
        {
            return false;
        }

        return true;
    }

    public static bool MatchesApprovalLimit(
        ChainOfCommandPositionDto dto,
        decimal requiredAmount,
        string normalizedCurrency) =>
        dto.ApprovalLimitAmount.HasValue &&
        dto.ApprovalLimitAmount.Value >= requiredAmount &&
        string.Equals(dto.ApprovalLimitCurrency, normalizedCurrency, StringComparison.OrdinalIgnoreCase);

    public static bool MatchesGradeCriteria(
        ChainOfCommandPositionDto dto,
        int? minRankOrder,
        JobLevelCategory? requiredCategory)
    {
        if (minRankOrder.HasValue && dto.JobGradeRankOrder < minRankOrder.Value)
        {
            return false;
        }

        if (requiredCategory is not null &&
            (string.IsNullOrWhiteSpace(dto.JobLevelCategory) ||
             !JobLevelCategory.TryGet(dto.JobLevelCategory, out var posCategory) ||
             posCategory.SortOrder < requiredCategory.SortOrder))
        {
            return false;
        }

        return true;
    }
}
