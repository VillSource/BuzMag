using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;

namespace Villsource.BuzMag.Infrastructure.ModuleLoader;

public static class Extension
{
    public static void LoadModules(this WebApplication endpoints, Action<LoadModulesOptions> configure)
    {
        LoadModulesOptions options = new LoadModulesOptions();
        configure(options);
        endpoints.LoadModules(options);
    }

    public static void LoadModules(this WebApplication endpoints, LoadModulesOptions options)
    {
        var moduleContext = new ModuleRegister(options.ModulePath);
        var moduleGroupEndpoint = endpoints.MapGroup("Modules");

        foreach (var module in moduleContext.Modules)
        {
            var endpointGroup = moduleGroupEndpoint.MapGroup(module.Name.Value).WithTags(module.Name.Value);
            module.EndpointMapper?.MapEndpoints(endpointGroup);

            var spaPath = Path.Combine(options.ModulePath, "wwwroot");
            var staticFileOptions = new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(spaPath),
                RequestPath = $"/Modules/{module.Name.Value}/ui"
            };

            endpoints.UseStaticFiles(staticFileOptions);

            endpoints.Use(async (context, next) =>
            {
                await next();
                if (context.Response.StatusCode == 404 && !Path.HasExtension(context.Request.Path.Value))
                {
                    context.Request.Path = "/index.html";
                    await context.Response.SendFileAsync(
                        new PhysicalFileProvider(spaPath).GetFileInfo("index.html")
                    );
                }
            });
        }
    }
}