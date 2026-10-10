using System.Security.Claims;

namespace Villsource.Modules.Oidc.Services;

/// <summary>
/// Builds OpenIddict-compatible <see cref="ClaimsIdentity"/> instances by delegating
/// credential validation and claim construction to the Identity module's contracts.
/// </summary>
internal interface IOidcUserClaimsService
{
    /// <summary>
    /// Validates credentials (including optional 2FA) and returns an OpenIddict
    /// <see cref="ClaimsIdentity"/> with all claims wired to their correct token destinations.
    /// Returns <c>null</c> if credentials are invalid.
    /// </summary>
    Task<ClaimsIdentity?> BuildOidcIdentityAsync(
        string tenantId,
        string email,
        string password,
        string? twoFactorCode,
        IEnumerable<string> requestedScopes,
        CancellationToken ct = default);

    /// <summary>
    /// Builds an OpenIddict <see cref="ClaimsIdentity"/> for an already-authenticated subject
    /// (used in the UserInfo endpoint to return live claim data).
    /// Returns <c>null</c> if the user cannot be found.
    /// </summary>
    Task<ClaimsIdentity?> BuildOidcIdentityForSubjectAsync(
        string subject,
        string tenantId,
        IEnumerable<string> requestedScopes,
        CancellationToken ct = default);
}
