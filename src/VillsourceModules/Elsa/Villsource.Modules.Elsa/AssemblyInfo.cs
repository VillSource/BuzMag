using FSH.Framework.Web.Modules;
using System.Runtime.CompilerServices;
using Villsource.Modules.Elsa;

[assembly: FshModule(typeof(ElsaModule), 900)]
[assembly: InternalsVisibleTo("Villsource.Modules.Elsa.Tests")]
