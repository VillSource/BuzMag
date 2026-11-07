using Finbuckle.MultiTenant.Abstractions;
using FSH.Framework.Core.Context;
using FSH.Framework.Shared.Multitenancy;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Villsource.Modules.Oidc.Contracts.v1;

namespace Villsource.Modules.Oidc.Features.v1;

public static class PingEndpoint
{
    public static void MapPingEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("ping", async (IMediator mediator, ICurrentUser usr, IHttpContextAccessor context, IMultiTenantContextAccessor<AppTenantInfo> tenant) =>
            {
                var  user = context.HttpContext?.User;
                _ = user;
                var currentUser = new
                {
                    tenant = usr.GetTenant() ,
                    claims = usr.GetUserClaims() ,
                    isAdmin = usr.IsInRole("admin") ,
                    usr ,
                    t = tenant.MultiTenantContext.TenantInfo
                } ;
                _ = currentUser;
                var res = await mediator.Send(new PingCommand()).ConfigureAwait(false);
                return res;
            })
            .WithName("Ping Endpoint");
    }
}