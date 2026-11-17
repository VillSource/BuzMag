using Riok.Mapperly.Abstractions;
using Villsource.FSH.Modules.Organization.Contracts.v1.LegalEntities.Commands;

namespace Villsource.FSH.Modules.Organization.Features.v1.LegalEntities;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
internal static partial class LegalEntityRequestMapper
{
    internal static partial UpdateLegalEntityCommand ToCommand(
        this UpdateLegalEntityRequest request,
        string refId);

    internal static partial UpdateLegalEntityProfileCommand ToCommand(
        this UpdateLegalEntityProfileRequest request,
        string refId);

    internal static partial AddTaxBranchCommand ToCommand(
        this AddTaxBranchRequest request,
        string legalEntityRefId,
        bool isHeadOffice = false);

    internal static partial UpdateTaxBranchCommand ToCommand(
        this UpdateTaxBranchRequest request,
        string legalEntityRefId,
        string refId);
}