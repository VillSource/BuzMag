using Asp.Versioning;
using FSH.Framework.Shared.Identity.Authorization;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Villsource.Modules.HumanResource.Constants;
using Villsource.Modules.HumanResource.Contracts.Authorization;
using Villsource.Modules.HumanResource.Contracts.Dtos;
using Villsource.Modules.HumanResource.Contracts.v1;

namespace Villsource.Modules.HumanResource.Features.v1;

public static class HumanResourceEndpointsExtension
{
    extension(IEndpointRouteBuilder endpoints)
    {
        public IEndpointRouteBuilder MapHumanResourceEndpoints()
        {
            var versionSet = endpoints.NewApiVersionSet()
                .HasApiVersion(new ApiVersion(1))
                .ReportApiVersions()
                .Build();

            var group = endpoints.MapGroup("api/v{version:apiVersion}/employees")
                .WithTags(ApiTags.HumanResource)
                .WithApiVersionSet(versionSet)
                .RequireAuthorization();

            group.MapCreateNewEmployeeEndpoint();
            group.MapUpdateEmployeeInfoEndpoint();
            group.MapAssignPositionEndpoint();
            group.MapUpdateEmploymentEndpoint();
            group.MapTerminateEmploymentEndpoint();
            group.MapGetEmployeesEndpoint();
            group.MapGetEmployeeBriefEndpoint();
            group.MapGetEmployeeDetailEndpoint();
            group.MapGetCurrentPositionsEndpoint();
            group.MapGetEmploymentHistoryEndpoint();
            group.MapGetPositionHistoryEndpoint();

            return endpoints;
        }

        internal RouteHandlerBuilder MapCreateNewEmployeeEndpoint()
            => endpoints.MapPost("/",
                    async ([FromBody] CreateNewEmployeeCommand command, IMediator mediator, CancellationToken cancellationToken) =>
                    {
                        var result = await mediator.Send(command, cancellationToken);
                        return Results.Created($"/api/v1/employees/{result.Ref}", result);
                    })
                .Produces<EmployeeDto>(StatusCodes.Status201Created)
                .WithName("CreateNewEmployee")
                .WithSummary("Create a new employee.")
                .RequirePermission(HumanResourcePermissions.Employees.Create);

        internal RouteHandlerBuilder MapUpdateEmployeeInfoEndpoint()
            => endpoints.MapPut("/{ref}",
                    async (string @ref, [FromBody] UpdateEmployeeInfoCommand command, IMediator mediator, CancellationToken cancellationToken) =>
                    {
                        var result = await mediator.Send(command with { EmployeeRef = @ref }, cancellationToken);
                        return Results.Ok(result);
                    })
                .Produces<EmployeeDto>()
                .WithName("UpdateEmployeeInfo")
                .WithSummary("Update employee personal information.")
                .RequirePermission(HumanResourcePermissions.Employees.Update);

        internal RouteHandlerBuilder MapAssignPositionEndpoint()
            => endpoints.MapPost("/{ref}/positions",
                    async (string @ref, [FromBody] AssignPositionCommand command, IMediator mediator, CancellationToken cancellationToken) =>
                    {
                        var result = await mediator.Send(command with { EmployeeRef = @ref }, cancellationToken);
                        return Results.Ok(result);
                    })
                .Produces<PositionAssignmentDto>()
                .WithName("AssignPosition")
                .WithSummary("Assign position to employee.")
                .RequirePermission(HumanResourcePermissions.Employees.AssignPosition);

        internal RouteHandlerBuilder MapUpdateEmploymentEndpoint()
            => endpoints.MapPut("/{ref}/employment",
                    async (string @ref, [FromBody] UpdateEmploymentCommand command, IMediator mediator, CancellationToken cancellationToken) =>
                    {
                        var result = await mediator.Send(command with { EmployeeRef = @ref }, cancellationToken);
                        return Results.Ok(result);
                    })
                .Produces<EmploymentDto>()
                .WithName("UpdateEmployment")
                .WithSummary("Update employee employment status or type.")
                .RequirePermission(HumanResourcePermissions.Employees.UpdateEmployment);

