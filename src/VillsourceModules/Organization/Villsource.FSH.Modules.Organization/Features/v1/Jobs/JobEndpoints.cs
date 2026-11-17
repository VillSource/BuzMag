using FSH.Framework.Shared.Persistence;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System.ComponentModel;
using Villsource.FSH.Modules.Organization.Contracts.v1.Jobs.Commands;
using Villsource.FSH.Modules.Organization.Contracts.v1.Jobs.Dtos;
using Villsource.FSH.Modules.Organization.Contracts.v1.Jobs.Queries;

namespace Villsource.FSH.Modules.Organization.Features.v1.Jobs;

public static class JobEndpoints
{
    public static IEndpointRouteBuilder MapJobEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/jobs")
            .WithTags("Jobs")
            .RequireAuthorization()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapGet("/", async (
                [AsParameters] SearchJobsQuery query,
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                return Results.Ok(await mediator.Send(query, cancellationToken));
            })
            .WithName("SearchJobs")
            .WithSummary("Search jobs")
            .WithDescription("Searches and paginates standardized job templates.")
            .Produces<PagedResponse<JobSummaryDto>>(StatusCodes.Status200OK, "application/json")
            .ProducesValidationProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/lookups", async (
                [FromQuery, Description("Filter to active jobs only (default: true).")] bool activeOnly = true,
                [FromQuery, Description("Optional job family filter.")] string? jobFamily = null,
                ISender mediator = default!,
                CancellationToken cancellationToken = default) =>
            {
                var result = await mediator.Send(new GetJobLookupsQuery(activeOnly, jobFamily), cancellationToken);
                return Results.Ok(result);
            })
            .WithName("GetJobLookups")
            .WithSummary("List job lookups")
            .WithDescription("Retrieves a lightweight list of jobs with default grade metadata for dropdown selectors.")
            .Produces<IReadOnlyList<JobLookupDto>>(StatusCodes.Status200OK, "application/json")
            .ProducesValidationProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/{refId}", async (
                [Description("Unique reference ID of the job.")] string refId,
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                return Results.Ok(await mediator.Send(new GetJobByRefIdQuery(refId), cancellationToken));
            })
            .WithName("GetJob")
            .WithSummary("Get job by reference ID")
            .Produces<JobDetailDto>(StatusCodes.Status200OK, "application/json")
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", async (
                [FromBody] CreateJobCommand command,
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                var refId = await mediator.Send(command, cancellationToken);
                return Results.CreatedAtRoute("GetJob", new { refId }, refId);
            })
            .WithName("CreateJob")
            .WithSummary("Create a job")
            .Accepts<CreateJobCommand>("application/json")
            .Produces<string>(StatusCodes.Status201Created, "application/json")
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPut("/{refId}", async (
                [Description("Unique reference ID of the job.")] string refId,
                [FromBody] UpdateJobRequest request,
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                return Results.Ok(await mediator.Send(request.ToCommand(refId), cancellationToken));
            })
            .WithName("UpdateJob")
            .WithSummary("Update a job")
            .Accepts<UpdateJobRequest>("application/json")
            .Produces<string>(StatusCodes.Status200OK, "application/json")
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPatch("/{refId}/activate", async (
                [Description("Unique reference ID of the job.")] string refId,
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                return Results.Ok(await mediator.Send(new ActivateJobCommand(refId), cancellationToken));
            })
            .WithName("ActivateJob")
            .WithSummary("Activate a job")
            .Produces<string>(StatusCodes.Status200OK, "application/json")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPatch("/{refId}/deactivate", async (
                [Description("Unique reference ID of the job.")] string refId,
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                return Results.Ok(await mediator.Send(new DeactivateJobCommand(refId), cancellationToken));
            })
            .WithName("DeactivateJob")
            .WithSummary("Deactivate a job")
            .Produces<string>(StatusCodes.Status200OK, "application/json")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{refId}", async (
                [Description("Unique reference ID of the job.")] string refId,
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                return Results.Ok(await mediator.Send(new DeleteJobCommand(refId), cancellationToken));
            })
            .WithName("DeleteJob")
            .WithSummary("Delete a job")
            .Produces<string>(StatusCodes.Status200OK, "application/json")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }
}