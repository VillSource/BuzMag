using System.Reflection;
using Villsource.FSH.Modules.Organization;
using Villsource.FSH.Modules.Organization.Contracts;

namespace BuzMag;

public static class BuzMagModuleAssemblies
{
    public static IReadOnlyCollection<Type> RuntimeType { get; } =
    [
        typeof(OrganizationModule)
    ];

    public static IReadOnlyCollection<Type> ContractType { get; } =
    [
        typeof(OrganizationContractsMarker)
    ];

    public static IReadOnlyCollection<Assembly> RuntimeAssemblies { get; } =
        [.. RuntimeType.Select(t => t.Assembly)];

    public static IReadOnlyCollection<Assembly> ContractAssemblies { get; } =
        [.. ContractType.Select(t => t.Assembly)];
}