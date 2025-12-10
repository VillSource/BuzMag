using System.Reflection;
using Villsource.BuzMag.Sdk;

namespace Villsource.BuzMag.Infrastructure.ModuleLoader;

public class ModuleRegister
{
    public string ModulePath { get; init; }
    public AssemblyName ModuleName { get; init; }

    public List<IModuleContext> Modules { get; init; }

    public ModuleRegister(string modulePath)
    {
        ModulePath = modulePath.TrimStart().TrimEnd(['/', '\\', ' ']);
        ModuleName = new AssemblyName(Path.GetFileName(ModulePath));

        var moduleLoaderContext = new ModuleLoaderContext(ModulePath);
        var moduleAssembly = moduleLoaderContext.LoadFromAssemblyName(ModuleName);

        var moduleContests = moduleAssembly.GetTypes()
            .Where(t => t is { IsInterface: false, IsAbstract: false, IsClass: true })
            .Where(t => typeof(IModuleContext).IsAssignableFrom(t))
            .Select(t => (IModuleContext)Activator.CreateInstance(t)!);
        Modules = moduleContests.ToList();
    }
}