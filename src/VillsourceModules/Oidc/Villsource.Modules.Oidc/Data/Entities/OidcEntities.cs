using FSH.Framework.Core.Domain;
using OpenIddict.EntityFrameworkCore.Models;

namespace Villsource.Modules.Oidc.Data.Entities;

public sealed class OidcApplication :
	OpenIddictEntityFrameworkCoreApplication<string, OidcAuthorization, OidcToken>, IGlobalEntity;

public sealed class OidcAuthorization :
	OpenIddictEntityFrameworkCoreAuthorization<string, OidcApplication, OidcToken>, IGlobalEntity;

public sealed class OidcScope : OpenIddictEntityFrameworkCoreScope, IGlobalEntity;

public sealed class OidcToken :
	OpenIddictEntityFrameworkCoreToken<string, OidcApplication, OidcAuthorization>, IGlobalEntity;