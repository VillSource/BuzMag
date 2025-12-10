using Microsoft.AspNetCore.Routing;

namespace Villsource.BuzMag.Sdk;

public interface IEndpointMapper
{
    void MapEndpoints(IEndpointRouteBuilder endpoints);
}