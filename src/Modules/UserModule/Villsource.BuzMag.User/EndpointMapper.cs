using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Villsource.BuzMag.Sdk;

namespace Villsource.BuzMag.User;

public class EndpointMapper : IEndpointMapper
{
    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/greeting", () => "hello");
    }
}