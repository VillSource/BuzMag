using Mediator;
using Villsource.FSH.Modules.Organization.Contracts.v1.DocumentTypes.Dtos;

namespace Villsource.FSH.Modules.Organization.Contracts.v1.DocumentTypes.Queries;

public sealed record GetDocumentTypesQuery(
    string? ModuleName = null,
    string? Keyword = null) : IRequest<IReadOnlyList<DocumentTypeDto>>;
