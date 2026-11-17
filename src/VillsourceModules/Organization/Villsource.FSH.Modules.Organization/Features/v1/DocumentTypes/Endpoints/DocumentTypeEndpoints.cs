using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Villsource.FSH.Modules.Organization.Features.v1.DocumentTypes.Endpoints;

public static class DocumentTypeEndpoints
{
    public static IEndpointRouteBuilder MapDocumentTypeEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/document-types")
            .WithTags("Document Types")
            .RequireAuthorization()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapGet("/", async (
                [AsParameters] GetDocumentTypesRequest request,
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                var result = await mediator.Send(request.ToQuery(), cancellationToken);
                return Results.Ok(result.ToResponse());
            })
            .WithName("GetDocumentTypes")
            .WithSummary("List registered document types")
            .WithDescription("Retrieves read-only system document types registered by modules for position delegation scoping.")
            .Produces<IReadOnlyList<DocumentTypeResponse>>(StatusCodes.Status200OK, "application/json");

        return app;
    }
}
