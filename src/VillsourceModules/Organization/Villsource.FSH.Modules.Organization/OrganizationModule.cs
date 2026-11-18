using Asp.Versioning;
using FluentValidation;
using FSH.Framework.Persistence;
using FSH.Framework.Shared.Constants;
using FSH.Framework.Web.Modules;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Villsource.FSH.Modules.Organization.Constants;
using Villsource.FSH.Modules.Organization.Contracts.Authorization;
using Villsource.FSH.Modules.Organization.Contracts.Services;
using Villsource.FSH.Modules.Organization.Data;
using Villsource.FSH.Modules.Organization.Features.v1.CostCenters.Endpoints;
using Villsource.FSH.Modules.Organization.Features.v1.DocumentTypes.Endpoints;
using Villsource.FSH.Modules.Organization.Features.v1.JobFamilies.Endpoints;
using Villsource.FSH.Modules.Organization.Features.v1.JobGrades.Endpoints;
using Villsource.FSH.Modules.Organization.Features.v1.Jobs.Endpoints;
using Villsource.FSH.Modules.Organization.Features.v1.LegalEntities.Endpoints;
using Villsource.FSH.Modules.Organization.Features.v1.OrganizationUnits.Endpoints;
using Villsource.FSH.Modules.Organization.Features.v1.OrganizationUnitTypes.Endpoints;
using Villsource.FSH.Modules.Organization.Features.v1.PositionDelegations.Endpoints;
using Villsource.FSH.Modules.Organization.Features.v1.Positions.Endpoints;
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
        builder.Services.AddScoped<IChainOfCommandService, ChainOfCommandService>();
        builder.Services.AddHostedService<DocumentTypeSyncHostedService>();
        builder.Services.AddValidatorsFromAssembly(typeof(OrganizationModule).Assembly);

        builder.Services.AddHealthChecks().AddDbContextCheck<OrganizationDbContext>(
            name: "db:organization",
            failureStatus: HealthStatus.Unhealthy);
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
            .WithApiVersionSet(versionSet);

        group.MapOrganizationUnitEndpoints();
        group.MapOrganizationUnitTypeEndpoints();
        group.MapLegalEntityEndpoints();
        group.MapCostCenterEndpoints();
        group.MapJobGradeEndpoints();
        group.MapJobFamilyEndpoints();
        group.MapJobEndpoints();
        group.MapPositionDelegationEndpoints();
        group.MapDocumentTypeEndpoints();
        group.MapPositionEndpoints();
    }
}
