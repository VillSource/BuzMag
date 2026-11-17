using FSH.Framework.Core.Context;
using FSH.Framework.Core.Exceptions;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Villsource.FSH.Modules.Organization.Contracts.v1.OrganizationUnits.Commands;
using Villsource.FSH.Modules.Organization.Data;
using Villsource.FSH.Modules.Organization.Domain.OrganizationUnits;

namespace Villsource.FSH.Modules.Organization.Features.v1.OrganizationUnits;

public sealed class DeleteOrganizationUnitCommandHandler(
    OrganizationDbContext dbContext,
    ICurrentUser currentUser)
    : IRequestHandler<DeleteOrganizationUnitCommand, string>
{
    public async ValueTask<string> Handle(
        DeleteOrganizationUnitCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var entity = await dbContext.OrganizationUnits
                         .Include(e => Enumerable.Where<OrganizationUnit>(e.Children, c => !c.IsDeleted))
                         .FirstOrDefaultAsync(e => e.ReferenceId == request.RefId, cancellationToken)
                         .ConfigureAwait(false)
                     ?? throw new NotFoundException($"OrganizationUnit '{request.RefId}' was not found.");

        entity.Delete(deletedBy: currentUser.GetUserId().ToString());

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return entity.ReferenceId;
    }
}