using System.ComponentModel;
using FSH.Framework.Shared.Persistence;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Villsource.FSH.Modules.Organization.Contracts.v1.JobGrades.Commands;
using Villsource.FSH.Modules.Organization.Contracts.v1.JobGrades.Queries;

namespace Villsource.FSH.Modules.Organization.Features.v1.JobGrades.Endpoints;

public static class JobGradeEndpoints
{
    public static IEndpointRouteBuilder MapJobGradeEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/job-grades")
            .WithTags("Job Grades")
            .RequireAuthorization()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapGet("/", async (
                [AsParameters] SearchJobGradesRequest request,
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                var result = await mediator.Send(request.ToQuery(), cancellationToken);
                return Results.Ok(result.ToResponse());
            })
            .WithName("SearchJobGrades")
            .WithSummary("Search job grades")
            .WithDescription("Searches and paginates job grades ordered by authority rank.")
            .Produces<PagedResponse<JobGradeSummaryResponse>>(StatusCodes.Status200OK, "application/json")
            .ProducesValidationProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/categories", async (
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                var result = await mediator.Send(new GetJobLevelCategoriesQuery(), cancellationToken);
                return Results.Ok(result.ToResponse());
            })
            .WithName("GetJobLevelCategories")
            .WithSummary("List job level categories")
            .Produces<IReadOnlyList<JobLevelCategoryResponse>>(StatusCodes.Status200OK, "application/json");

        group.MapGet("/lookups", async (
                [FromQuery, Description("Filter to active job grades only (default: true).")]
                bool activeOnly = true,
                ISender mediator = default!,
                CancellationToken cancellationToken = default) =>
            {
                var result = await mediator.Send(new GetJobGradeLookupsQuery(activeOnly), cancellationToken);
                return Results.Ok(result.ToResponse());
            })
            .WithName("GetJobGradeLookups")
            .WithSummary("List job grade lookups")
            .WithDescription(
                "Retrieves a lightweight list of job grades ordered by rank for dropdowns and approval matrix configuration.")
            .Produces<IReadOnlyList<JobGradeLookupResponse>>(StatusCodes.Status200OK, "application/json");

        group.MapGet("/{refId}", async (
                [Description("Unique reference ID of the job grade.")]
                string refId,
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                var result = await mediator.Send(new GetJobGradeByRefIdQuery(refId), cancellationToken);
                return Results.Ok(result.ToResponse());
            })
            .WithName("GetJobGrade")
            .WithSummary("Get job grade by reference ID")
            .WithDescription("Retrieves job grade details and authority rank.")
            .Produces<JobGradeDetailResponse>(StatusCodes.Status200OK, "application/json")
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", async (
                [FromBody] CreateJobGradeRequest request,
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                var refId = await mediator.Send(request.ToCommand(), cancellationToken);
                return Results.CreatedAtRoute("GetJobGrade", new { refId }, refId);
            })
            .WithName("CreateJobGrade")
            .WithSummary("Create a job grade")
            .WithDescription("Creates a new job grade with seniority rank order.")
            .Accepts<CreateJobGradeRequest>("application/json")
            .Produces<string>(StatusCodes.Status201Created, "application/json")
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPut("/{refId}", async (
                [Description("Unique reference ID of the job grade.")]
                string refId,
                [FromBody] UpdateJobGradeRequest request,
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                var updatedRefId = await mediator.Send(request.ToCommand(refId), cancellationToken);
                return Results.Ok(updatedRefId);
            })
            .WithName("UpdateJobGrade")
            .WithSummary("Update a job grade")
            .WithDescription("Updates an existing job grade.")
            .Accepts<UpdateJobGradeRequest>("application/json")
            .Produces<string>(StatusCodes.Status200OK, "application/json")
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPatch("/{refId}/activate", async (
                [Description("Unique reference ID of the job grade.")]
                string refId,
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                return Results.Ok(await mediator.Send(new ActivateJobGradeCommand(refId), cancellationToken));
            })
            .WithName("ActivateJobGrade")
            .WithSummary("Activate a job grade")
            .Produces<string>(StatusCodes.Status200OK, "application/json")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPatch("/{refId}/deactivate", async (
                [Description("Unique reference ID of the job grade.")]
                string refId,
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                return Results.Ok(await mediator.Send(new DeactivateJobGradeCommand(refId), cancellationToken));
            })
            .WithName("DeactivateJobGrade")
            .WithSummary("Deactivate a job grade")
            .Produces<string>(StatusCodes.Status200OK, "application/json")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{refId}", async (
                [Description("Unique reference ID of the job grade.")]
                string refId,
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                return Results.Ok(await mediator.Send(new DeleteJobGradeCommand(refId), cancellationToken));
            })
            .WithName("DeleteJobGrade")
            .WithSummary("Delete a job grade")
            .WithDescription("Soft-deletes a job grade if no jobs or positions reference it.")
            .Produces<string>(StatusCodes.Status200OK, "application/json")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }
}
