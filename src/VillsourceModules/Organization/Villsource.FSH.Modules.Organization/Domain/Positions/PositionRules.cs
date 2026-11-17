using System.Text.RegularExpressions;

namespace Villsource.FSH.Modules.Organization.Domain.Positions;

internal static partial class PositionRules
{
    public const int MaxCodeLength = 50;
    public const int MaxTitleLength = 200;
    public const int MinHeadcountCapacity = 1;

    [GeneratedRegex(@"^[A-Z0-9\-_]{2,50}$", RegexOptions.CultureInvariant)]
    private static partial Regex CodeRegex();

    public static bool IsValidCode(string? code) =>
        !string.IsNullOrWhiteSpace(code) && CodeRegex().IsMatch(code.Trim().ToUpperInvariant());

    public static string NormalizeAndValidateCode(string code)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        var normalized = code.Trim().ToUpperInvariant();

        if (!CodeRegex().IsMatch(normalized))
        {
            throw new ArgumentException(
                $"Position code '{normalized}' must be 2-{MaxCodeLength} uppercase alphanumeric characters, hyphens, or underscores.",
                nameof(code));
        }

        return normalized;
    }

    public static (DateTimeOffset? NormalizedFrom, DateTimeOffset? NormalizedTo) NormalizeAndValidateValidity(
        DateTimeOffset? validFrom,
        DateTimeOffset? validTo)
    {
        var utcFrom = validFrom?.ToUniversalTime();
        var utcTo = validTo?.ToUniversalTime();

        if (utcFrom.HasValue && utcTo.HasValue && utcTo.Value < utcFrom.Value)
        {
            throw new ArgumentException("ValidTo cannot be earlier than ValidFrom.", nameof(validTo));
        }

        return (utcFrom, utcTo);
    }

    public static void ValidateReportingTargets(
        Guid positionId,
        Guid? reportsToPositionId,
        Guid? dottedLinePositionId)
    {
        if (reportsToPositionId.HasValue && reportsToPositionId.Value == positionId)
        {
            throw new InvalidOperationException("A position cannot report to itself.");
        }

        if (dottedLinePositionId.HasValue && dottedLinePositionId.Value == positionId)
        {
            throw new InvalidOperationException("A position cannot have a dotted-line report to itself.");
        }

        if (reportsToPositionId.HasValue &&
            dottedLinePositionId.HasValue &&
            reportsToPositionId.Value == dottedLinePositionId.Value)
        {
            throw new InvalidOperationException(
                "Dotted-line reporting position cannot be the same as the primary reporting position.");
        }
    }
}