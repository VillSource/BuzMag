using System.ComponentModel;
using FSH.Framework.Shared.Persistence;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Villsource.FSH.Modules.Organization.Contracts.v1.JobFamilies.Commands;
using Villsource.FSH.Modules.Organization.Contracts.v1.JobFamilies.Queries;

namespace Villsource.FSH.Modules.Organization.Features.v1.JobFamilies.Endpoints;

public static class JobFamilyEndpoints
{
    public static IEndpointRouteBuilder MapJobFamilyEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/job-families")
            .WithTags("Job Families")
            .RequireAuthorization()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapGet("/", async (
                [AsParameters] SearchJobFamiliesRequest request,
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                var result = await mediator.Send(request.ToQuery(), cancellationToken);
                return Results.Ok(result.ToResponse());
            })
            .WithName("SearchJobFamilies")
            .WithSummary("Search job families")
            .Produces<PagedResponse<JobFamilySummaryResponse>>(StatusCodes.Status200OK, "application/json")
            .ProducesValidationProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/lookups", async (
                [FromQuery, Description("Filter to active job families only (default: true).")] bool activeOnly = true,
                ISender mediator = default!,
                CancellationToken cancellationToken = default) =>
            {
                var result = await mediator.Send(new GetJobFamilyLookupsQuery(activeOnly), cancellationToken);
                return Results.Ok(result.ToResponse());
            })
            .WithName("GetJobFamilyLookups")
            .WithSummary("List job family lookups")
            .Produces<IReadOnlyList<JobFamilyLookupResponse>>(StatusCodes.Status200OK, "application/json");

        group.MapGet("/{refId}", async (
                [Description("Unique reference ID of the job family.")] string refId,
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                var result = await mediator.Send(new GetJobFamilyByRefIdQuery(refId), cancellationToken);
                return Results.Ok(result.ToResponse());
            })
            .WithName("GetJobFamily")
            .WithSummary("Get job family by reference ID")
            .Produces<JobFamilyDetailResponse>(StatusCodes.Status200OK, "application/json")
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", async (
                [FromBody] CreateJobFamilyRequest request,
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                var refId = await mediator.Send(request.ToCommand(), cancellationToken);
                return Results.CreatedAtRoute("GetJobFamily", new { refId }, refId);
            })
            .WithName("CreateJobFamily")
            .WithSummary("Create a job family")
            .Accepts<CreateJobFamilyRequest>("application/json")
            .Produces<string>(StatusCodes.Status201Created, "application/json")
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPut("/{refId}", async (
                [Description("Unique reference ID of the job family.")] string refId,
                [FromBody] UpdateJobFamilyRequest request,
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                var updatedRefId = await mediator.Send(request.ToCommand(refId), cancellationToken);
                return Results.Ok(updatedRefId);
            })
            .WithName("UpdateJobFamily")
            .WithSummary("Update a job family")
            .Accepts<UpdateJobFamilyRequest>("application/json")
            .Produces<string>(StatusCodes.Status200OK, "application/json")
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/{refId}/activate", async (
                [Description("Unique reference ID of the job family.")] string refId,
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                await mediator.Send(new ActivateJobFamilyCommand(refId), cancellationToken);
                return Results.NoContent();
            })
            .WithName("ActivateJobFamily")
            .WithSummary("Activate a job family")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/{refId}/deactivate", async (
                [Description("Unique reference ID of the job family.")] string refId,
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                await mediator.Send(new DeactivateJobFamilyCommand(refId), cancellationToken);
                return Results.NoContent();
            })
            .WithName("DeactivateJobFamily")
            .WithSummary("Deactivate a job family")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{refId}", async (
                [Description("Unique reference ID of the job family.")] string refId,
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                await mediator.Send(new DeleteJobFamilyCommand(refId), cancellationToken);
                return Results.NoContent();
            })
            .WithName("DeleteJobFamily")
            .WithSummary("Delete a job family")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        return app;
    }
}
