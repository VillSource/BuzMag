using FSH.Framework.Core.Exceptions;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Villsource.FSH.Modules.Organization.Contracts.v1.OrganizationUnits.Commands;
using Villsource.FSH.Modules.Organization.Data;

namespace Villsource.FSH.Modules.Organization.Features.v1.OrganizationUnits;

public sealed class UpdateOrganizationUnitCommandHandler(OrganizationDbContext dbContext)
    : IRequestHandler<UpdateOrganizationUnitCommand, string>
{
    public async ValueTask<string> Handle(
        UpdateOrganizationUnitCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var entity = await dbContext.OrganizationUnits
                         .FirstOrDefaultAsync(e => e.ReferenceId == request.RefId, cancellationToken)
                         .ConfigureAwait(false)
                     ?? throw new NotFoundException($"OrganizationUnit '{request.RefId}' was not found.");

        entity.Update(
            code: request.Code,
            name: request.Name,
            internationalName: request.InternationalName,
            type: request.Type,
            description: request.Description,
            costCenterCode: request.CostCenterCode);

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return entity.ReferenceId;
    }
}