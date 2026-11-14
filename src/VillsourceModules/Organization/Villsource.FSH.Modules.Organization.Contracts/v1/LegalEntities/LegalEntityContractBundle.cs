using FSH.Framework.Shared.Persistence;
using Mediator;
using Villsource.ObjectValue;

namespace Villsource.FSH.Modules.Organization.Contracts.v1.LegalEntities;

#region DTOs

public sealed record LegalEntitySummaryDto(
    string RefId,
    string Code,
    string CountryCode,
    string LegalName,
    string InternationalName,
    string TaxId,
    string? RegistrationNumber,
    string BaseCurrency,
    string? TradeName,
    string? LogoUrl,
    int ActiveBranchCount);

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

public sealed record CompanyProfileDto(
    string? TradeName,
    string? LogoUrl,
    string? Website,
    string? ContactEmail,
    string? ContactPhone);

public sealed record TaxBranchDto(
    string RefId,
    string LegalEntityRefId,
    string BranchCode,
    bool IsHeadOffice,
    string Name,
    string InternationalName,
    string? TaxSubId,
    Address RegisteredAddress,
    bool IsActive,
    DateTimeOffset? RegisteredDate,
    DateTimeOffset? ClosedDate);

#endregion

#region Queries

public sealed record SearchLegalEntitiesQuery : IPagedQuery, IRequest<PagedResponse<LegalEntitySummaryDto>>
{
    public int? PageNumber { get; set; }
    public int? PageSize { get; set; }
    public string? Sort { get; set; }
    public string? Keyword { get; set; }
    public string? CountryCode { get; set; }
}

public sealed record GetLegalEntityByRefIdQuery(string RefId) : IRequest<LegalEntityDetailDto>;

#endregion

#region LegalEntity Commands

public sealed record CreateLegalEntityCommand(
    string Code,
    string CountryCode,
    string LegalName,
    string InternationalName,
    string TaxId,
    string BaseCurrency,
    string? RegistrationNumber = null,
    CompanyProfileDto? Profile = null,
    Address? HeadOfficeAddress = null,
    string? HeadOfficeBranchCode = null,
    string? HeadOfficeName = null,
    string? HeadOfficeInternationalName = null,
    DateTimeOffset? HeadOfficeRegisteredDate = null) : IRequest<string>;

public sealed record UpdateLegalEntityCommand(
    string RefId,
    string Code,
    string LegalName,
    string InternationalName,
    string TaxId,
    string BaseCurrency,
    string? RegistrationNumber = null) : IRequest<string>;

public sealed record UpdateLegalEntityProfileCommand(
    string RefId,
    string? TradeName,
    string? LogoUrl,
    string? Website,
    string? ContactEmail,
    string? ContactPhone) : IRequest<string>;

public sealed record DeleteLegalEntityCommand(string RefId) : IRequest<string>;

#endregion

#region TaxBranch Commands

public sealed record AddTaxBranchCommand(
    string LegalEntityRefId,
    string BranchCode,
    string Name,
    string InternationalName,
    Address RegisteredAddress,
    bool IsHeadOffice = false,
    string? TaxSubId = null,
    DateTimeOffset? RegisteredDate = null,
    bool IsActive = true) : IRequest<string>;

public sealed record UpdateTaxBranchCommand(
    string LegalEntityRefId,
    string RefId,
    string Name,
    string InternationalName,
    Address RegisteredAddress,
    string? TaxSubId = null,
    DateTimeOffset? RegisteredDate = null) : IRequest<string>;

public sealed record ActivateTaxBranchCommand(
    string LegalEntityRefId,
    string RefId) : IRequest<string>;

public sealed record CloseTaxBranchCommand(
    string LegalEntityRefId,
    string RefId,
    DateTimeOffset? ClosedDate = null) : IRequest<string>;

public sealed record RemoveTaxBranchCommand(
    string LegalEntityRefId,
    string RefId) : IRequest<string>;

#endregion