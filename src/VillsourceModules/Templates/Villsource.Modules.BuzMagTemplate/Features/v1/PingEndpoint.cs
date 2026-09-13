using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Villsource.Modules.BuzMagTemplate.Contracts.v1;

namespace Villsource.Modules.BuzMagTemplate.Features.v1;

public static class PingEndpoint
{
    public static void MapPingEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("ping", async (IMediator mediator) =>
        {
            var res = await mediator.Send(new PingCommand()).ConfigureAwait(false);
            return res;
        })
        .AllowAnonymous()
        .WithName("Ping Endpoint");
    }
}