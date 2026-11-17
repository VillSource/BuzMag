using System.Text.RegularExpressions;

namespace Villsource.FSH.Modules.Organization.Domain.JobFamilies;

internal static partial class JobFamilyRules
{
    public const int MaxCodeLength = 50;
    public const int MaxNameLength = 150;
    public const int MaxDescriptionLength = 500;

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
                $"Job family code '{normalized}' must be 2-{MaxCodeLength} uppercase alphanumeric characters, hyphens, or underscores.",
                nameof(code));
        }

        return normalized;
    }

    public static (string Code, string Name, string InternationalName, string? Description)
        NormalizeAndValidate(
            string code,
            string name,
            string internationalName,
            string? description)
    {
        var normalizedCode = NormalizeAndValidateCode(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(internationalName);

        var normalizedDesc = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        return (normalizedCode, name.Trim(), internationalName.Trim(), normalizedDesc);
    }
}
