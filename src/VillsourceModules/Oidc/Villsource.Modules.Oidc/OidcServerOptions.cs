namespace Villsource.Modules.Oidc;

internal sealed class OidcServerOptions
{
    public const string SectionName = "Oidc";

    public bool UseDevelopmentCertificates { get; set; }
    public string SigningCertificatePath { get; set; } = string.Empty;
    public string? SigningCertificatePassword { get; set; }
}
