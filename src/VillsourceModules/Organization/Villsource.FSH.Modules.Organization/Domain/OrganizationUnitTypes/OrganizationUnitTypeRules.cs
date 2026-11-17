using System.Text.RegularExpressions;

namespace Villsource.FSH.Modules.Organization.Domain.OrganizationUnitTypes;

internal static partial class OrganizationUnitTypeRules
{
    public const int MaxCodeLength = 30;
    public const int MaxNameLength = 150;
    public const int MaxDescriptionLength = 500;
    public const int MinHierarchyLevel = 0;

    [GeneratedRegex(@"^[A-Z0-9\-_]{1,30}$", RegexOptions.CultureInvariant)]
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
                $"Organization unit type code '{normalized}' must be 1-{MaxCodeLength} uppercase alphanumeric characters, hyphens, or underscores.",
                nameof(code));
        }

        return normalized;
    }

    public static (string Code, string Name, string InternationalName, int HierarchyLevel, string? Description)
        NormalizeAndValidate(
            string code,
            string name,
            string internationalName,
            int hierarchyLevel,
            string? description)
    {
        var normalizedCode = NormalizeAndValidateCode(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(internationalName);

        if (hierarchyLevel < MinHierarchyLevel)
        {
            throw new ArgumentOutOfRangeException(
                nameof(hierarchyLevel),
                $"HierarchyLevel must be greater than or equal to {MinHierarchyLevel}.");
        }

        var normalizedDesc = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        return (normalizedCode, name.Trim(), internationalName.Trim(), hierarchyLevel, normalizedDesc);
    }
}
