using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;

namespace Villsource.Modules.Oidc;

public class OidcStaticFileStartupFilter(string assetsPath, string path) : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        return app =>
        {
            
            var fileProvider = new PhysicalFileProvider(assetsPath);
            var requestPath = path;

            var defaultFilesOptions = new DefaultFilesOptions
            {
                FileProvider = fileProvider,
                RequestPath = requestPath
            };
            app.UseDefaultFiles(defaultFilesOptions);

            var staticFileOptions = new StaticFileOptions
            {
                FileProvider = fileProvider,
                RequestPath = requestPath
            };
            app.UseStaticFiles(staticFileOptions);

            next(app);
        };
    }
}