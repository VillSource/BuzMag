namespace Villsource.FSH.Modules.Organization.Contracts.v1.LegalEntities.Dtos;

public sealed record LegalEntityDetailDto(
    string RefId,
    string Code,
    string CountryCode,
    string LegalName,
    string InternationalName,
    string TaxId,
    string? RegistrationNumber,
    string BaseCurrency,
    CompanyProfileDto Profile,
    IReadOnlyList<TaxBranchDto> Branches,
    DateTimeOffset CreatedOnUtc,
    DateTimeOffset? LastModifiedOnUtc);