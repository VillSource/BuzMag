using Scalar.AspNetCore;
using Villsource.BuzMag.Infrastructure.ModuleLoader;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddPolicy("dev", policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

app.UseStaticFiles();
app.UseDefaultFiles();
app.MapFallbackToFile("index.html");

if (app.Environment.IsDevelopment())
{
    app.UseCors("dev");
    app.MapOpenApi();
    app.MapScalarApiReference();
    app.MapGet("/", () => Results.Redirect("/scalar")).ExcludeFromDescription();
}
app.UseHttpsRedirection();

app.MapGet("federation.manifest.json", (HttpRequest req) => Results.Json(new
{
    user = $"{req.Scheme}://{req.Host.Value}/Modules/Users/ui/remoteEntry.json",
    test="from server"
}));

app.LoadModules(options =>
    options.ModulePath = "/home/pandora/Desktop/BuzMag/src/Modules/UserModule/Villsource.BuzMag.User/bin/Release/net10.0/Villsource.BuzMag.User//");

app.Run();
