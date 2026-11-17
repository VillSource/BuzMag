using FSH.Framework.Core.Exceptions;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Villsource.FSH.Exceptions;
using Villsource.FSH.Modules.Organization.Contracts.v1.OrganizationUnitTypes.Commands;
using Villsource.FSH.Modules.Organization.Data;
using Villsource.FSH.Modules.Organization.Domain.OrganizationUnitTypes;

namespace Villsource.FSH.Modules.Organization.Features.v1.OrganizationUnitTypes.Commands;

public sealed class UpdateOrganizationUnitTypeCommandHandler(OrganizationDbContext dbContext)
    : IRequestHandler<UpdateOrganizationUnitTypeCommand, string>
{
    public async ValueTask<string> Handle(
        UpdateOrganizationUnitTypeCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var entity = await dbContext.OrganizationUnitTypes
            .FirstOrDefaultAsync(t => t.ReferenceId == request.RefId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Organization unit type '{request.RefId}' was not found.");

        var normalizedCode = OrganizationUnitTypeRules.NormalizeAndValidateCode(request.Code);

        if (await dbContext.OrganizationUnitTypes
                .AnyAsync(t => t.Code == normalizedCode && t.Id != entity.Id, cancellationToken)
                .ConfigureAwait(false))
        {
            throw new ConflictException($"Organization unit type code '{normalizedCode}' already exists.");
        }

        if (entity.Update(
                code: normalizedCode,
                name: request.Name,
                internationalName: request.InternationalName,
                hierarchyLevel: request.HierarchyLevel,
                description: request.Description))
        {
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        return entity.ReferenceId;
    }
}
