using FSH.Framework.Core.Exceptions;
using FSH.Framework.Shared.Constants;
using FSH.Modules.Identity.Contracts.Services;
using Microsoft.Extensions.Logging;
using OpenIddict.Abstractions;
using System.Security.Claims;

namespace Villsource.Modules.Oidc.Services;

/// <summary>
/// Bridges the OIDC connect endpoints to the Identity module contracts.
/// Converts FSH claims into OpenIddict-routable claim sets with correct Destinations.
/// </summary>
internal sealed class OidcUserClaimsService(
    IIdentityService identityService,
    ILogger<OidcUserClaimsService> logger) : IOidcUserClaimsService
{
    private const string AuthenticationType = "OpenIdConnect";

    public async Task<ClaimsIdentity?> BuildOidcIdentityAsync(
        string email,
        string password,
        string? twoFactorCode,
        IEnumerable<string> requestedScopes,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(email);
        ArgumentNullException.ThrowIfNull(password);

        try
        {
            var result = await identityService
                .ValidateCredentialsAsync(email, password, twoFactorCode, ct)
                .ConfigureAwait(false);

            if (result is null)
            {
                return null;
            }

            return BuildIdentity(result.Value.Subject, result.Value.Claims, requestedScopes);
        }
        catch (UnauthorizedException ex)
        {
            logger.LogWarning(ex, "OIDC login failed for {Email}: {Reason}", email, ex.Message);
            return null;
        }
        catch (CustomException ex)
        {
            logger.LogWarning(ex, "OIDC login blocked for {Email}: {Reason}", email, ex.Message);
            return null;
        }
    }

    public async Task<ClaimsIdentity?> BuildOidcIdentityForSubjectAsync(
        string subject,
        string tenantId,
        IEnumerable<string> requestedScopes,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(subject);
        ArgumentNullException.ThrowIfNull(tenantId);

        try
        {
            var result = await identityService
                .BuildClaimsForUserAsync(subject, tenantId, ct)
                .ConfigureAwait(false);

            if (result is null)
            {
                return null;
            }

            return BuildIdentity(result.Value.Subject, result.Value.Claims, requestedScopes);
        }
        catch (UnauthorizedException ex)
        {
            logger.LogWarning(ex, "OIDC claims refresh rejected for subject {Subject} in tenant {TenantId}", subject, tenantId);
            return null;
        }
    }

    // ---------------------------------------------------------------------------
    // Private helpers
    // ---------------------------------------------------------------------------

    private static ClaimsIdentity BuildIdentity(
        string subject,
        IEnumerable<Claim> fshClaims,
        IEnumerable<string> requestedScopes)
    {
        var identity = new ClaimsIdentity(
            authenticationType: AuthenticationType,
            nameType: OpenIddictConstants.Claims.Name,
            roleType: OpenIddictConstants.Claims.Role);

        HashSet<string> scopes = requestedScopes.ToHashSet(StringComparer.Ordinal);

        var subClaim = new Claim(OpenIddictConstants.Claims.Subject, subject);
        subClaim.SetDestinations(OpenIddictConstants.Destinations.AccessToken, OpenIddictConstants.Destinations.IdentityToken);
        identity.AddClaim(subClaim);


#if DEBUG
        // MOCK PERMISSION
        var permClaim = new Claim("permissions", "*");
        permClaim.SetDestinations(OpenIddictConstants.Destinations.AccessToken, OpenIddictConstants.Destinations.IdentityToken);
        identity.AddClaim(permClaim);
#endif
        
        var nameIdentifierClaim = new Claim(ClaimTypes.NameIdentifier, subject);
        nameIdentifierClaim.SetDestinations(OpenIddictConstants.Destinations.IdentityToken);
        identity.AddClaim(nameIdentifierClaim);

        foreach (Claim claim in fshClaims)
        {
            switch (claim.Type)
            {
                case ClaimConstants.Tenant:
                    AddClaim(identity, claim.Type, claim.Value, OpenIddictConstants.Destinations.AccessToken);
                    break;

                case ClaimConstants.Fullname when scopes.Contains(OpenIddictConstants.Scopes.Profile):
                    AddClaim(identity, OpenIddictConstants.Claims.Name, claim.Value,
                        OpenIddictConstants.Destinations.AccessToken, OpenIddictConstants.Destinations.IdentityToken);
                    AddClaim(identity, ClaimConstants.Fullname, claim.Value, OpenIddictConstants.Destinations.AccessToken);
                    break;

                case ClaimTypes.Email when scopes.Contains(OpenIddictConstants.Scopes.Email):
                    AddClaim(identity, OpenIddictConstants.Claims.Email, claim.Value,
                        OpenIddictConstants.Destinations.AccessToken, OpenIddictConstants.Destinations.IdentityToken);
                    AddClaim(identity, OpenIddictConstants.Claims.EmailVerified, "true",
                        OpenIddictConstants.Destinations.IdentityToken);
                    break;

                case ClaimTypes.Name when scopes.Contains(OpenIddictConstants.Scopes.Profile):
                    AddClaim(identity, OpenIddictConstants.Claims.GivenName, claim.Value,
                        OpenIddictConstants.Destinations.AccessToken, OpenIddictConstants.Destinations.IdentityToken);
                    break;

                case ClaimTypes.Surname when scopes.Contains(OpenIddictConstants.Scopes.Profile):
                    AddClaim(identity, OpenIddictConstants.Claims.FamilyName, claim.Value,
                        OpenIddictConstants.Destinations.AccessToken, OpenIddictConstants.Destinations.IdentityToken);
                    break;

                case ClaimTypes.MobilePhone when scopes.Contains(OpenIddictConstants.Scopes.Profile):
                    AddClaim(identity, OpenIddictConstants.Claims.PhoneNumber, claim.Value,
                        OpenIddictConstants.Destinations.AccessToken, OpenIddictConstants.Destinations.IdentityToken);
                    break;

                case ClaimTypes.Role when scopes.Contains("roles"):
                    AddClaim(identity, OpenIddictConstants.Claims.Role, claim.Value,
                        OpenIddictConstants.Destinations.AccessToken);
                    break;
            }
        }

        return identity;
    }

    private static void AddClaim(
        ClaimsIdentity identity,
        string type,
        string value,
        params string[] destinations)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        Claim mappedClaim = new(type, value);
        mappedClaim.SetDestinations(destinations);
        identity.AddClaim(mappedClaim);
    }
}
