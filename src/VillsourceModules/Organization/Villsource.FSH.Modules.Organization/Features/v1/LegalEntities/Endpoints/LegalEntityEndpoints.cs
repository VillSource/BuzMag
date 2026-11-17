using FSH.Framework.Shared.Persistence;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System.ComponentModel;
using Villsource.FSH.Modules.Organization.Contracts.v1.LegalEntities.Commands;
using Villsource.FSH.Modules.Organization.Contracts.v1.LegalEntities.Dtos;
using Villsource.FSH.Modules.Organization.Contracts.v1.LegalEntities.Queries;

namespace Villsource.FSH.Modules.Organization.Features.v1.LegalEntities;

public static class LegalEntityEndpoints
{
    public static IEndpointRouteBuilder MapLegalEntityEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/legal-entities")
            .WithTags("Legal Entities")
            .RequireAuthorization()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        // Queries
        group.MapGet("/", async (
                [AsParameters] SearchLegalEntitiesQuery query,
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                var result = await mediator.Send(request: query, cancellationToken: cancellationToken);
                return Results.Ok(result);
            })
            .WithName("SearchLegalEntities")
            .WithSummary("Search legal entities")
            .WithDescription(
                "Searches and paginates legal entities with optional keyword, country code filter, and sorting.")
            .Produces<PagedResponse<LegalEntitySummaryDto>>(StatusCodes.Status200OK, "application/json")
            .ProducesValidationProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/{refId}", async (
                [Description("Unique reference ID of the legal entity.")]
                string refId,
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                var result = await mediator.Send(
                    request: new GetLegalEntityByRefIdQuery(RefId: refId),
                    cancellationToken: cancellationToken);
                return Results.Ok(result);
            })
            .WithName("GetLegalEntity")
            .WithSummary("Get legal entity by reference ID")
            .WithDescription(
                "Retrieves detailed legal entity information, company profile, and associated tax branches.")
            .Produces<LegalEntityDetailDto>(StatusCodes.Status200OK, "application/json")
            .ProducesProblem(StatusCodes.Status404NotFound);

        // LegalEntity Commands
        group.MapPost("/", async (
                [FromBody] CreateLegalEntityCommand command,
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                var refId = await mediator.Send(request: command, cancellationToken: cancellationToken);
                return Results.CreatedAtRoute(
                    routeName: "GetLegalEntity",
                    routeValues: new { refId },
                    value: refId);
            })
            .WithName("CreateLegalEntity")
            .WithSummary("Create a legal entity")
            .WithDescription(
                "Creates a new legal entity with optional company profile and head office tax branch, returning its reference ID.")
            .Accepts<CreateLegalEntityCommand>("application/json")
            .Produces<string>(StatusCodes.Status201Created, "application/json")
            .ProducesValidationProblem(StatusCodes.Status400BadRequest);

        group.MapPut("/{refId}", async (
                [Description("Unique reference ID of the legal entity.")]
                string refId,
                [FromBody] UpdateLegalEntityRequest request,
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                var command = request.ToCommand(refId: refId);
                var updatedRefId = await mediator.Send(request: command, cancellationToken: cancellationToken);
                return Results.Ok(updatedRefId);
            })
            .WithName("UpdateLegalEntity")
            .WithSummary("Update a legal entity")
            .WithDescription("Updates core legal registration and tax details of an existing legal entity.")
            .Accepts<UpdateLegalEntityRequest>("application/json")
            .Produces<string>(StatusCodes.Status200OK, "application/json")
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/{refId}/profile", async (
                [Description("Unique reference ID of the legal entity.")]
                string refId,
                [FromBody] UpdateLegalEntityProfileRequest request,
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                var command = request.ToCommand(refId: refId);
                var updatedRefId = await mediator.Send(request: command, cancellationToken: cancellationToken);
                return Results.Ok(updatedRefId);
            })
            .WithName("UpdateLegalEntityProfile")
            .WithSummary("Update legal entity profile")
            .WithDescription("Updates branding and contact profile metadata for an existing legal entity.")
            .Accepts<UpdateLegalEntityProfileRequest>("application/json")
            .Produces<string>(StatusCodes.Status200OK, "application/json")
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{refId}", async (
                [Description("Unique reference ID of the legal entity.")]
                string refId,
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                var deletedRefId = await mediator.Send(
                    request: new DeleteLegalEntityCommand(RefId: refId),
                    cancellationToken: cancellationToken);
                return Results.Ok(deletedRefId);
            })
            .WithName("DeleteLegalEntity")
            .WithSummary("Delete a legal entity")
            .WithDescription("Soft-deletes a legal entity and all of its associated tax branches.")
            .Produces<string>(StatusCodes.Status200OK, "application/json")
            .ProducesProblem(StatusCodes.Status404NotFound);

        // TaxBranch Sub-resource Commands
        group.MapPost("/{legalEntityRefId}/branches", async (
                [Description("Unique reference ID of the parent legal entity.")]
                string legalEntityRefId,
                [FromBody] AddTaxBranchRequest request,
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                var command = request.ToCommand(legalEntityRefId: legalEntityRefId);
                var branchRefId = await mediator.Send(request: command, cancellationToken: cancellationToken);
                return Results.Ok(branchRefId);
            })
            .WithName("AddTaxBranch")
            .WithSummary("Add a tax branch")
            .WithDescription(
                "Adds a new tax branch or establishment to an existing legal entity, returning the branch reference ID.")
            .Accepts<AddTaxBranchRequest>("application/json")
            .Produces<string>(StatusCodes.Status200OK, "application/json")
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/{legalEntityRefId}/branches/{refId}", async (
                [Description("Unique reference ID of the parent legal entity.")]
                string legalEntityRefId,
                [Description("Unique reference ID of the tax branch.")]
                string refId,
                [FromBody] UpdateTaxBranchRequest request,
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                var command = request.ToCommand(legalEntityRefId: legalEntityRefId, refId: refId);
                var updatedBranchRefId = await mediator.Send(request: command, cancellationToken: cancellationToken);
                return Results.Ok(updatedBranchRefId);
            })
            .WithName("UpdateTaxBranch")
            .WithSummary("Update a tax branch")
            .WithDescription("Updates registration details and address of an existing tax branch.")
            .Accepts<UpdateTaxBranchRequest>("application/json")
            .Produces<string>(StatusCodes.Status200OK, "application/json")
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPatch("/{legalEntityRefId}/branches/{refId}/activate", async (
                [Description("Unique reference ID of the parent legal entity.")]
                string legalEntityRefId,
                [Description("Unique reference ID of the tax branch.")]
                string refId,
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                var activatedRefId = await mediator.Send(
                    request: new ActivateTaxBranchCommand(LegalEntityRefId: legalEntityRefId, RefId: refId),
                    cancellationToken: cancellationToken);
                return Results.Ok(activatedRefId);
            })
            .WithName("ActivateTaxBranch")
            .WithSummary("Activate a tax branch")
            .WithDescription("Re-activates a closed tax branch.")
            .Produces<string>(StatusCodes.Status200OK, "application/json")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPatch("/{legalEntityRefId}/branches/{refId}/close", async (
                [Description("Unique reference ID of the parent legal entity.")]
                string legalEntityRefId,
                [Description("Unique reference ID of the tax branch.")]
                string refId,
                [FromQuery, Description("Optional effective closure timestamp in UTC.")]
                DateTimeOffset? closedDate,
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                var closedRefId = await mediator.Send(
                    request: new CloseTaxBranchCommand(
                        LegalEntityRefId: legalEntityRefId,
                        RefId: refId,
                        ClosedDate: closedDate),
                    cancellationToken: cancellationToken);
                return Results.Ok(closedRefId);
            })
            .WithName("CloseTaxBranch")
            .WithSummary("Close a tax branch")
            .WithDescription("Closes an active tax branch with an optional effective closure date.")
            .Produces<string>(StatusCodes.Status200OK, "application/json")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{legalEntityRefId}/branches/{refId}", async (
                [Description("Unique reference ID of the parent legal entity.")]
                string legalEntityRefId,
                [Description("Unique reference ID of the tax branch.")]
                string refId,
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                var removedRefId = await mediator.Send(
                    request: new RemoveTaxBranchCommand(LegalEntityRefId: legalEntityRefId, RefId: refId),
                    cancellationToken: cancellationToken);
                return Results.Ok(removedRefId);
            })
            .WithName("RemoveTaxBranch")
            .WithSummary("Remove a tax branch")
            .WithDescription("Soft-deletes a tax branch from a legal entity.")
            .Produces<string>(StatusCodes.Status200OK, "application/json")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }
}