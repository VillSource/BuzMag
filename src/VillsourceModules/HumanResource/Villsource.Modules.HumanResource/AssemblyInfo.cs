using FSH.Framework.Web.Modules;
using System.Runtime.CompilerServices;
using Villsource.Modules.HumanResource;

[assembly: FshModule(typeof(HumanResourceModule), 1000)]
[assembly: InternalsVisibleTo("Villsource.Modules.HumanResource.Tests")]