        internal RouteHandlerBuilder MapTerminateEmploymentEndpoint()
            => endpoints.MapPost("/{ref}/terminate",
                    async (string @ref, [FromBody] TerminateEmploymentCommand command, IMediator mediator, CancellationToken cancellationToken) =>
                    {
                        var result = await mediator.Send(command with { EmployeeRef = @ref }, cancellationToken);
                        return Results.Ok(result);
                    })
                .Produces<EmployeeDto>()
                .WithName("TerminateEmployment")
                .WithSummary("Terminate employment.")
                .RequirePermission(HumanResourcePermissions.Employees.Terminate);

        internal RouteHandlerBuilder MapGetEmployeesEndpoint()
            => endpoints.MapGet("/",
                    async ([AsParameters] GetEmployeesQuery query, IMediator mediator, CancellationToken cancellationToken) =>
                    {
                        var result = await mediator.Send(query, cancellationToken);
                        return Results.Ok(result);
                    })
                .Produces<ICollection<EmployeeDto>>()
                .WithName("GetEmployees")
                .WithSummary("Get all employees.")
                .RequirePermission(HumanResourcePermissions.Employees.View);

        internal RouteHandlerBuilder MapGetEmployeeBriefEndpoint()
            => endpoints.MapGet("/{ref}/brief",
                    async (string @ref, IMediator mediator, CancellationToken cancellationToken) =>
                    {
                        var result = await mediator.Send(new GetEmployeeBriefQuery(@ref), cancellationToken);
                        return Results.Ok(result);
                    })
                .Produces<EmployeeBriefDto>()
                .WithName("GetEmployeeBrief")
                .WithSummary("Get brief employee details for reference.")
                .RequirePermission(HumanResourcePermissions.Employees.View);

        internal RouteHandlerBuilder MapGetEmployeeDetailEndpoint()
            => endpoints.MapGet("/{ref}",
                    async (string @ref, IMediator mediator, CancellationToken cancellationToken) =>
                    {
                        var result = await mediator.Send(new GetEmployeeDetailQuery(@ref), cancellationToken);
                        return Results.Ok(result);
                    })
                .Produces<EmployeeDto>()
                .WithName("GetEmployeeDetail")
                .WithSummary("Get full employee details.")
                .RequirePermission(HumanResourcePermissions.Employees.View);

        internal RouteHandlerBuilder MapGetCurrentPositionsEndpoint()
            => endpoints.MapGet("/{ref}/positions/current",
                    async (string @ref, [FromQuery] bool? isPrimary, IMediator mediator, CancellationToken cancellationToken) =>
                    {
                        var result = await mediator.Send(new GetCurrentPositionsQuery(@ref, isPrimary), cancellationToken);
                        return Results.Ok(result);
                    })
                .Produces<ICollection<PositionAssignmentDto>>()
                .WithName("GetCurrentPositions")
                .WithSummary("Get current active position assignments.")
                .RequirePermission(HumanResourcePermissions.Employees.View);

        internal RouteHandlerBuilder MapGetEmploymentHistoryEndpoint()
            => endpoints.MapGet("/{ref}/employments/history",
                    async (string @ref, IMediator mediator, CancellationToken cancellationToken) =>
                    {
                        var result = await mediator.Send(new GetEmploymentHistoryQuery(@ref), cancellationToken);
                        return Results.Ok(result);
                    })
                .Produces<ICollection<EmploymentDto>>()
                .WithName("GetEmploymentHistory")
                .WithSummary("Get employment history.")
                .RequirePermission(HumanResourcePermissions.Employees.View);

        internal RouteHandlerBuilder MapGetPositionHistoryEndpoint()
            => endpoints.MapGet("/{ref}/positions/history",
                    async (string @ref, IMediator mediator, CancellationToken cancellationToken) =>
                    {
                        var result = await mediator.Send(new GetPositionHistoryQuery(@ref), cancellationToken);
                        return Results.Ok(result);
                    })
                .Produces<ICollection<PositionAssignmentDto>>()
                .WithName("GetPositionHistory")
                .WithSummary("Get position assignment history.")
                .RequirePermission(HumanResourcePermissions.Employees.View);
    }
}