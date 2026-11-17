using System.Text.RegularExpressions;

namespace Villsource.FSH.Modules.Organization.Domain.Jobs;

internal static partial class JobRules
{
    public const int MaxCodeLength = 50;
    public const int MaxNameLength = 200;
    public const int MaxDescriptionLength = 1000;

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
                $"Job code '{normalized}' must be 2-{MaxCodeLength} uppercase alphanumeric characters, hyphens, or underscores.",
                nameof(code));
        }

        return normalized;
    }

    public static (string Code, string Name, string InternationalName, string? Description)
        NormalizeAndValidate(
            string code,
            string name,
            string internationalName,
            Guid defaultJobGradeId,
            string? description)
    {
        if (defaultJobGradeId == Guid.Empty)
        {
            throw new ArgumentException("DefaultJobGradeId cannot be empty.", nameof(defaultJobGradeId));
        }

        var normalizedCode = NormalizeAndValidateCode(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(internationalName);

        var normalizedDesc = string.IsNullOrWhiteSpace(description) ? null : description.Trim();

        return (normalizedCode, name.Trim(), internationalName.Trim(), normalizedDesc);
    }
}