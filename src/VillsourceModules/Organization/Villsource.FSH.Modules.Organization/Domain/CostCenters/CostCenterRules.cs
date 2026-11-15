using System.Text.RegularExpressions;

namespace Villsource.FSH.Modules.Organization.Domain.CostCenters;

internal static partial class CostCenterRules
{
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
                $"Cost center code '{normalized}' must be 2-50 uppercase alphanumeric characters, hyphens, or underscores.",
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
}