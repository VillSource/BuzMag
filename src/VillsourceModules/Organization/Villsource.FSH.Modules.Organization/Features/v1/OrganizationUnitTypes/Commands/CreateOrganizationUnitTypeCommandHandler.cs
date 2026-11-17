using Mediator;
using Microsoft.EntityFrameworkCore;
using Villsource.FSH.Exceptions;
using Villsource.FSH.Modules.Organization.Contracts.v1.OrganizationUnitTypes.Commands;
using Villsource.FSH.Modules.Organization.Data;
using Villsource.FSH.Modules.Organization.Domain.OrganizationUnitTypes;

namespace Villsource.FSH.Modules.Organization.Features.v1.OrganizationUnitTypes.Commands;

public sealed class CreateOrganizationUnitTypeCommandHandler(OrganizationDbContext dbContext)
    : IRequestHandler<CreateOrganizationUnitTypeCommand, string>
{
    public async ValueTask<string> Handle(
        CreateOrganizationUnitTypeCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var normalizedCode = OrganizationUnitTypeRules.NormalizeAndValidateCode(request.Code);

        if (await dbContext.OrganizationUnitTypes
                .AnyAsync(t => t.Code == normalizedCode, cancellationToken)
                .ConfigureAwait(false))
        {
            throw new ConflictException($"Organization unit type code '{normalizedCode}' already exists.");
        }

        var entity = OrganizationUnitType.Create(
            code: normalizedCode,
            name: request.Name,
            internationalName: request.InternationalName,
            hierarchyLevel: request.HierarchyLevel,
            description: request.Description,
            isActive: request.IsActive);

        await dbContext.OrganizationUnitTypes.AddAsync(entity, cancellationToken).ConfigureAwait(false);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return entity.ReferenceId;
    }
}
