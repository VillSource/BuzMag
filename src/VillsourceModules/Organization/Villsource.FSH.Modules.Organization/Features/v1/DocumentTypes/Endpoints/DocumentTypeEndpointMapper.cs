using Riok.Mapperly.Abstractions;
using Villsource.FSH.Modules.Organization.Contracts.v1.DocumentTypes.Dtos;
using Villsource.FSH.Modules.Organization.Contracts.v1.DocumentTypes.Queries;

namespace Villsource.FSH.Modules.Organization.Features.v1.DocumentTypes.Endpoints;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
internal static partial class DocumentTypeEndpointMapper
{
    [MapProperty(nameof(GetDocumentTypesRequest.Module), nameof(GetDocumentTypesQuery.ModuleName))]
    internal static partial GetDocumentTypesQuery ToQuery(this GetDocumentTypesRequest request);

    [MapProperty(nameof(DocumentTypeDto.ModuleName), nameof(DocumentTypeResponse.Module))]
    internal static partial DocumentTypeResponse ToResponse(this DocumentTypeDto dto);

    internal static partial IReadOnlyList<DocumentTypeResponse> ToResponse(this IReadOnlyList<DocumentTypeDto> dtos);
}
