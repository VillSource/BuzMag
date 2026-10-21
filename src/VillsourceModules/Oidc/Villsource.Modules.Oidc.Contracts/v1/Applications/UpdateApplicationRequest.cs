namespace Villsource.Modules.Oidc.Contracts.v1.Applications;

public sealed record UpdateApplicationRequest(
    string? DisplayName,
    string? ClientSecret,
    IReadOnlyList<string>? RedirectUris,
    IReadOnlyList<string>? PostLogoutRedirectUris);
