using Thinktecture;

namespace Villsource.FSH.Modules.Organization.Contracts.Constants;

[SmartEnum<string>]
public sealed partial class JobLevelCategory
{
    public static readonly JobLevelCategory Operational = new(
        key: "OPERATIONAL",
        displayName: "Operational",
        sortOrder: 1);

    public static readonly JobLevelCategory Supervisory = new(
        key: "SUPERVISORY",
        displayName: "Supervisory",
        sortOrder: 2);

    public static readonly JobLevelCategory Management = new(
        key: "MANAGEMENT",
        displayName: "Management",
        sortOrder: 3);

    public static readonly JobLevelCategory Executive = new(
        key: "EXECUTIVE",
        displayName: "Executive",
        sortOrder: 4);

    public string DisplayName { get; }
    public int SortOrder { get; }

    public static string AllowedKeysDescription =>
        string.Join(", ", Items.Select(i => i.Key));

    public static JobLevelCategory FromKey(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        var normalized = key.Trim().ToUpperInvariant();

        if (!TryGet(normalized, out var result))
        {
            throw new ArgumentException(
                $"Invalid job level category '{key}'. Allowed values: {AllowedKeysDescription}.",
                nameof(key));
        }

        return result;
    }

    public static bool IsValidKey(string? key) =>
        !string.IsNullOrWhiteSpace(key) && TryGet(key.Trim().ToUpperInvariant(), out _);
}
