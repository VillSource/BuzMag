using Villsource.BuzMag.Infrastructure.ModuleLoader;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var app = builder.Build();

app.UseStaticFiles();
app.UseDefaultFiles();
app.MapFallbackToFile("index.html");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    app.MapGet("/", () => Results.Redirect("/scalar")).ExcludeFromDescription();
}
app.UseHttpsRedirection();

app.LoadModules(options =>
    options.ModulePath = "/home/pandora/Desktop/BuzMag/src/Modules/Villsource.BuzMag.User/bin/Release/net10.0/Villsource.BuzMag.User//");

app.Run();
