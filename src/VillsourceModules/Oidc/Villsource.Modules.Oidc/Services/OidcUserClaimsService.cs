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
        string tenantId,
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

            return BuildIdentity(result.Value.Subject, tenantId, result.Value.Claims);
        }
        catch (UnauthorizedException ex)
        {
            logger.LogWarning(ex, "OIDC login failed for {Email} in tenant {TenantId}: {Reason}", email, tenantId, ex.Message);
            return null;
        }
        catch (CustomException ex)
        {
            logger.LogWarning(ex, "OIDC login blocked for {Email} in tenant {TenantId}: {Reason}", email, tenantId, ex.Message);
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

        var result = await identityService
            .BuildClaimsForUserAsync(subject, tenantId, ct)
            .ConfigureAwait(false);

        if (result is null)
        {
            return null;
        }

        return BuildIdentity(result.Value.Subject, tenantId, result.Value.Claims);
    }

    // ---------------------------------------------------------------------------
    // Private helpers
    // ---------------------------------------------------------------------------

    private static ClaimsIdentity BuildIdentity(
        string subject,
        string tenantId,
        IEnumerable<Claim> fshClaims)
    {
        var identity = new ClaimsIdentity(
            authenticationType: AuthenticationType,
            nameType: OpenIddictConstants.Claims.Name,
            roleType: OpenIddictConstants.Claims.Role);

        // Subject — always required by OIDC Core.
        var subClaim = new Claim(OpenIddictConstants.Claims.Subject, subject);
        subClaim.SetDestinations(OpenIddictConstants.Destinations.AccessToken, OpenIddictConstants.Destinations.IdentityToken);
        identity.AddClaim(subClaim);

        // Map FSH claims → OIDC claim names & destinations.
        foreach (var claim in fshClaims)
        {
            MapAndAddClaim(identity, claim);
        }

        // Ensure subject is also duplicated as NameIdentifier for compatibility.
        if (identity.FindFirst(ClaimTypes.NameIdentifier) is null)
        {
            var nameIdClaim = new Claim(ClaimTypes.NameIdentifier, subject);
            nameIdClaim.SetDestinations(OpenIddictConstants.Destinations.AccessToken, OpenIddictConstants.Destinations.IdentityToken);
            identity.AddClaim(nameIdClaim);
        }

        // Tenant claim — AccessToken only (not exposed in ID-token).
        if (identity.FindFirst("tenant") is null)
        {
            var tenantClaim = new Claim("tenant", tenantId);
            tenantClaim.SetDestinations(OpenIddictConstants.Destinations.AccessToken);
            identity.AddClaim(tenantClaim);
        }

        return identity;
    }

    private static void MapAndAddClaim(ClaimsIdentity identity, Claim claim)
    {
        switch (claim.Type)
        {
            // Sub / NameIdentifier — already set above as Subject; skip duplicate.
            case ClaimTypes.NameIdentifier:
                break;

            // Full name → OIDC name claim.
            case ClaimConstants.Fullname:
                var nameClaim = new Claim(OpenIddictConstants.Claims.Name, claim.Value);
                nameClaim.SetDestinations(OpenIddictConstants.Destinations.AccessToken, OpenIddictConstants.Destinations.IdentityToken);
                identity.AddClaim(nameClaim);
                break;

            // Email.
            case ClaimTypes.Email:
                var emailClaim = new Claim(OpenIddictConstants.Claims.Email, claim.Value);
                emailClaim.SetDestinations(OpenIddictConstants.Destinations.AccessToken, OpenIddictConstants.Destinations.IdentityToken);
                identity.AddClaim(emailClaim);

                var verifiedClaim = new Claim(OpenIddictConstants.Claims.EmailVerified, "true", ClaimValueTypes.Boolean);
                verifiedClaim.SetDestinations(OpenIddictConstants.Destinations.IdentityToken);
                identity.AddClaim(verifiedClaim);
                break;

            // Roles — access-token only (ID-token stays small).
            case ClaimTypes.Role:
                var roleClaim = new Claim(OpenIddictConstants.Claims.Role, claim.Value);
                roleClaim.SetDestinations(OpenIddictConstants.Destinations.AccessToken);
                identity.AddClaim(roleClaim);
                break;

            // Tenant — access-token only.
            case ClaimConstants.Tenant:
                var tenantClaim = new Claim("tenant", claim.Value);
                tenantClaim.SetDestinations(OpenIddictConstants.Destinations.AccessToken);
                identity.AddClaim(tenantClaim);
                break;

            // Phone.
            case ClaimTypes.MobilePhone:
                if (!string.IsNullOrEmpty(claim.Value))
                {
                    var phoneClaim = new Claim(OpenIddictConstants.Claims.PhoneNumber, claim.Value);
                    phoneClaim.SetDestinations(OpenIddictConstants.Destinations.AccessToken, OpenIddictConstants.Destinations.IdentityToken);
                    identity.AddClaim(phoneClaim);
                }
                break;

            // Given name.
            case ClaimTypes.Name:
                if (!string.IsNullOrEmpty(claim.Value))
                {
                    var givenNameClaim = new Claim(OpenIddictConstants.Claims.GivenName, claim.Value);
                    givenNameClaim.SetDestinations(OpenIddictConstants.Destinations.AccessToken, OpenIddictConstants.Destinations.IdentityToken);
                    identity.AddClaim(givenNameClaim);
                }
                break;

            // Family name.
            case ClaimTypes.Surname:
                if (!string.IsNullOrEmpty(claim.Value))
                {
                    var surnameClaim = new Claim(OpenIddictConstants.Claims.FamilyName, claim.Value);
                    surnameClaim.SetDestinations(OpenIddictConstants.Destinations.AccessToken, OpenIddictConstants.Destinations.IdentityToken);
                    identity.AddClaim(surnameClaim);
                }
                break;

            // Ignore FSH-internal claims that have no OIDC equivalent.
            default:
                break;
        }
    }
}
