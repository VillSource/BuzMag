using Mediator;
using Microsoft.EntityFrameworkCore;
using Villsource.FSH.Modules.Organization.Contracts.v1.DocumentTypes.Dtos;
using Villsource.FSH.Modules.Organization.Contracts.v1.DocumentTypes.Queries;
using Villsource.FSH.Modules.Organization.Data;

namespace Villsource.FSH.Modules.Organization.Features.v1.DocumentTypes.Queries;

public sealed class GetDocumentTypesQueryHandler(OrganizationDbContext dbContext)
    : IRequestHandler<GetDocumentTypesQuery, IReadOnlyList<DocumentTypeDto>>
{
    public async ValueTask<IReadOnlyList<DocumentTypeDto>> Handle(
        GetDocumentTypesQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var query = dbContext.DocumentTypes.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.ModuleName))
        {
            var moduleName = request.ModuleName.Trim();
            query = query.Where(d => EF.Functions.ILike(d.ModuleName, moduleName));
        }

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var keyword = request.Keyword.Trim();
            query = query.Where(d =>
                EF.Functions.ILike(d.Code, $"%{keyword}%") ||
                EF.Functions.ILike(d.Name, $"%{keyword}%") ||
                EF.Functions.ILike(d.ModuleName, $"%{keyword}%"));
        }

        return await query
            .OrderBy(d => d.ModuleName)
            .ThenBy(d => d.Code)
            .Select(d => new DocumentTypeDto(d.Code, d.Name, d.ModuleName, d.Description))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}
