namespace Villsource.FSH.Modules.Organization.Contracts.v1.DocumentTypes.Dtos;

public sealed record DocumentTypeDto(
    string Code,
    string Name,
    string ModuleName,
    string? Description);
