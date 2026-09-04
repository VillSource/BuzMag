using Asp.Versioning;
using FluentValidation;
using FSH.Framework.Persistence;
using FSH.Framework.Shared.Constants;
using FSH.Framework.Web.Modules;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Villsource.FSH.Modules.Organization.Constants;
using Villsource.FSH.Modules.Organization.Contracts.Authorization;
using Villsource.FSH.Modules.Organization.Data;
using Villsource.FSH.Modules.Organization.Features.v1.Structures.OrganizationUnits;
using Villsource.FSH.Modules.Organization.Features.v1.Structures.Positions;
using Villsource.FSH.Modules.Organization.Features.v1.Structures.PositionAllocations;
using Villsource.FSH.Modules.Organization.Services;

namespace Villsource.FSH.Modules.Organization;

public sealed class OrganizationModule : IModule
{
    public void ConfigureServices(IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        PermissionConstants.Register(OrganizationPermissions.All);

        builder.Services.AddHeroDbContext<OrganizationDbContext>();
        builder.Services.AddScoped<IDbInitializer, OrganizationDbInitializer>();
        builder.Services.AddValidatorsFromAssembly(typeof(OrganizationModule).Assembly);

        builder.Services.AddHealthChecks().AddDbContextCheck<OrganizationDbContext>(
            name: "db:organization",
            failureStatus: HealthStatus.Unhealthy);

        builder.Services.AddTransient<IOrganizationUnitService, OrganizationUnitService>();
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var versionSet = endpoints.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1))
            .ReportApiVersions()
            .Build();

        var group = endpoints.MapGroup("api/v{version:apiVersion}/organizations")
            .WithTags(ApiTags.Organization)
            .WithApiVersionSet(versionSet)
            .RequireAuthorization();

        group.MapGetAllOrganizationUnitEndpoint();
        group.MapCreateOrganizationUnitEndpoint();
        group.MapDeleteOrganizationUnitEndpoint();
        group.MapUpdateOrganizationUnitEndpoint();
        group.MapMoveOrganizationUnitEndpoint();

        group.MapGetAllPositionsEndpoint();
        group.MapGetPositionByIdEndpoint();
        group.MapCreatePositionEndpoint();
        group.MapUpdatePositionEndpoint();
        group.MapDeletePositionEndpoint();

        group.MapGetCurrentPositionAllocationsEndpoint();
        group.MapGetPositionAllocationHistoryEndpoint();
        group.MapAllocatePositionEndpoint();
        group.MapChangePositionAllocationEndpoint();
        group.MapEndPositionAllocationEndpoint();

        group.MapGetPositionTiers();
    }
}
