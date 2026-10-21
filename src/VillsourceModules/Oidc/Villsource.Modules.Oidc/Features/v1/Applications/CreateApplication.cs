using FluentValidation;
using FSH.Framework.Shared.Identity.Authorization;
using FSH.Framework.Web.Modules;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Abstractions;
using Villsource.Modules.Oidc.Contracts.Authorization;
using Villsource.Modules.Oidc.Contracts.v1.Applications;

namespace Villsource.Modules.Oidc.Features.v1.Applications;

public sealed class CreateApplicationValidator : AbstractValidator<CreateApplicationRequest>
{
    public CreateApplicationValidator()
    {
        RuleFor(x => x.ClientId)
            .NotEmpty().WithMessage("ClientId is required.")
            .MaximumLength(100);

        RuleFor(x => x.RedirectUris)
            .NotNull()
            .ForEach(uri => uri.Must(u => Uri.TryCreate(u, UriKind.Absolute, out _))
                .WithMessage("Each RedirectUri must be an absolute URI."));
    }
}

public static class CreateApplicationEndpoint
{
    public static IEndpointRouteBuilder MapCreateApplicationEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/", async (
            CreateApplicationRequest request,
            IOpenIddictApplicationManager manager,
            IValidator<CreateApplicationRequest> validator,
            CancellationToken ct) =>
        {
            var validation = await validator.ValidateAsync(request, ct).ConfigureAwait(false);
            if (!validation.IsValid)
            {
                return Results.ValidationProblem(validation.ToDictionary());
            }

            if (await manager.FindByClientIdAsync(request.ClientId, ct).ConfigureAwait(false) is not null)
            {
                return Results.Conflict(new { error = "client_id_conflict", error_description = "A client with this ClientId already exists." });
            }

            var descriptor = new OpenIddictApplicationDescriptor
            {
                ClientId = request.ClientId,
                DisplayName = request.DisplayName,
                ClientSecret = request.ClientSecret,
                ClientType = string.IsNullOrEmpty(request.ClientSecret)
                    ? OpenIddictConstants.ClientTypes.Public
                    : OpenIddictConstants.ClientTypes.Confidential,
            };

            foreach (var uri in request.RedirectUris)
            {
                descriptor.RedirectUris.Add(new Uri(uri));
            }

            foreach (var uri in request.PostLogoutRedirectUris)
            {
                descriptor.PostLogoutRedirectUris.Add(new Uri(uri));
            }

            descriptor.Permissions.Add(OpenIddictConstants.Permissions.Endpoints.Authorization);
            descriptor.Permissions.Add(OpenIddictConstants.Permissions.Endpoints.Token);
            descriptor.Permissions.Add(OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode);
            descriptor.Permissions.Add(OpenIddictConstants.Permissions.GrantTypes.RefreshToken);
            descriptor.Permissions.Add(OpenIddictConstants.Permissions.ResponseTypes.Code);
            descriptor.Permissions.Add(OpenIddictConstants.Permissions.Scopes.Email);
            descriptor.Permissions.Add(OpenIddictConstants.Permissions.Scopes.Profile);
            descriptor.Permissions.Add(OpenIddictConstants.Permissions.Scopes.Roles);

            var app = await manager.CreateAsync(descriptor, ct).ConfigureAwait(false);
            var id = await manager.GetIdAsync(app!, ct).ConfigureAwait(false);

            return Results.Created($"/api/v1/oidc/applications/{id}", new { Id = id });
        })
        .WithSummary("Create a new OIDC application (client)")
        .RequirePermission(OidcPermissions.Applications.Create);

        return endpoints;
    }
}
