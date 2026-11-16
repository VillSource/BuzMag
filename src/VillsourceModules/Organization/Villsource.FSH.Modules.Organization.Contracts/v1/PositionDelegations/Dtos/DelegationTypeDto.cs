namespace Villsource.FSH.Modules.Organization.Contracts.v1.PositionDelegations.Dtos;

public sealed record DelegationTypeDto(
    string Key,
    string DisplayName,
    bool RequiresDocumentScope);
