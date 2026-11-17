using Thinktecture;

namespace Villsource.FSH.Modules.Organization.Domain.PositionDelegations;

[SmartEnum<string>]
public sealed partial class DelegationType
{
    public static readonly DelegationType ActingFull = new(
        key: "ACTING_FULL",
        displayName: "Full Acting Authority",
        requiresDocumentScope: false);

    public static readonly DelegationType ApprovalOnly = new(
        key: "APPROVAL_ONLY",
        displayName: "All Document Approvals",
        requiresDocumentScope: false);

    public static readonly DelegationType SpecificDocuments = new(
        key: "SPECIFIC_DOCUMENTS",
        displayName: "Specific Document Types Only",
        requiresDocumentScope: true);

    public string DisplayName { get; }
    public bool RequiresDocumentScope { get; }

    public static string AllowedKeysDescription =>
        string.Join(", ", Items.Select(i => i.Key));

    public static DelegationType FromKey(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        var normalized = key.Trim().ToUpperInvariant();

        if (!TryGet(normalized, out var result))
        {
            throw new ArgumentException(
                $"Invalid delegation type '{key}'. Allowed values: {AllowedKeysDescription}.",
                nameof(key));
        }

        return result;
    }

    public static bool IsValidKey(string? key) =>
        !string.IsNullOrWhiteSpace(key) && TryGet(key.Trim().ToUpperInvariant(), out _);
}