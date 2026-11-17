using System.Text.RegularExpressions;

namespace Villsource.FSH.Modules.Organization.Domain.OrganizationUnits;

internal static partial class OrganizationUnitRules
{
    [GeneratedRegex(@"^[A-Z0-9\-_]{2,50}$", RegexOptions.CultureInvariant)]
    internal static partial Regex CodeRegex();

    [GeneratedRegex(@"^[A-Z0-9_]{2,30}$", RegexOptions.CultureInvariant)]
    internal static partial Regex TypeRegex();
}