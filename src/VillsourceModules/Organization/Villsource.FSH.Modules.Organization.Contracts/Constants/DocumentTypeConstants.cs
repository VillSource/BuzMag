namespace Villsource.FSH.Modules.Organization.Contracts.Constants;

/// <summary>
/// Central document type registry for position delegation scoping.
/// Modules register their document types via <see cref="Register"/> during startup.
/// </summary>
public static class DocumentTypeConstants
{
    private static readonly List<OrgDocumentType> _all = new();

    /// <summary>
    /// Registers document types from a module. Duplicates (by normalized <see cref="OrgDocumentType.Code"/>) are skipped.
    /// </summary>
    public static void Register(IEnumerable<OrgDocumentType> documentTypes)
    {
        ArgumentNullException.ThrowIfNull(documentTypes);

        foreach (var docType in documentTypes)
        {
            ArgumentNullException.ThrowIfNull(docType);
            ArgumentException.ThrowIfNullOrWhiteSpace(docType.Code);
            ArgumentException.ThrowIfNullOrWhiteSpace(docType.Name);
            ArgumentException.ThrowIfNullOrWhiteSpace(docType.ModuleName);

            var normalizedCode = docType.Code.Trim().ToUpperInvariant();
            if (_all.Any(d => string.Equals(d.Code, normalizedCode, StringComparison.Ordinal)))
            {
                continue;
            }

            _all.Add(docType with
            {
                Code = normalizedCode,
                Name = docType.Name.Trim(),
                ModuleName = docType.ModuleName.Trim(),
                Description = string.IsNullOrWhiteSpace(docType.Description) ? null : docType.Description.Trim()
            });
        }
    }

    public static IReadOnlyList<OrgDocumentType> All => _all.AsReadOnly();
}

public sealed record OrgDocumentType(
    string Code,
    string Name,
    string ModuleName,
    string? Description = null);
