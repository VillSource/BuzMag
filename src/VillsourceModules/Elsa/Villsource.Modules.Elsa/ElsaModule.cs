using Elsa.Extensions;
using FluentValidation;
using FSH.Framework.Core.Context;
using FSH.Framework.Eventing;
using FSH.Framework.Persistence;
using FSH.Framework.Shared.Constants;
using FSH.Framework.Web.Modules;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using OpenIddict.Validation.AspNetCore;
using Villsource.Modules.Elsa.Contracts.Authorization;
using Villsource.Modules.Elsa.Contracts.Services;
using Villsource.Modules.Elsa.Contracts.v1;
using Villsource.Modules.Elsa.Contracts.v1.Approval;
using Villsource.Modules.Elsa.Data;
using Villsource.Modules.Elsa.Feature.v1;
using Villsource.Modules.Elsa.Services;

namespace Villsource.Modules.Elsa;

public sealed class ElsaModule : IModule
{
    public void ConfigureServices(IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        PermissionConstants.Register(ElsaPermissions.All);

        builder.Services.AddHeroDbContext<ElsaDbContext>();
        builder.Services.AddScoped<IDbInitializer, ElsaDbInitializer>();
        builder.Services.AddValidatorsFromAssembly(typeof(ElsaModule).Assembly);
        builder.Services.AddIntegrationEventHandlers(typeof(ElsaModule).Assembly);

        builder.Services.AddElsaStubServices();
        builder.Services.AddScoped<IApprovalGateway, ApprovalGateway>();

        builder.ConfigElsa();

        builder.Services.AddHealthChecks().AddDbContextCheck<ElsaDbContext>(
            name: "db:elsa",
            failureStatus: HealthStatus.Unhealthy);
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);
    }

    public void ConfigureMiddleware(IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);
        
        app.Use(async (context, next) =>
        {
            ArgumentNullException.ThrowIfNull(context);
            if (context.Request.Path.StartsWithSegments("/elsa/api", StringComparison.OrdinalIgnoreCase))
            {
                var authService = context.RequestServices.GetRequiredService<IAuthorizationService>();
                var result = await authService.AuthorizeAsync(context.User, "RequiredPermission");

                if (!result.Succeeded)
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return;
                }
            }

            await next();
        });
        app.UseTenants();
        app.UseWorkflowsApi();
    }
}