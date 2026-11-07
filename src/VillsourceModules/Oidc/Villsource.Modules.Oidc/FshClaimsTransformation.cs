using Microsoft.AspNetCore.Authentication;
using OpenIddict.Abstractions;
using System.Security.Claims;

namespace Villsource.Modules.Oidc;

public sealed class FshClaimsTransformation : IClaimsTransformation
{
    // 1. ใช้ Dictionary (O(1) Lookup) สำหรับ Map สั้น -> ยาว และ ยาว -> สั้น
    private static readonly Dictionary<string, string> ForwardMappings = new(18, StringComparer.Ordinal)
    {
        [OpenIddictConstants.Claims.Subject] = ClaimTypes.NameIdentifier,
        [OpenIddictConstants.Claims.Name] = ClaimTypes.Name,
        [OpenIddictConstants.Claims.GivenName] = ClaimTypes.GivenName,
        [OpenIddictConstants.Claims.FamilyName] = ClaimTypes.Surname,
        [OpenIddictConstants.Claims.Email] = ClaimTypes.Email,
        [OpenIddictConstants.Claims.Role] = ClaimTypes.Role,
        [OpenIddictConstants.Claims.Actor] = ClaimTypes.Actor,
        [OpenIddictConstants.Claims.Birthdate] = ClaimTypes.DateOfBirth,
        [OpenIddictConstants.Claims.Country] = ClaimTypes.Country,
        [OpenIddictConstants.Claims.Gender] = ClaimTypes.Gender,
        [OpenIddictConstants.Claims.Locality] = ClaimTypes.Locality,
        [OpenIddictConstants.Claims.PhoneNumber] = ClaimTypes.MobilePhone,
        [OpenIddictConstants.Claims.PostalCode] = ClaimTypes.PostalCode,
        [OpenIddictConstants.Claims.StreetAddress] = ClaimTypes.StreetAddress,
        [OpenIddictConstants.Claims.Website] = ClaimTypes.Webpage,
        [OpenIddictConstants.Claims.AuthenticationMethodReference] = ClaimTypes.AuthenticationMethod,
        [OpenIddictConstants.Claims.AuthenticationTime] = ClaimTypes.AuthenticationInstant,
        [OpenIddictConstants.Claims.ExpiresAt] = ClaimTypes.Expiration
    };

    private static readonly Dictionary<string, string> ReverseMappings = new(18, StringComparer.Ordinal);

    static FshClaimsTransformation()
    {
        foreach (var kvp in ForwardMappings)
        {
            ReverseMappings[kvp.Value] = kvp.Key;
        }
    }

    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);
        
        if (principal.Identity is not ClaimsIdentity identity || !identity.IsAuthenticated)
        {
            return Task.FromResult(principal);
        }

        // 2. ใช้ HashSet<(string Type, string Value)> ตรวจสอบความซ้ำซ้อนด้วย Hash ในการอ่านครั้งเดียว
        var existingClaims =
            new HashSet<(string Type, string Value)>(identity.Claims.Count(), ClaimPairComparer.Instance);

        // snapshot claims ปัจจุบันเพื่อหลีกเลี่ยงการโดน Exception เมื่อทำการ AddClaim ระหว่างวนลูป
        var originalClaims = identity.Claims.ToArray();

        for (int i = 0; i < originalClaims.Length; i++)
        {
            var claim = originalClaims[i];
            existingClaims.Add((claim.Type, claim.Value));
        }

        // 3. วนลูปเฉพาะ Claim ที่ส่งมาจริงใน Token (O(N) ตามจำนวน Claim ใน Token ไม่ใช่จำนวน Mappings)
        for (int i = 0; i < originalClaims.Length; i++)
        {
            var claim = originalClaims[i];

            // เช็ก ทิศทาง 1: OIDC (สั้น) -> System (ยาว)
            if (ForwardMappings.TryGetValue(claim.Type, out var targetSystemType))
            {
                if (existingClaims.Add((targetSystemType, claim.Value)))
                {
                    identity.AddClaim(new Claim(targetSystemType, claim.Value, claim.ValueType, claim.Issuer,
                        claim.OriginalIssuer));
                }
            }
            // เช็ก ทิศทาง 2: System (ยาว) -> OIDC (สั้น)
            else if (ReverseMappings.TryGetValue(claim.Type, out var targetOidcType) && existingClaims.Add((targetOidcType, claim.Value)))
            {
                identity.AddClaim(new Claim(targetOidcType, claim.Value, claim.ValueType, claim.Issuer,
                    claim.OriginalIssuer));
            }
        }

        return Task.FromResult(principal);
    }

    // High-performance EqualityComparer; No Alloc Struct/Boxing
    private sealed class ClaimPairComparer : IEqualityComparer<(string Type, string Value)>
    {
        public static readonly ClaimPairComparer Instance = new();

        public bool Equals((string Type, string Value) x, (string Type, string Value) y)
        {
            return string.Equals(x.Type, y.Type, StringComparison.Ordinal) &&
                   string.Equals(x.Value, y.Value, StringComparison.Ordinal);
        }

        public int GetHashCode((string Type, string Value) obj)
        {
            return HashCode.Combine(
                StringComparer.Ordinal.GetHashCode(obj.Type),
                StringComparer.Ordinal.GetHashCode(obj.Value));
        }
    }
}