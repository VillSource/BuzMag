using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Villsource.BuzMag.Infrastructure.ModuleLoader;

public static class Extension
{
    public static void LoadModules(this IEndpointRouteBuilder endpoints,  Action<LoadModulesOptions> configure)
    {
        LoadModulesOptions options = new LoadModulesOptions();
        configure(options);
        endpoints.LoadModules(options);
    }
    public static void LoadModules(this IEndpointRouteBuilder endpoints, LoadModulesOptions options)
    {
        var m = new ModuleRegister(options.ModulePath);
        var G = endpoints.MapGroup("Modules");

        foreach (var module in m.Modules)
        {
            var g = G.MapGroup(module.Name.Value).WithTags(module.Name.Value);
            module.EndpointMapper?.MapEndpoints(g);
        }
    }
}