using System.Runtime.CompilerServices;
using FSH.Framework.Web.Modules;
using Villsource.Modules.Oidc;

[assembly: FshModule(typeof(OidcModule), 150)]
[assembly: InternalsVisibleTo("Oidc.Tests")]