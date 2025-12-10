using System.Reflection;
using System.Runtime.Loader;

namespace Villsource.BuzMag.Infrastructure.ModuleLoader;

public class ModuleLoaderContext : AssemblyLoadContext
{
    private readonly string _modulePath;

    public ModuleLoaderContext(string modulePath)
    {
        _modulePath = modulePath;
    }

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        string assemblyPath = Path.Combine(_modulePath, assemblyName.Name + ".dll");
        if (File.Exists(assemblyPath))
        {
            return LoadFromAssemblyPath(assemblyPath);
        }

        return null; 
    }
}