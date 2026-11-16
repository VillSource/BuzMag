using Mediator;

namespace Villsource.FSH.Modules.Organization.Contracts.v1.LegalEntities.Commands;

public sealed record UpdateLegalEntityProfileCommand(
    string RefId,
    string? TradeName,
    string? LogoUrl,
    string? Website,
    string? ContactEmail,
    string? ContactPhone) : IRequest<string>;