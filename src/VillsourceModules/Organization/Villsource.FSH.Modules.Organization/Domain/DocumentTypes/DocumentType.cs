using FSH.Framework.Core.Domain;

namespace Villsource.FSH.Modules.Organization.Domain.DocumentTypes;

/// <summary>
/// System-managed document type metadata registered by modules via <c>DocumentTypeConstants.Register</c>.
/// Read-only to end users.
/// </summary>
public sealed class DocumentType : BaseEntity<Guid>, IGlobalEntity
{
    private DocumentType() { }

    public string Code { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string ModuleName { get; private set; } = null!;
    public string? Description { get; private set; }

    public static DocumentType Create(string code, string name, string moduleName, string? description = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(moduleName);

        return new DocumentType
        {
            Id = Guid.CreateVersion7(),
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            ModuleName = moduleName.Trim(),
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim()
        };
    }

    public bool SyncMetadata(string name, string moduleName, string? description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(moduleName);

        var normalizedName = name.Trim();
        var normalizedModule = moduleName.Trim();
        var normalizedDesc = string.IsNullOrWhiteSpace(description) ? null : description.Trim();

        if (string.Equals(Name, normalizedName, StringComparison.Ordinal) &&
            string.Equals(ModuleName, normalizedModule, StringComparison.Ordinal) &&
            string.Equals(Description, normalizedDesc, StringComparison.Ordinal))
        {
            return false;
        }

        Name = normalizedName;
        ModuleName = normalizedModule;
        Description = normalizedDesc;
        return true;
    }
}
