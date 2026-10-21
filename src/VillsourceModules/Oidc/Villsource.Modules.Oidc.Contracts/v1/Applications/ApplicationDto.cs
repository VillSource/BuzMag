namespace Villsource.Modules.Oidc.Contracts.v1.Applications;

public sealed record ApplicationDto(
    string Id,
    string ClientId,
    string? DisplayName,
    string ClientType,
    IReadOnlyList<string> RedirectUris,
    IReadOnlyList<string> PostLogoutRedirectUris,
    IReadOnlyList<string> Permissions);
