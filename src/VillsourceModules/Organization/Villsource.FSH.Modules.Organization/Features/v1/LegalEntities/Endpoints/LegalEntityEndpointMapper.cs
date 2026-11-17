using FSH.Framework.Shared.Persistence;
using Riok.Mapperly.Abstractions;
using Villsource.FSH.Modules.Organization.Contracts.v1.LegalEntities.Commands;
using Villsource.FSH.Modules.Organization.Contracts.v1.LegalEntities.Dtos;
using Villsource.FSH.Modules.Organization.Contracts.v1.LegalEntities.Queries;

namespace Villsource.FSH.Modules.Organization.Features.v1.LegalEntities.Endpoints;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
internal static partial class LegalEntityEndpointMapper
{
    [MapProperty(nameof(CreateLegalEntityRequest.IntlName), nameof(CreateLegalEntityCommand.InternationalName))]
    [MapProperty(nameof(CreateLegalEntityRequest.RegNumber), nameof(CreateLegalEntityCommand.RegistrationNumber))]
    [MapProperty(nameof(CreateLegalEntityRequest.HoAddress), nameof(CreateLegalEntityCommand.HeadOfficeAddress))]
    [MapProperty(nameof(CreateLegalEntityRequest.HoBranchCode), nameof(CreateLegalEntityCommand.HeadOfficeBranchCode))]
    [MapProperty(nameof(CreateLegalEntityRequest.HoName), nameof(CreateLegalEntityCommand.HeadOfficeName))]
    [MapProperty(nameof(CreateLegalEntityRequest.HoIntlName), nameof(CreateLegalEntityCommand.HeadOfficeInternationalName))]
    [MapProperty(nameof(CreateLegalEntityRequest.HoRegisteredDate), nameof(CreateLegalEntityCommand.HeadOfficeRegisteredDate))]
    internal static partial CreateLegalEntityCommand ToCommand(this CreateLegalEntityRequest request);

    [MapProperty(nameof(UpdateLegalEntityRequest.IntlName), nameof(UpdateLegalEntityCommand.InternationalName))]
    [MapProperty(nameof(UpdateLegalEntityRequest.RegNumber), nameof(UpdateLegalEntityCommand.RegistrationNumber))]
    internal static partial UpdateLegalEntityCommand ToCommand(
        this UpdateLegalEntityRequest request,
        string refId);

    [MapProperty(nameof(UpdateLegalEntityProfileRequest.Email), nameof(UpdateLegalEntityProfileCommand.ContactEmail))]
    [MapProperty(nameof(UpdateLegalEntityProfileRequest.Phone), nameof(UpdateLegalEntityProfileCommand.ContactPhone))]
    internal static partial UpdateLegalEntityProfileCommand ToCommand(
        this UpdateLegalEntityProfileRequest request,
        string refId);

    [MapProperty(nameof(AddTaxBranchRequest.IntlName), nameof(AddTaxBranchCommand.InternationalName))]
    [MapProperty(nameof(AddTaxBranchRequest.Address), nameof(AddTaxBranchCommand.RegisteredAddress))]
    internal static partial AddTaxBranchCommand ToCommand(
        this AddTaxBranchRequest request,
        string legalEntityRefId,
        bool isHeadOffice = false);

    [MapProperty(nameof(UpdateTaxBranchRequest.IntlName), nameof(UpdateTaxBranchCommand.InternationalName))]
    [MapProperty(nameof(UpdateTaxBranchRequest.Address), nameof(UpdateTaxBranchCommand.RegisteredAddress))]
    internal static partial UpdateTaxBranchCommand ToCommand(
        this UpdateTaxBranchRequest request,
        string legalEntityRefId,
        string refId);

    [MapProperty(nameof(CompanyProfileRequest.Email), nameof(CompanyProfileDto.ContactEmail))]
    [MapProperty(nameof(CompanyProfileRequest.Phone), nameof(CompanyProfileDto.ContactPhone))]
    internal static partial CompanyProfileDto ToDto(this CompanyProfileRequest request);

    internal static partial SearchLegalEntitiesQuery ToQuery(this SearchLegalEntitiesRequest request);

    [MapProperty(nameof(LegalEntitySummaryDto.InternationalName), nameof(LegalEntitySummaryResponse.IntlName))]
    [MapProperty(nameof(LegalEntitySummaryDto.RegistrationNumber), nameof(LegalEntitySummaryResponse.RegNumber))]
    internal static partial LegalEntitySummaryResponse ToResponse(this LegalEntitySummaryDto dto);

    internal static partial PagedResponse<LegalEntitySummaryResponse> ToResponse(this PagedResponse<LegalEntitySummaryDto> page);

    [MapProperty(nameof(LegalEntityDetailDto.InternationalName), nameof(LegalEntityDetailResponse.IntlName))]
    [MapProperty(nameof(LegalEntityDetailDto.RegistrationNumber), nameof(LegalEntityDetailResponse.RegNumber))]
    internal static partial LegalEntityDetailResponse ToResponse(this LegalEntityDetailDto dto);

    [MapProperty(nameof(CompanyProfileDto.ContactEmail), nameof(CompanyProfileResponse.Email))]
    [MapProperty(nameof(CompanyProfileDto.ContactPhone), nameof(CompanyProfileResponse.Phone))]
    internal static partial CompanyProfileResponse ToResponse(this CompanyProfileDto dto);

    [MapProperty(nameof(TaxBranchDto.InternationalName), nameof(TaxBranchResponse.IntlName))]
    [MapProperty(nameof(TaxBranchDto.RegisteredAddress), nameof(TaxBranchResponse.Address))]
    internal static partial TaxBranchResponse ToResponse(this TaxBranchDto dto);
}
