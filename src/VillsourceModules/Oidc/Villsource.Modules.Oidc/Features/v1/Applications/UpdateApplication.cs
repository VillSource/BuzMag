using FluentValidation;
using FSH.Framework.Shared.Identity.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using OpenIddict.Abstractions;
using Villsource.Modules.Oidc.Contracts.Authorization;
using Villsource.Modules.Oidc.Contracts.v1.Applications;

namespace Villsource.Modules.Oidc.Features.v1.Applications;

public sealed class UpdateApplicationValidator : AbstractValidator<UpdateApplicationRequest>
{
    public UpdateApplicationValidator()
    {
        When(x => x.RedirectUris is not null, () =>
        {
            RuleForEach(x => x.RedirectUris!)
                .Must(u => Uri.TryCreate(u, UriKind.Absolute, out _))
                .WithMessage("Each RedirectUri must be an absolute URI.");
        });
    }
}

public static class UpdateApplicationEndpoint
{
    public static IEndpointRouteBuilder MapUpdateApplicationEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPut("/{id}", async (
            string id,
            UpdateApplicationRequest request,
            IOpenIddictApplicationManager manager,
            IValidator<UpdateApplicationRequest> validator,
            CancellationToken ct) =>
        {
            var validation = await validator.ValidateAsync(request, ct).ConfigureAwait(false);
            if (!validation.IsValid)
            {
                return Results.ValidationProblem(validation.ToDictionary());
            }

            var app = await manager.FindByIdAsync(id, ct).ConfigureAwait(false);
            if (app is null)
            {
                return Results.NotFound();
            }

            var descriptor = new OpenIddictApplicationDescriptor();
            await manager.PopulateAsync(descriptor, app, ct).ConfigureAwait(false);

            if (request.DisplayName is not null)
            {
                descriptor.DisplayName = request.DisplayName;
            }

            if (request.ClientSecret is not null)
            {
                descriptor.ClientSecret = request.ClientSecret;
            }

            if (request.RedirectUris is not null)
            {
                descriptor.RedirectUris.Clear();
                foreach (var uri in request.RedirectUris)
                {
                    descriptor.RedirectUris.Add(new Uri(uri));
                }
            }

            if (request.PostLogoutRedirectUris is not null)
            {
                descriptor.PostLogoutRedirectUris.Clear();
                foreach (var uri in request.PostLogoutRedirectUris)
                {
                    descriptor.PostLogoutRedirectUris.Add(new Uri(uri));
                }
            }

            await manager.UpdateAsync(app, descriptor, ct).ConfigureAwait(false);
            return Results.NoContent();
        })
        .WithSummary("Update an existing OIDC application")
        .RequirePermission(OidcPermissions.Applications.Update);

        return endpoints;
    }
}
