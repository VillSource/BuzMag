using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace Villsource.Modules.Elsa;

public class ElsaPermissionTransformer : IClaimsTransformation
{
    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);
        
        var clone = principal.Clone();
        var identity = clone.Identities.FirstOrDefault();

        if (identity == null || !identity.IsAuthenticated)
            return Task.FromResult(principal);

        if (principal.IsInRole("Admin"))
        {
            identity.AddClaim(new Claim("permissions", "*"));
        }
        
        return Task.FromResult(clone);
    }
}