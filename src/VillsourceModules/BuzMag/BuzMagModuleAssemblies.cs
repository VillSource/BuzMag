using System.Reflection;
using Villsource.FSH.Modules.Organization;
using Villsource.FSH.Modules.Organization.Contracts;

namespace BuzMag;

public static class BuzMagModuleAssemblies
{
    public static IReadOnlyList<Assembly> RuntimeAssemblies { get; } =
    [
        typeof(OrganizationModule).Assembly
    ];
    
    public static IReadOnlyList<Assembly> ContractAssemblies { get; } =
    [
        typeof(OrganizationContractsMarker).Assembly
    ];
}